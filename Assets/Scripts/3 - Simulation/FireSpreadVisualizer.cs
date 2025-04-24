using UnityEngine;

[RequireComponent(typeof(FireIntegrityTracker))]
public class FireSpreadVisualizer : MonoBehaviour
{
    [Header("Visual Settings")]
    [Tooltip("Prefab for fire particle effect")]
    public GameObject fireEffectPrefab;
    [Tooltip("Whether to create fire effects when spreading")]
    public bool showFireEffects = true;
    
    private FireIntegrityTracker fireTracker;
    private GameObject fireEffect;
    private bool wasCurrentlySpreading = false;
    
    void Start()
    {
        fireTracker = GetComponent<FireIntegrityTracker>();
    }
    
    void Update()
    {
        if (fireTracker == null) return;
        
        MaterialProperties props = fireTracker.GetComponent<MaterialProperties>();
        if (props == null) return;
        
        bool isCurrentlySpreading = props.isExposedToFire && 
                                   props.exposureTimeSeconds >= fireTracker.spreadThresholdSeconds &&
                                   !props.hasFailed;
        
        // Update visualization when spreading state changes
        if (isCurrentlySpreading != wasCurrentlySpreading)
        {
            if (isCurrentlySpreading)
            {
                // Create fire effect if it doesn't exist
                if (showFireEffects && fireEffect == null && fireEffectPrefab != null)
                {
                    // Find a good position for the fire effect using the renderer if available
                    Renderer objectRenderer = GetComponentInChildren<Renderer>();
                    Vector3 effectPosition = objectRenderer != null 
                        ? objectRenderer.bounds.center 
                        : transform.position;
                    
                    fireEffect = Instantiate(fireEffectPrefab, effectPosition, Quaternion.identity);
                    fireEffect.transform.SetParent(transform);
                    
                    Debug.Log($"Created fire effect on {gameObject.name} at position {effectPosition}");
                }
            }
            else
            {
                // Remove fire effect if it exists
                if (fireEffect != null)
                {
                    Destroy(fireEffect);
                    fireEffect = null;
                }
            }
            
            wasCurrentlySpreading = isCurrentlySpreading;
        }
        
        // If the object moves, update fire effect position
        if (fireEffect != null)
        {
            Renderer objectRenderer = GetComponentInChildren<Renderer>();
            if (objectRenderer != null)
            {
                fireEffect.transform.position = objectRenderer.bounds.center;
            }
        }
    }
    
    void OnDestroy()
    {
        if (fireEffect != null)
        {
            Destroy(fireEffect);
        }
    }
} 