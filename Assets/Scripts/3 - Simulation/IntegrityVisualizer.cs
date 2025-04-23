using UnityEngine;

[RequireComponent(typeof(FireIntegrityTracker))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MaterialProperties))]
public class IntegrityVisualizer : MonoBehaviour
{
    [Header("Visualization Settings")]
    [Tooltip("Gradient representing the color transition from the moment of exposure (time 0) to the point of failure (time 1).")]
    public Gradient exposureGradient; // Replaces exposedColor

    [Tooltip("Color when the element has reached its fire resistance failure point.")]
    public Color failedColor = Color.black; // Default Black

    private FireIntegrityTracker integrityTracker;
    private MaterialProperties materialProperties; // Added reference
    private MeshRenderer meshRenderer;
    private Material materialInstance; // Instance of the material for this object
    private Color originalColor;
    private bool isInitialized = false;

    void Awake()
    {
        Initialize();
    }

    void Initialize()
    {
        if (isInitialized) return;

        integrityTracker = GetComponent<FireIntegrityTracker>();
        materialProperties = GetComponent<MaterialProperties>(); // Get reference
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer.material == null)
        {
            Debug.LogError($"IntegrityVisualizer on {gameObject.name} requires a Material on its MeshRenderer.", this);
            this.enabled = false;
            return;
        }
        if (materialProperties == null) // Safety check
        {
             Debug.LogError($"IntegrityVisualizer on {gameObject.name} could not find MaterialProperties component.", this);
             this.enabled = false;
             return;
        }

        // Create an instance of the material for this specific object
        materialInstance = meshRenderer.material;
        originalColor = materialInstance.color; 

        // Initialize Gradient if null (prevents errors)
        if (exposureGradient == null)
        {
            exposureGradient = new Gradient();
            // Set default gradient (e.g., white to red)
            exposureGradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.green, 0.0f), new GradientColorKey(Color.red, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
            );
            Debug.LogWarning($"Exposure Gradient was not set for {gameObject.name}. Using default Green->Red gradient.");
        }

        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized || integrityTracker == null || materialProperties == null) return;

        FireIntegrityTracker.IntegrityState currentState = integrityTracker.CurrentState;
        Color targetColor = originalColor;

        switch (currentState)
        {
            case FireIntegrityTracker.IntegrityState.Exposed:
                float failureTimeSeconds = materialProperties.achievedFireResistanceRating * 3600f;
                if (failureTimeSeconds > 0)
                {
                    // Calculate progress towards failure (0.0 to 1.0)
                    float progress = Mathf.Clamp01(materialProperties.exposureTimeSeconds / failureTimeSeconds);
                    targetColor = exposureGradient.Evaluate(progress);
                }
                else
                {
                    // If rating is invalid, just use the start color of the gradient
                    targetColor = exposureGradient.Evaluate(0f); 
                }
                break;

            case FireIntegrityTracker.IntegrityState.Failed:
                targetColor = failedColor;
                break;

            case FireIntegrityTracker.IntegrityState.Healthy:
            default:
                targetColor = originalColor;
                break;
        }

        if (materialInstance.color != targetColor)
        { 
             materialInstance.color = targetColor;
        }
    }

    // Called by SimulationManager (or potentially FireIntegrityTracker) when resetting
    public void ResetVisuals()
    {
        if (!isInitialized)
        {
             // If Awake hasn't run yet (e.g., object inactive during reset), try initializing.
             Initialize();
             if (!isInitialized) return; // Still couldn't initialize
        }
        
        if (materialInstance != null)
        {
            materialInstance.color = originalColor;
            // Debug.Log($"Resetting visuals for {gameObject.name} to {originalColor}");
        }
    }

    // Optional: Clean up the material instance when the object is destroyed
    void OnDestroy()
    {
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }
} 