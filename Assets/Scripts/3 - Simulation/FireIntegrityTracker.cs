using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MaterialProperties))]
public class FireIntegrityTracker : MonoBehaviour
{
    private MaterialProperties materialProperties;
    private SimulationManager simulationManager;

    // Threshold state for visualization (optional, used by visualizer)
    public enum IntegrityState { Healthy, Exposed, Failed }
    public IntegrityState CurrentState { get; private set; } = IntegrityState.Healthy;

    [Header("Fire Spreading")]
    [Tooltip("Whether this object can spread fire to nearby objects")]
    public bool canSpreadFire = true;
    [Tooltip("Radius within which this object can spread fire when partially burning")]
    public float spreadRadius = 3.0f; // Increased default radius
    [Tooltip("How much exposure time (seconds) before this object starts spreading fire")]
    public float spreadThresholdSeconds = 30f; // Default to 30 seconds for faster testing
    [Tooltip("How frequently to check for spreading (in seconds)")]
    public float spreadCheckInterval = 3f; // Check more frequently
    [Tooltip("Layer mask for objects that can catch fire")]
    public LayerMask spreadableLayerMask;
    
    [Header("Debug Options")]
    [Tooltip("Show debug visualization lines to nearby objects")]
    public bool showDebugLines = false; // Turned off by default for better performance
    [Tooltip("How long debug lines remain visible (seconds)")]
    public float debugLineDuration = 2f; // Reduced for performance
    [Tooltip("Enable detailed debug logging")]
    public bool enableDebugLogging = false; // Turned off by default
    [Tooltip("Override the detection center (leave empty to use automatic detection)")]
    public Transform detectionCenterOverride;

    private float timeSinceLastSpreadCheck = 0f;
    private bool isCurrentlySpreading = false;
    private List<GameObject> nearbyObjects = new List<GameObject>();

    // Debug variables
    private Vector3 lastDetectionPosition;
    
    void Start()
    {
        materialProperties = GetComponent<MaterialProperties>();
        
        // Use the singleton instance for more reliable access
        simulationManager = SimulationManager.Instance;
        if (simulationManager == null)
        {
            Debug.LogError("FireIntegrityTracker could not find the SimulationManager.Instance!", this);
            this.enabled = false; // Disable component if manager is missing
        }

        // We assume materialProperties.achievedFireResistanceRating has been pre-calculated
        // by another script (e.g., after user edits properties or on scene load).
        if (materialProperties.achievedFireResistanceRating <= 0)
        {
             Debug.LogWarning($"Material {gameObject.name} has an invalid fire rating ({materialProperties.achievedFireResistanceRating} hours). It will not fail based on time.", this);
        }
        
        // Set default layer mask if not set
        if (spreadableLayerMask.value == 0)
        {
            spreadableLayerMask = LayerMask.GetMask("InspectableModel");
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
                // Use the simulation-specific delta time instead of the global Time.deltaTime
                // This allows the simulation to run at a different speed than the rest of the game
                materialProperties.exposureTimeSeconds += simulationManager.simulationDeltaTime;
                float failureTimeSeconds = materialProperties.achievedFireResistanceRating * 3600f;

                if (materialProperties.exposureTimeSeconds >= failureTimeSeconds)
                {
                    materialProperties.hasFailed = true;
                    if (enableDebugLogging) Debug.Log($"{gameObject.name} failed at {simulationManager.simulationTimeSeconds:F2} simulation seconds (Rating: {materialProperties.achievedFireResistanceRating} hrs).");
                }
            }
            
            // Check for fire spreading only if we've passed the threshold and are allowed to spread
            if (canSpreadFire && materialProperties.exposureTimeSeconds >= spreadThresholdSeconds)
            {
                timeSinceLastSpreadCheck += simulationManager.simulationDeltaTime;
                
                if (timeSinceLastSpreadCheck >= spreadCheckInterval)
                {
                    // Find nearby objects and spread fire in one simple step
                    SpreadFire();
                    timeSinceLastSpreadCheck = 0f;
                }
            }
        }

        // Update the visual state (used by IntegrityVisualizer)
        UpdateCurrentState();
        
        // Update spreading state
        isCurrentlySpreading = materialProperties.isExposedToFire && 
                              materialProperties.exposureTimeSeconds >= spreadThresholdSeconds &&
                              !materialProperties.hasFailed;
    }
    
    // Get the best detection center for this object
    private Vector3 GetDetectionCenter()
    {
        // If an override is set, use that
        if (detectionCenterOverride != null)
        {
            return detectionCenterOverride.position;
        }
        
        // First try to use the renderer bounds
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.center;
        }
        
        // Next try to use a collider if available
        Collider collider = GetComponentInChildren<Collider>();
        if (collider != null)
        {
            return collider.bounds.center;
        }
        
        // Fall back to transform position
        return transform.position;
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
    
    private void SpreadFire()
    {
        if (!isCurrentlySpreading) return;
        
        // Get the detection center
        Vector3 center = GetDetectionCenter();
        
        // One simple OverlapSphere call
        Collider[] hitColliders = Physics.OverlapSphere(center, spreadRadius, spreadableLayerMask);
        
        foreach (Collider hitCollider in hitColliders)
        {
            // Skip self
            if (hitCollider.gameObject == gameObject) continue;
            
            // Try to get material properties
            MaterialProperties nearbyProps = hitCollider.GetComponent<MaterialProperties>();
            if (nearbyProps != null && !nearbyProps.isExposedToFire)
            {
                // Get center of target
                Vector3 targetPos = hitCollider.bounds.center;
                
                // Calculate distance
                float distance = Vector3.Distance(center, targetPos);
                
                // Only spread if within radius
                if (distance <= spreadRadius)
                {
                    // Calculate probability based on distance
                    float probability = 1.0f - (distance / spreadRadius); // 1.0 at center, 0.0 at edge
                    
                    // Check against random value for randomness in spreading
                    if (Random.value <= probability)
                    {
                        nearbyProps.isExposedToFire = true;
                        
                        // Show visual connection when fire spreads (only if debug is enabled)
                        if (showDebugLines)
                        {
                            Debug.DrawLine(center, targetPos, Color.red, debugLineDuration);
                        }
                    }
                }
            }
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
        timeSinceLastSpreadCheck = 0f;
        isCurrentlySpreading = false;
        nearbyObjects.Clear();
    }
    
    // Draw gizmos to visualize spread radius when selected in editor
    void OnDrawGizmosSelected()
    {
        Vector3 center = GetDetectionCenter();
        
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Orange with transparency
        Gizmos.DrawWireSphere(center, spreadRadius);
    }
} 