using UnityEngine;
using TMPro; // Required for TextMeshPro UI elements

public class FaceInspector : MonoBehaviour
{
    [Header("Raycasting Settings")]
    [SerializeField] private float maxRayDistance = 100f; // How far the ray should check
    [SerializeField] private LayerMask inspectLayerMask; // Which layers to inspect (optional, but good practice)

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI inspectionTextUI; // Assign your TextMeshPro UGUI element here

    private Camera mainCamera;
    private bool isInspectorActive = false; // Start inactive by default

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            // Fallback if script is not directly on the camera
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("FaceInspector requires a Camera component on the same GameObject or a tagged 'MainCamera' in the scene.");
                enabled = false; // Disable script if no camera found
                return;
            }
        }

        if (inspectionTextUI == null)
        {
             Debug.LogError("FaceInspector requires the 'Inspection Text UI' field to be assigned in the Inspector.");
             enabled = false;
        }
        else
        {
            // Clear text initially
            inspectionTextUI.text = "";
        }

        // Ensure it starts in the correct state (likely inactive)
        SetInspectorActive(isInspectorActive);
    }

    void Update()
    {
        // Only run if inspector mode is active
        if (!isInspectorActive || mainCamera == null || inspectionTextUI == null) return;

        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;

        // Perform the raycast
        // Using LayerMask is optional but recommended for performance if you only want to inspect specific layers
        bool didHit = Physics.Raycast(ray, out hit, maxRayDistance); // Add layerMask parameter if using LayerMask

        if (didHit)
        {
            // Try to get MaterialProperties from the hit object
            MaterialProperties props = hit.collider.GetComponent<MaterialProperties>();

            if (props != null && props.realMaterial != null)
            {
                // Found properties, format and display them
                inspectionTextUI.text = $"Looking at: {props.realMaterial.realmaterialName}\n" +
                                        $"Type: {props.elementType}\n" +
                                        $"Rating: {props.achievedFireResistanceRating:F1} hrs\n" + // Format to 1 decimal place
                                        $"Cover: {props.actualCover_u * 1000:F0} mm\n" + // Show in mm
                                        $"Thickness (tₑ): {props.actualEquivalentThickness_te * 1000:F0} mm"; // Show in mm
            }
            else
            {
                // Hit something, but it doesn't have the properties we need
                inspectionTextUI.text = $"Looking at: {hit.collider.gameObject.name} (No Fire Properties)";
            }
        }
        else
        {
            // Ray didn't hit anything within range
            inspectionTextUI.text = ""; // Clear the text
        }
    }

    // Public method to toggle the inspector state
    public void SetInspectorActive(bool isActive)
    {
        isInspectorActive = isActive;
        if (!isActive && inspectionTextUI != null)
        {
            inspectionTextUI.text = ""; // Clear text when deactivated
        }
        Debug.Log($"Inspector Mode Active: {isInspectorActive}"); // Optional debug log
    }
} 