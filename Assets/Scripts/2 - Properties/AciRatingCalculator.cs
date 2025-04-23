using UnityEngine;
using System.Collections.Generic;
using System;

// Static class to handle ACI 216.1-14 calculations for fire resistance rating.
// Focused on non-prestressed concrete elements.
public static class AciRatingCalculator
{
    // --- ACI 216.1M-14 Data --- 

    // Table 4.2: Minimum Equivalent Thickness (te) in mm for Walls, Floors, Roofs
    // Key: Aggregate Category
    // Value: Dictionary<float (Rating in hours), float (Min te in mm)>
    private static readonly Dictionary<AciAggregateCategory, Dictionary<float, float>> Table4_2_EquivalentThickness = new Dictionary<AciAggregateCategory, Dictionary<float, float>>
    {
        { AciAggregateCategory.Siliceous, new Dictionary<float, float> { { 1f, 90f }, { 1.5f, 105f }, { 2f, 125f }, { 3f, 155f }, { 4f, 180f } } },
        { AciAggregateCategory.Carbonate, new Dictionary<float, float> { { 1f, 85f }, { 1.5f, 100f }, { 2f, 110f }, { 3f, 135f }, { 4f, 155f } } },
        { AciAggregateCategory.SemiLightweight, new Dictionary<float, float> { { 1f, 65f }, { 1.5f, 80f }, { 2f, 90f }, { 3f, 110f }, { 4f, 125f } } },
        { AciAggregateCategory.Lightweight, new Dictionary<float, float> { { 1f, 60f }, { 1.5f, 70f }, { 2f, 80f }, { 3f, 95f }, { 4f, 110f } } }
        // Note: Unknown category should be handled explicitly
    };

    // --- Main Calculation Function ---
    public static float CalculateRating(MaterialProperties props)
    {
        if (props == null || props.realMaterial == null)
        {
            Debug.LogError("Cannot calculate rating: MaterialProperties or AggregateType is null.");
            return 0f;
        }

        AciAggregateCategory aggregate = props.realMaterial.aggregateCategory;
        if (aggregate == AciAggregateCategory.Unknown)
        {
            Debug.LogWarning($"Cannot calculate rating for {props.gameObject.name}: Aggregate Category is Unknown.");
            return 0f;
        }

        // Convert actual dimensions from meters (as stored) to millimeters for table lookups
        float actualTe_mm = props.actualEquivalentThickness_te * 1000f;
        float actualCover_mm = props.actualCover_u * 1000f;
        float actualLeastDim_mm = props.actualLeastDimension * 1000f;

        switch (props.elementType)
        {
            case AciElementType.Slab:
            case AciElementType.Wall:
                // Primarily governed by thickness (Table 4.2)
                // TODO: Potentially consider cover for slabs (Table 4.3.1.1) if needed for more detailed analysis?
                // For now, using Table 4.2 for simplicity as it covers walls/floors/roofs
                return CalculateRatingFromThickness(aggregate, actualTe_mm);

            case AciElementType.Beam:
                // Governed by cover (Table 4.3.1.2 - Nonprestressed) and potentially width
                // TODO: Implement Beam calculation (requires beam width)
                Debug.LogWarning($"Beam calculation not yet implemented for {props.gameObject.name}. Returning 0.");
                return 0f; // Placeholder

            case AciElementType.ConcreteColumn:
                // Governed by least dimension (Table 4.5.1a or 4.5.1b)
                // TODO: Implement Column calculation (requires exposure condition)
                Debug.LogWarning($"Column calculation not yet implemented for {props.gameObject.name}. Returning 0.");
                return 0f; // Placeholder

            case AciElementType.Other:
            default:
                Debug.LogWarning($"Cannot calculate rating for element type {props.elementType} on {props.gameObject.name}.");
                return 0f;
        }
    }

    // --- Helper Calculation Functions ---

    private static float CalculateRatingFromThickness(AciAggregateCategory aggregate, float actualTe_mm)
    {
        if (!Table4_2_EquivalentThickness.ContainsKey(aggregate))
        {
            Debug.LogError($"Aggregate type {aggregate} not found in Table 4.2 data.");
            return 0f;
        }

        var thicknessTable = Table4_2_EquivalentThickness[aggregate];
        return InterpolateRating(thicknessTable, actualTe_mm);
    }

    // TODO: Add CalculateRatingFromCover (for Slabs/Beams)
    // TODO: Add CalculateRatingFromLeastDimension (for Columns)

    // --- Interpolation Logic ---
    private static float InterpolateRating(Dictionary<float, float> tableData, float actualValue_mm)
    {
        // Sort the table ratings (keys) to ensure proper interpolation order
        var sortedRatings = new List<float>(tableData.Keys);
        sortedRatings.Sort();

        float lowerRating = 0f;
        float upperRating = 0f;
        float lowerValue = 0f;
        float upperValue = 0f;

        // Find the rating bounds for the actual value
        for (int i = 0; i < sortedRatings.Count; i++)
        {
            float currentRating = sortedRatings[i];
            float currentValue = tableData[currentRating];

            if (actualValue_mm <= currentValue)
            {
                upperRating = currentRating;
                upperValue = currentValue;
                if (i > 0)
                {
                    lowerRating = sortedRatings[i - 1];
                    lowerValue = tableData[lowerRating];
                }
                else // Value is below the lowest rating's requirement
                {
                    lowerRating = 0f; // Assuming 0 rating for 0 thickness
                    lowerValue = 0f;
                }
                break; // Found the upper bound
            }
            // If loop finishes, value is above the highest rating's requirement
            if (i == sortedRatings.Count - 1)
            {
                lowerRating = currentRating;
                lowerValue = currentValue;
                upperRating = currentRating; // Cap at max rating
                upperValue = currentValue;
            }
        }

        // Perform linear interpolation
        if (upperValue <= lowerValue) // Avoid division by zero or negative range (handles capping)
        {
            // If actual value exactly matches a table value or is below the min/above max
            if (actualValue_mm >= upperValue)
                return upperRating; // Met or exceeded requirement for upper rating
            else // Below requirement for the lowest rating (or lowest = 0)
                return lowerRating; // Return the lower bound rating (potentially 0)
        }

        // Interpolate: Rating = R1 + (Actual - Val1) / (Val2 - Val1) * (R2 - R1)
        float interpolatedRating = lowerRating + (actualValue_mm - lowerValue) / (upperValue - lowerValue) * (upperRating - lowerRating);
        
        // Clamp rating to reasonable bounds (e.g., 0 to max rating in table?)
        // Let's use the max rating found in the specific aggregate table
        float maxRatingInTable = sortedRatings[sortedRatings.Count - 1];
        return Mathf.Clamp(interpolatedRating, 0f, maxRatingInTable); 
    }
} 