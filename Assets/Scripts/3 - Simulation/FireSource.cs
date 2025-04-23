using UnityEngine;
using System.Collections.Generic;

public class FireSource : MonoBehaviour
{
    [Header("Exposure Settings")]
    [Tooltip("The radius around the fire source within which objects will be considered exposed.")]
    public float exposureRadius = 5f;

    [Tooltip("The layer mask defining which layers contain the structural elements to affect.")]
    public LayerMask structuralLayerMask;

    [Tooltip("How often (in seconds) to check for nearby objects. Lower values are more responsive but less performant.")]
    public float checkInterval = 0.5f;

    private SimulationManager simulationManager;
    private float elapsedTimeSinceCheck = 0f;

    // Optional: Keep track of exposed objects to potentially unexpose them if fire stops/pauses without a full reset.
    // private HashSet<MaterialProperties> currentlyExposed = new HashSet<MaterialProperties>();

    void Start()
    {
        // Use the singleton instance for more reliable access
        simulationManager = SimulationManager.Instance;
        if (simulationManager == null)
        {
            Debug.LogError("FireSource could not find the SimulationManager.Instance!", this);
            this.enabled = false; // Disable component if manager is missing
        }

        // Ensure the layer mask is assigned in the inspector
        if (structuralLayerMask.value == 0) // LayerMask value is 0 if nothing is selected
        {
             Debug.LogWarning("FireSource has no Structural Layer Mask assigned. It might not detect any objects.", this);
        }
    }

    void Update()
    {
        if (simulationManager == null) return;

        // Only perform checks if the simulation is running
        if (simulationManager.currentState == SimulationManager.SimulationState.Running)
        {
            // Use the real deltaTime for the check interval as fire spreading is a visual element
            // and shouldn't be tied to simulation speed
            elapsedTimeSinceCheck += Time.deltaTime;

            if (elapsedTimeSinceCheck >= checkInterval)
            {
                ApplyFireExposure();
                elapsedTimeSinceCheck = 0f; // Reset timer
            }
        }
        // else
        // {
            // Optional: Handle pausing/stopping - clear exposure or rely on Tracker's ResetState
            // ClearExposure(); 
        // }
    }

    void ApplyFireExposure()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, exposureRadius, structuralLayerMask);
        
        // Optional: Clear the list if we need to track who *stops* being exposed
        // currentlyExposed.Clear(); 

        foreach (var hitCollider in hitColliders)
        {
            MaterialProperties props = hitCollider.GetComponent<MaterialProperties>();
            if (props != null)
            {
                // Mark the object as exposed if it wasn't already
                // No need for this check if FireIntegrityTracker handles the logic robustly
                // if (!props.isExposedToFire)
                // {
                     props.isExposedToFire = true;
                // }
                
                // Optional: Add to tracking list
                // currentlyExposed.Add(props);
            }
        }
    }

    // --- Optional Gizmo --- 
    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere in the editor to visualize the exposure radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, exposureRadius);
    }

    /* // Optional: Function to clear exposure if needed when fire stops/pauses
    void ClearExposure()
    {
        foreach (var props in currentlyExposed)
        {
            if (props != null) // Check if object still exists
            {
                props.isExposedToFire = false;
            }
        }
        currentlyExposed.Clear();
        elapsedTimeSinceCheck = 0f; // Reset check timer immediately when stopped/paused
    }
    */
} 