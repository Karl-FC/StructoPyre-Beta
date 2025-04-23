using UnityEngine;
// Removed System.Collections.Generic and System namespaces as they are no longer needed here
// using System.Collections.Generic; 
// using System; 

// Static class to handle ACI 216.1-14 calculations for fire resistance rating.
// Uses FireResistanceCalculator from DataArray.cs for the core logic.
public static class AciRatingCalculator
{
    // --- Data and Interpolation moved to FireResistanceCalculator in DataArray.cs ---
    // private static readonly Dictionary<AciAggregateCategory, Dictionary<float, float>> Table4_2_EquivalentThickness = ... (Removed)

    // Keep a single instance of the calculator ready.
    private static readonly FireResistanceCalculator fireResistanceCalculator = new FireResistanceCalculator();

    // --- Main Calculation Function ---
    // This function now acts as a wrapper to use the FireResistanceCalculator
    public static float CalculateRating(MaterialProperties props)
    {
        if (props == null || props.realMaterial == null)
        {
            Debug.LogError("Cannot calculate rating: MaterialProperties or its AggregateType (realMaterial) is null.");
            return 0f;
        }

        AciAggregateCategory aggregate = props.realMaterial.aggregateCategory;
        if (aggregate == AciAggregateCategory.Unknown)
        {
            Debug.LogWarning($"Cannot calculate rating for {props.gameObject.name}: Aggregate Category is Unknown.");
            return 0f;
        }

        // Get thickness from MaterialProperties (assumed to be in meters)
        float actualTe_meters = props.actualEquivalentThickness_te;

        // TODO: Respect props.inputUnitSystem if it's ever implemented for thickness input.
        // Assuming actualEquivalentThickness_te is ALWAYS stored in METERS for now.
        float actualTe_mm = actualTe_meters * 1000f;

        // Use the centralized calculator instance
        // Note: LinearInterpolateCalc handles edge cases (thickness outside range) internally.
        double rating = fireResistanceCalculator.LinearInterpolateCalc(actualTe_mm, aggregate);
        
        // Handle potential NaN result from LinearInterpolateCalc if data was missing (shouldn't happen with current setup)
        if (double.IsNaN(rating))
        {
            Debug.LogError($"Calculation returned NaN for {props.gameObject.name}. Thickness: {actualTe_mm}mm, Aggregate: {aggregate}");
            return 0f;
        }

        return (float)rating; // Cast result to float

        // --- Removed the old switch statement and helper/interpolation functions ---
        // switch (props.elementType) { ... } (Removed)
        // private static float CalculateRatingFromThickness(...) { ... } (Removed)
        // private static float InterpolateRating(...) { ... } (Removed)
    }
} 