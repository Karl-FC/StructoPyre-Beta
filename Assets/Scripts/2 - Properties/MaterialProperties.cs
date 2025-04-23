using UnityEngine;

public enum AciElementType { Slab, Beam, Wall, ConcreteColumn, Other }
public enum AciRestraint { Restrained, Unrestrained, NotApplicable }
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

    // Input unit system for the dimension values below (Metric or Imperial)
    public UnitSystem inputUnitSystem;

    [Tooltip("Actual concrete cover (in meters, converted from input unit)")]
    public float actualCover_u = 0.0254f; // Default 1 inch converted to meters

    [Tooltip("Actual equivalent thickness for walls/slabs (in meters, converted from input unit)")]
    public float actualEquivalentThickness_te = 0.1524f; // Default 6 inches converted to meters

    [Tooltip("Actual least dimension for columns (in meters, converted from input unit)")]
    public float actualLeastDimension = 0.3048f; // Default 12 inches converted to meters

    [Tooltip("Fire exposure condition for concrete columns")]
    public AciColumnFireExposure columnFireExposure = AciColumnFireExposure.Other;

    [Tooltip("Calculated fire resistance rating (in hours) based on ACI tables")]
    public float achievedFireResistanceRating = 0.0f;
    // --- End ACI Specific Properties ---

    // --- Simulation State Tracking ---
    [HideInInspector] // Internal state, not typically set by user
    public bool isExposedToFire = false;

    [HideInInspector]
    public float exposureTimeSeconds = 0.0f;

    [HideInInspector]
    public bool hasFailed = false;
    // --- End Simulation State Tracking ---

    // Helper methods below are removed as they are not currently needed
    // or reference removed fields.
}
