using UnityEngine;

[RequireComponent(typeof(FireSource))]
public class FireConfig : MonoBehaviour
{
    [Header("Fire Appearance")]
    [Tooltip("Particle system used for fire visualization")]
    public ParticleSystem fireParticles;
    [Tooltip("Light component for fire illumination")]
    public Light fireLight;
    
    [Header("Fire Settings")]
    [Range(1f, 20f)]
    [Tooltip("Maximum radius this fire can affect")]
    public float maxRadius = 5f;
    [Tooltip("How quickly fire grows to max radius (meters per minute)")]
    public float growthRate = 0.5f;
    [Tooltip("Whether fire grows automatically or remains at starting size")]
    public bool autoGrow = true;
    
    private FireSource fireSource;
    private float initialRadius;
    private float growthTimer = 0f;
    private SimulationManager simulationManager;
    
    void Start()
    {
        fireSource = GetComponent<FireSource>();
        initialRadius = fireSource.exposureRadius;
        
        // Find simulation manager
        simulationManager = SimulationManager.Instance;
        if (simulationManager == null)
        {
            Debug.LogWarning("FireConfig could not find SimulationManager instance");
        }
        
        // Make sure layer mask is set for structural elements
        if (fireSource.structuralLayerMask.value == 0)
        {
            fireSource.structuralLayerMask = LayerMask.GetMask("InspectableModel");
        }
    }
    
    void Update()
    {
        if (simulationManager == null || !autoGrow) return;
        
        // Only grow when simulation is running
        if (simulationManager.currentState == SimulationManager.SimulationState.Running)
        {
            // Convert growth rate from meters/minute to meters/second
            float growthPerSecond = growthRate / 60f; 
            
            // Apply growth based on simulation delta time
            growthTimer += simulationManager.simulationDeltaTime;
            float targetRadius = Mathf.Min(initialRadius + (growthTimer * growthPerSecond), maxRadius);
            
            // Update fire source radius
            if (fireSource.exposureRadius != targetRadius)
            {
                fireSource.exposureRadius = targetRadius;
                
                // Scale particle systems if available
                if (fireParticles != null)
                {
                    // Scale particle system relative to radius
                    float scale = targetRadius / initialRadius;
                    
                    var main = fireParticles.main;
                    main.startSizeMultiplier = Mathf.Max(1f, scale);
                    
                    // Could also adjust emission rate, lifetime, etc based on scale
                }
                
                // Adjust light intensity if available
                if (fireLight != null)
                {
                    fireLight.range = targetRadius * 2f;
                    fireLight.intensity = Mathf.Min(4f, 1f + targetRadius / 3f);
                }
            }
        }
    }
    
    // Called when the simulation resets
    public void ResetFire()
    {
        fireSource.exposureRadius = initialRadius;
        growthTimer = 0f;
        
        if (fireParticles != null)
        {
            var main = fireParticles.main;
            main.startSizeMultiplier = 1f;
        }
        
        if (fireLight != null)
        {
            fireLight.range = initialRadius * 2f;
            fireLight.intensity = 1f;
        }
    }
} 