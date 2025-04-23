using UnityEngine;

[RequireComponent(typeof(FireIntegrityTracker))]
[RequireComponent(typeof(MeshRenderer))]
public class IntegrityVisualizer : MonoBehaviour
{
    [Header("State Colors")]
    [Tooltip("Color when the element is exposed to fire but hasn't failed yet.")]
    public Color exposedColor = Color.yellow; // Default Yellow

    [Tooltip("Color when the element has reached its fire resistance failure point.")]
    public Color failedColor = Color.black; // Default Black

    private FireIntegrityTracker integrityTracker;
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
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer.material == null)
        {
            Debug.LogError($"IntegrityVisualizer on {gameObject.name} requires a Material on its MeshRenderer.", this);
            this.enabled = false;
            return;
        }

        // Create an instance of the material for this specific object
        // This prevents changing the shared material asset.
        materialInstance = meshRenderer.material;
        originalColor = materialInstance.color; // Store the original color

        isInitialized = true;
        // Debug.Log($"Initialized Visualizer for {gameObject.name}. Original Color: {originalColor}");
    }

    void Update()
    {
        if (!isInitialized || integrityTracker == null) return;

        // Get the current state from the tracker
        FireIntegrityTracker.IntegrityState currentState = integrityTracker.CurrentState;

        // Apply color based on state
        Color targetColor = originalColor;
        switch (currentState)
        {
            case FireIntegrityTracker.IntegrityState.Exposed:
                targetColor = exposedColor;
                break;
            case FireIntegrityTracker.IntegrityState.Failed:
                targetColor = failedColor;
                break;
            case FireIntegrityTracker.IntegrityState.Healthy:
            default:
                targetColor = originalColor;
                break;
        }

        // Apply the color only if it has changed to avoid unnecessary material updates
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