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
        AciAggregateCategory aggCategory = props.realMaterial.aggregateCategory;

        // Get Beam Width (using least dimension as proxy for now)
        float beamWidth_mm = actualLeastDimension_mm; // <<< Using least dimension proxy

        switch (props.elementType)
        {
            case AciElementType.Slab:
                // Could use WallSlab thickness OR specific Slab cover check. Thickness is simpler for now.
                // If implementing Slab Cover (Table 4.3.1.1), it would look similar to Beam Cover logic.
                return CalculateRating_WallSlab_Thickness(actualEquivalentThickness_te_mm, aggCategory);
            case AciElementType.Wall:
                return CalculateRating_WallSlab_Thickness(actualEquivalentThickness_te_mm, aggCategory);

            case AciElementType.Beam:
                // Using Nonprestressed Beam Cover check (Table 4.3.1.2)
                // Passing aggregate category, although this specific table doesn't use it directly.
                return CalculateRating_Beam_Cover_Nonprestressed(actualCover_u_mm, beamWidth_mm, props.restraint, aggCategory);

            case AciElementType.ConcreteColumn:
                 return CalculateRating_ConcreteColumn_Dimension(actualLeastDimension_mm, aggCategory, props.columnFireExposure);

            case AciElementType.Other:
            default:
                Debug.LogWarning($"Rating calculation not explicitly implemented for element type: {props.elementType}. Returning 0.");
                return 0f;
        }
    }

    // --- Specific Calculation Methods ---

    // Based on ACI 216.1M-14 Table 4.2 - Equivalent Thickness for Walls/Slabs/Roofs
    private static float CalculateRating_WallSlab_Thickness(float actualTe_mm, AciAggregateCategory aggCategory)
    {
        // Table 4.2 Data (Thickness in mm for ratings 1, 1.5, 2, 3, 4 hours)
        float[] ratings = { 1f, 1.5f, 2f, 3f, 4f };
        float[] reqTe_Siliceous = { 90f, 105f, 125f, 155f, 180f };
        float[] reqTe_Carbonate = { 80f, 95f, 105f, 130f, 150f };
        // Assuming "Semi-lightweight" maps to sand-lightweight values from full table
        float[] reqTe_SemiLight = { 65f, 80f, 90f, 110f, 125f }; 
        float[] reqTe_Lightweight = { 55f, 65f, 75f, 90f, 105f };

        float[] reqTe;
        switch (aggCategory)
        {
            case AciAggregateCategory.Siliceous: reqTe = reqTe_Siliceous; break;
            case AciAggregateCategory.Carbonate: reqTe = reqTe_Carbonate; break;
            case AciAggregateCategory.SemiLightweight: reqTe = reqTe_SemiLight; break;
            case AciAggregateCategory.Lightweight: reqTe = reqTe_Lightweight; break;
            default:
                Debug.LogWarning($"CalculateRating_WallSlab_Thickness: Unknown aggregate category: {aggCategory}");
                return 0f;
        }
        return InterpolateRating(actualTe_mm, reqTe, ratings);
    }

    // Based on ACI 216.1M-14 Table 4.3.1.2 - Cover for Nonprestressed Beams
    private static float CalculateRating_Beam_Cover_Nonprestressed(float actualCover_mm, float beamWidth_mm, AciRestraint restraint, AciAggregateCategory aggCategory) // Added aggCategory for potential future use
    {
        // Table 4.3.1.2 Data (Cover in mm for ratings 1, 1.5, 2, 3, 4 hours)
        // Ratings are the same for all rows
        float[] ratings = { 1f, 1.5f, 2f, 3f, 4f };

        // Required Cover (mm) Arrays based on Width (W) and Restraint (R)
        // W < 180mm
        float[] reqCover_WLess180_RRestrained   = { 20f, 20f, 20f, 20f, 20f };
        float[] reqCover_WLess180_RUnrestrained = { 20f, 20f, 25f, 30f, 35f }; // From excerpt
        // 180mm <= W < 250mm
        float[] reqCover_W180_250_RRestrained   = { 20f, 20f, 20f, 20f, 20f };
        float[] reqCover_W180_250_RUnrestrained = { 20f, 20f, 20f, 25f, 30f }; // From excerpt
        // W >= 250mm
        float[] reqCover_WOver250_RRestrained   = { 20f, 20f, 20f, 20f, 20f };
        float[] reqCover_WOver250_RUnrestrained = { 20f, 20f, 20f, 20f, 25f }; // From excerpt

        float[] reqCover;

        // Select cover array based on width and restraint
        if (beamWidth_mm < 180f)
        {
            reqCover = (restraint == AciRestraint.Restrained) ? reqCover_WLess180_RRestrained : reqCover_WLess180_RUnrestrained;
        }
        else if (beamWidth_mm < 250f)
        {
             reqCover = (restraint == AciRestraint.Restrained) ? reqCover_W180_250_RRestrained : reqCover_W180_250_RUnrestrained;
        }
        else // beamWidth_mm >= 250f
        {
            reqCover = (restraint == AciRestraint.Restrained) ? reqCover_WOver250_RRestrained : reqCover_WOver250_RUnrestrained;
        }

        if (restraint == AciRestraint.NotApplicable)
        {
            Debug.LogWarning("CalculateRating_Beam_Cover_Nonprestressed: Restraint is NotApplicable. Cannot determine rating. Returning 0.");
            return 0f;
        }

        // Use interpolation helper
        return InterpolateRating(actualCover_mm, reqCover, ratings);
    }


    // Based on ACI 216.1M-14 Table 4.5.1a/b - Least Dimension for Concrete Columns
    private static float CalculateRating_ConcreteColumn_Dimension(float actualDim_mm, AciAggregateCategory aggCategory, AciColumnFireExposure exposure)
    {
        // Table 4.5.1a/b Data (Least Dimension in mm for ratings 1, 1.5, 2, 3, 4 hours)
        float[] ratings = { 1f, 1.5f, 2f, 3f, 4f };
        float[] reqDim_Siliceous = { 200f, 230f, 250f, 300f, 350f };
        float[] reqDim_Carbonate = { 200f, 230f, 250f, 300f, 350f }; // Same as Siliceous in table
        float[] reqDim_SemiLight = { 180f, 200f, 215f, 250f, 300f };
        float[] reqDim_Lightweight = { 165f, 190f, 215f, 250f, 300f }; // Sand-Lightweight values used

        if (exposure == AciColumnFireExposure.Other)
        {
             Debug.LogWarning("CalculateRating_ConcreteColumn_Dimension: Column fire exposure 'Other' not specifically handled by Tables 4.5.1a/b, using FourSides/Table 4.5.1a data.");
        }

        float[] reqDim;
        switch (aggCategory)
        {
            case AciAggregateCategory.Siliceous: reqDim = reqDim_Siliceous; break;
            case AciAggregateCategory.Carbonate: reqDim = reqDim_Carbonate; break;
            case AciAggregateCategory.SemiLightweight: reqDim = reqDim_SemiLight; break;
            case AciAggregateCategory.Lightweight: reqDim = reqDim_Lightweight; break;
            default:
                Debug.LogWarning($"CalculateRating_ConcreteColumn_Dimension: Unknown aggregate category: {aggCategory}");
                return 0f;
        }
        return InterpolateRating(actualDim_mm, reqDim, ratings);
    }


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