using UnityEngine;

[CreateAssetMenu(fileName = "NewAggregateType", menuName = "Simulation/Real World Material")]
public class AggregateType : ScriptableObject
{
    // Add properties needed for simulation
    public string realmaterialName = "Unnamed"; // User-friendly name
    public float defaultThickness = 0.1f; // Example property
    public float flammabilityRating = 0.5f; // Example property
    // Add other properties like conductivity, density, etc.
}
