using UnityEngine;

[RequireComponent(typeof(MaterialProperties))]
public class FireIntegrityTracker : MonoBehaviour
{
    private MaterialProperties materialProperties;
    private SimulationManager simulationManager;

    // Threshold state for visualization (optional, used by visualizer)
    public enum IntegrityState { Healthy, Exposed, Failed }
    public IntegrityState CurrentState { get; private set; } = IntegrityState.Healthy;

    void Awake()
    {
        materialProperties = GetComponent<MaterialProperties>();
        
        // Find the SimulationManager instance in the scene. 
        // Consider a more robust method like a singleton or dependency injection for larger projects.
        simulationManager = FindObjectOfType<SimulationManager>(); 
        if (simulationManager == null)
        {
            Debug.LogError("FireIntegrityTracker could not find the SimulationManager in the scene!", this);
            this.enabled = false; // Disable component if manager is missing
        }

        // We assume materialProperties.achievedFireResistanceRating has been pre-calculated
        // by another script (e.g., after user edits properties or on scene load).
        if (materialProperties.achievedFireResistanceRating <= 0)
        {
             Debug.LogWarning($"Material {gameObject.name} has an invalid fire rating ({materialProperties.achievedFireResistanceRating} hours). It will not fail based on time.", this);
        }
    }

    void Update()
    {
        if (simulationManager == null || materialProperties == null) return;

        // Only update exposure time and check for failure if the simulation is running,
        // the element is exposed, and it hasn't already failed.
        if (simulationManager.currentState == SimulationManager.SimulationState.Running && 
            materialProperties.isExposedToFire && 
            !materialProperties.hasFailed)
        {
            // Check if the rating is valid before proceeding
            if (materialProperties.achievedFireResistanceRating > 0)
            {
                materialProperties.exposureTimeSeconds += Time.deltaTime;
                float failureTimeSeconds = materialProperties.achievedFireResistanceRating * 3600f;

                if (materialProperties.exposureTimeSeconds >= failureTimeSeconds)
                {
                    materialProperties.hasFailed = true;
                    Debug.Log($"{gameObject.name} failed at {simulationManager.simulationTimeSeconds:F2} simulation seconds (Rating: {materialProperties.achievedFireResistanceRating} hrs).");
                }
            }
        }

        // Update the visual state (used by IntegrityVisualizer)
        UpdateCurrentState();
    }
    
    private void UpdateCurrentState()
    {
         if (materialProperties.hasFailed)
         {
            CurrentState = IntegrityState.Failed;
         }
         else if (materialProperties.isExposedToFire)
         {
             CurrentState = IntegrityState.Exposed;
         }
         else
         {
             CurrentState = IntegrityState.Healthy;
         }
    }

    // Called by SimulationManager when the simulation is reset
    public void ResetState()
    {
        if (materialProperties != null)
        {
            materialProperties.isExposedToFire = false;
            materialProperties.exposureTimeSeconds = 0f;
            materialProperties.hasFailed = false;
        }
        CurrentState = IntegrityState.Healthy; // Reset visual state as well
        // Debug.Log($"Resetting state for {gameObject.name}");
    }
} 