using UnityEngine;

// Ensure this script is saved as "MaterialProperties.cs"
public enum AciElementType { Slab, Beam, Wall, ConcreteColumn, ProtectedSteelColumn, Other }
public enum AciRestraint { Restrained, Unrestrained, NotApplicable }
public enum AciPrestress { Prestressed, Nonprestressed, NotApplicable }
public enum AciColumnFireExposure { TwoParallelSides, FourSides, Other }

public enum UnitSystem { Metric, Imperial }

public class MaterialProperties : MonoBehaviour
{
    // This field will hold the reference to the ScriptableObject asset (AggregateType)
    // assigned after the user confirms the mapping in the UI.
    public AggregateType realMaterial; // Make sure the AggregateType class is compiled

    // --- ACI Specific Properties ---
    public AciElementType elementType = AciElementType.Other;

    public AciRestraint restraint = AciRestraint.NotApplicable;

    public AciPrestress prestress = AciPrestress.NotApplicable;

    // Input unit system for the dimension values below (Metric or Imperial)
    public UnitSystem inputUnitSystem;

    [Tooltip("Actual concrete cover (in meters, converted from input unit)")]
    public float actualCover_u = 0.0254f; // Default 1 inch converted to meters

    [Tooltip("Actual equivalent thickness for walls/slabs (in meters, converted from input unit)")]
    public float actualEquivalentThickness_te = 0.1524f; // Default 6 inches converted to meters

    [Tooltip("Actual least dimension for columns (in meters, converted from input unit)")]
    public float actualLeastDimension = 0.3048f; // Default 12 inches converted to meters

    [Tooltip("For protected steel columns (e.g., \"W10x45\")")]
    public string steelShape = "";

    [Tooltip("Actual protection thickness for protected steel columns (in meters, converted from input unit)")]
    public float actualProtectionThickness_h = 0.0508f; // Default 2 inches converted to meters

    [Tooltip("Material used for protection of steel columns")]
    public AggregateType protectionMaterial;

    [Tooltip("Fire exposure condition for concrete columns")]
    public AciColumnFireExposure columnFireExposure = AciColumnFireExposure.Other;

    [Tooltip("Calculated fire resistance rating (in hours) based on ACI tables")]
    public float achievedFireResistanceRating = 0.0f;
    // --- End ACI Specific Properties ---

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
