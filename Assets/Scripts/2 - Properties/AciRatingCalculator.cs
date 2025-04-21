using UnityEngine;
using System; // For Math.Max/Min

// Static class to calculate achieved fire resistance ratings based on ACI 216.1M-14
public static class AciRatingCalculator
{
    private const float METERS_TO_MM = 1000f;

    // Main entry point - determines element type and calls specific calculation
    public static float CalculateRating(MaterialProperties props)
    {
        if (props == null || props.realMaterial == null)
        {
            Debug.LogWarning("Cannot calculate rating: MaterialProperties or realMaterial is null.");
            return 0f;
        }

        // Convert dimensions from meters (stored) to mm (for ACI tables)
        float actualCover_u_mm = props.actualCover_u * METERS_TO_MM;
        float actualEquivalentThickness_te_mm = props.actualEquivalentThickness_te * METERS_TO_MM;
        float actualLeastDimension_mm = props.actualLeastDimension * METERS_TO_MM;
        float actualProtectionThickness_h_mm = props.actualProtectionThickness_h * METERS_TO_MM;
        AciAggregateCategory aggCategory = props.realMaterial.aggregateCategory;
        AciAggregateCategory protectionAggCategory = props.protectionMaterial?.aggregateCategory ?? AciAggregateCategory.Unknown; // Handle null protectionMaterial

        // TODO: Get Beam Width and Steel Area if needed, potentially from user input or defaults

        switch (props.elementType)
        {
            case AciElementType.Slab:
            case AciElementType.Wall: // Treat walls and slabs similarly for thickness check
                // Primarily use thickness (Table 4.2), but could also check cover (Table 4.3.1.1) if relevant for slabs
                return CalculateRating_WallSlab_Thickness(actualEquivalentThickness_te_mm, aggCategory);

            // case AciElementType.Beam: // Needs further split for prestressed/nonprestressed
            //     return CalculateRating_Beam_Cover(actualCover_u_mm, props.restraint, props.prestress, aggCategory /*, beamWidth, steelArea*/);

            case AciElementType.ConcreteColumn:
                 return CalculateRating_ConcreteColumn_Dimension(actualLeastDimension_mm, aggCategory, props.columnFireExposure);

            // case AciElementType.ProtectedSteelColumn:
            //     return CalculateRating_ProtectedSteelColumn_Thickness(actualProtectionThickness_h_mm, protectionAggCategory, props.steelShape /*, config*/);

            case AciElementType.Other:
            default:
                Debug.LogWarning($"Rating calculation not implemented for element type: {props.elementType}");
                return 0f; // No rating calculated for 'Other' or unhandled types
        }
    }

    // --- Specific Calculation Methods ---

    // Based on ACI 216.1M-14 Table 4.2 - Equivalent Thickness for Walls/Slabs/Roofs
    private static float CalculateRating_WallSlab_Thickness(float actualTe_mm, AciAggregateCategory aggCategory)
    {
        // Simplified Table 4.2 Data (Thickness in mm for ratings 1, 1.5, 2, 3, 4 hours)
        // Add more rows if needed (e.g., sand-lightweight)
        float[] ratings = { 1f, 1.5f, 2f, 3f, 4f };
        float[] reqTe_Siliceous = { 90f, 105f, 125f, 155f, 180f };
        float[] reqTe_Carbonate = { 80f, 95f, 105f, 130f, 150f };
        float[] reqTe_SemiLight = { 65f, 80f, 90f, 110f, 125f }; // Assuming "Semi-lightweight" maps here
        float[] reqTe_Lightweight = { 55f, 65f, 75f, 90f, 105f };

        float[] reqTe;
        switch (aggCategory)
        {
            case AciAggregateCategory.Siliceous: reqTe = reqTe_Siliceous; break;
            case AciAggregateCategory.Carbonate: reqTe = reqTe_Carbonate; break;
            case AciAggregateCategory.SemiLightweight: reqTe = reqTe_SemiLight; break;
            case AciAggregateCategory.Lightweight: reqTe = reqTe_Lightweight; break;
            default:
                Debug.LogWarning($"Unknown aggregate category for Table 4.2 lookup: {aggCategory}");
                return 0f;
        }

        return InterpolateRating(actualTe_mm, reqTe, ratings);
    }

    // TODO: Implement CalculateRating_Beam_Cover (using Tables 4.3.1.1, 4.3.1.2, 4.3.1.3a/b)
    // This will be more complex due to restraint, prestressing, beam width/area factors.

