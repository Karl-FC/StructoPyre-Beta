using UnityEngine;

// Ensure this script is saved as "MaterialProperties.cs"
public class MaterialProperties : MonoBehaviour
{
    // This field will hold the reference to the ScriptableObject asset (AggregateType)
    // assigned after the user confirms the mapping in the UI.
    public AggregateType realMaterial; // Make sure the AggregateType class is compiled

    // Example helper method to get thickness from the assigned ScriptableObject
    // Returns a default value if no material is assigned.
    public float GetThickness()
    {
        // Use the null-conditional operator ?. for brevity
        return realMaterial?.defaultThickness ?? 0.1f; // Default to 0.1 if null
    }

    // Example helper method to get flammability
    public float GetFlammability()
    {
         return realMaterial?.flammabilityRating ?? 0.0f; // Default to 0 if null
    }

    // Add other helper methods here to easily access properties
    // from the 'realMaterial' ScriptableObject for your simulation logic.
    // For example:
    // public string GetMaterialName() => realMaterial?.realmaterialName ?? "Unknown";
    // public float GetDensity() => realMaterial?.density ?? 1000f; // Example with default
    // public float GetThermalConductivity() => realMaterial?.thermalConductivity ?? 1.0f; // Example
}