    // Based on ACI 216.1M-14 Table 4.5.1a/b - Least Dimension for Concrete Columns
    private static float CalculateRating_ConcreteColumn_Dimension(float actualDim_mm, AciAggregateCategory aggCategory, AciColumnFireExposure exposure)
    {
        // Simplified Table 4.5.1a/b Data (Least Dimension in mm for ratings 1, 1.5, 2, 3, 4 hours)
        // Note: Table 4.5.1b (Parallel Sides) has same values as 4.5.1a in the standard excerpt
        float[] ratings = { 1f, 1.5f, 2f, 3f, 4f };
        float[] reqDim_Siliceous = { 200f, 230f, 250f, 300f, 350f };
        float[] reqDim_Carbonate = { 200f, 230f, 250f, 300f, 350f }; // Same as Siliceous in table
        float[] reqDim_SemiLight = { 180f, 200f, 215f, 250f, 300f };
        float[] reqDim_Lightweight = { 165f, 190f, 215f, 250f, 300f }; // Sand-Lightweight values used

        // For this table, exposure doesn't change the values based on the provided ACI excerpt tables 4.5.1a/b
        // If more detailed tables distinguish, add logic here.
        if (exposure == AciColumnFireExposure.Other)
        {
             Debug.LogWarning("Column fire exposure 'Other' not specifically handled by Tables 4.5.1a/b, using FourSides data.");
        }

        float[] reqDim;
        switch (aggCategory)
        {
            case AciAggregateCategory.Siliceous: reqDim = reqDim_Siliceous; break;
            case AciAggregateCategory.Carbonate: reqDim = reqDim_Carbonate; break;
            case AciAggregateCategory.SemiLightweight: reqDim = reqDim_SemiLight; break; // Mapping SemiLightweight
            case AciAggregateCategory.Lightweight: reqDim = reqDim_Lightweight; break; // Mapping Lightweight
            default:
                Debug.LogWarning($"Unknown aggregate category for Table 4.5.1 lookup: {aggCategory}");
                return 0f;
        }

        return InterpolateRating(actualDim_mm, reqDim, ratings);
    }


    // TODO: Implement CalculateRating_ProtectedSteelColumn_Thickness (using Tables 4.6a-d)
    // Requires steel shape, protection material, and configuration details.


    // --- Helper Methods ---

    // Linear interpolation to find rating based on actual dimension and required dimensions for known ratings
    private static float InterpolateRating(float actualValue, float[] requiredValues, float[] correspondingRatings)
    {
        if (requiredValues.Length != correspondingRatings.Length || requiredValues.Length == 0)
        {
            Debug.LogError("Interpolation error: Input arrays mismatch or empty.");
            return 0f;
        }

        // Check boundaries
        if (actualValue < requiredValues[0])
        {
            // Extrapolate downwards (or simply return 0 or a minimum rating like 0.5h?)
            // For simplicity, let's return a proportional value below the first point, capped at 0.
             if (requiredValues[0] <= 0) return 0f; // Avoid division by zero
             float extrapolated = correspondingRatings[0] * (actualValue / requiredValues[0]);
             return Math.Max(0f, extrapolated);
           // return 0f; // Simplest approach: Rating is 0 if below the first threshold
        }
        if (actualValue >= requiredValues[requiredValues.Length - 1])
        {
            // Exceeds highest requirement, return highest rating (or extrapolate upwards?)
            return correspondingRatings[correspondingRatings.Length - 1];
        }

        // Find the interval
        for (int i = 0; i < requiredValues.Length - 1; i++)
        {
            if (actualValue >= requiredValues[i] && actualValue < requiredValues[i + 1])
            {
                // Interpolate: Rating = R1 + (Actual - Req1) / (Req2 - Req1) * (R2 - R1)
                float req1 = requiredValues[i];
                float req2 = requiredValues[i + 1];
                float r1 = correspondingRatings[i];
                float r2 = correspondingRatings[i + 1];

                if (req2 - req1 == 0) // Avoid division by zero if table values are identical
                {
                     return r1; // Or r2, they should be the same if reqs are same
                }

                float rating = r1 + (actualValue - req1) / (req2 - req1) * (r2 - r1);
                return rating;
            }
        }

        Debug.LogError("Interpolation failed: Value did not fall within expected range.");
        return 0f; // Should not happen if boundaries are checked
    }
} 