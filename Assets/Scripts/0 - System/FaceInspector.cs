using UnityEngine;
using TMPro; // Required for TextMeshPro UI elements

[RequireComponent(typeof(LineRenderer))] // Ensure LineRenderer is present
public class FaceInspector : MonoBehaviour
{
    [Header("Raycasting Settings")]
    [SerializeField] private float maxRayDistance = 100f; // How far the ray should check
    [SerializeField] private LayerMask inspectLayerMask; // Which layers to inspect (optional, but good practice)

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI inspectionTextUI; // Assign your TextMeshPro UGUI element here

    [Header("Visual Feedback - Toggleable Laser Pointer")]
    [SerializeField] private bool showRaycastVisual = true; // Toggle for the laser pointer
    // LineRenderer will be grabbed automatically due to [RequireComponent]

    private Camera mainCamera;
    private bool isInspectorActive = false; // Start inactive by default
    private LineRenderer lineRenderer;

    // Public getter for the internal state
    public bool IsInspectorCurrentlyActive => isInspectorActive;

    // Add a property to access the currently inspected MaterialProperties
    private MaterialProperties currentlyInspectedProperties;
    public MaterialProperties CurrentlyInspectedProperties => currentlyInspectedProperties;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        lineRenderer = GetComponent<LineRenderer>(); // Get the component

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
             // Don't disable the whole script, just the UI part might fail
        }
        else
        {
            inspectionTextUI.text = ""; // Clear text initially
        }

        // Configure LineRenderer defaults
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.useWorldSpace = true;
        // Make sure LineRenderer material is set in Inspector (e.g., a simple Unlit/Color material)
        if (lineRenderer.sharedMaterial == null)
        {
             Debug.LogWarning("LineRenderer on FaceInspector needs a material assigned in the Inspector.");
        }

        // Ensure initial state is correct
        SetInspectorActive(isInspectorActive); // Handles clearing text
        UpdateRaycastVisual(); // Handles enabling/disabling LineRenderer

    }

    void Update()
    {
        // Only run if inspector mode is active
        if (!isInspectorActive || mainCamera == null || inspectionTextUI == null) return;

        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;

        // Perform the raycast, now using the LayerMask
        bool didHit = Physics.Raycast(ray, out hit, maxRayDistance, inspectLayerMask);

        if (didHit)
        {
            // Try to get MaterialProperties from the hit object
            MaterialProperties props = hit.collider.GetComponent<MaterialProperties>();
            
            // Store the reference to the current properties being inspected
            currentlyInspectedProperties = props;

            if (props != null && props.realMaterial != null)
            {
                if (inspectionTextUI != null) inspectionTextUI.text = $"Looking at: {props.realMaterial.realmaterialName}\n" +
                                        $"Type: {props.elementType}\n" +
                                        $"Rating: {props.achievedFireResistanceRating:F1} hrs\n" + // Format to 1 decimal place
                                        $"Cover: {props.actualCover_u * 1000:F0} mm\n" + // Show in mm
                                        $"Thickness (tₑ): {props.actualEquivalentThickness_te * 1000:F0} mm"; // Show in mm
            }
            else
            {
                if (inspectionTextUI != null) inspectionTextUI.text = $"Looking at: {hit.collider.gameObject.name} (No Fire Properties)";
            }
        }
        else
        {
            // Ray didn't hit anything within range
            currentlyInspectedProperties = null; // Clear the reference
            if (inspectionTextUI != null) inspectionTextUI.text = ""; // Clear the text
        }

        // Update visual even if inspector text isn't active, based on toggle
        if (showRaycastVisual && lineRenderer != null)
        {
             UpdateLaserPointer();
        }
        else if (lineRenderer != null && lineRenderer.enabled)
        {
            lineRenderer.enabled = false; // Ensure it's off if toggled off
        }
    }

    void UpdateLaserPointer()
    {
        if (mainCamera == null || lineRenderer == null) return;

        lineRenderer.enabled = true; // Make sure it's visible
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;

        Vector3 endPosition;
        // Perform a raycast *just for the visual* - can potentially hit different layers than the inspection ray if masks differ
        // Or reuse the inspection hit info if Update order guarantees PerformRaycast runs first when active
        if (Physics.Raycast(ray, out hit, maxRayDistance)) // Use a simple raycast for the visual endpoint
        {
            endPosition = hit.point;
        }
        else
        {
            endPosition = mainCamera.transform.position + mainCamera.transform.forward * maxRayDistance;
        }

        lineRenderer.SetPosition(0, mainCamera.transform.position);
        lineRenderer.SetPosition(1, endPosition);
    }

    // Public method to toggle the inspector state
    public void SetInspectorActive(bool isActive)
    {
        isInspectorActive = isActive;
        if (!isActive)
        {
            if (inspectionTextUI != null)
            {
                inspectionTextUI.text = ""; // Clear text when deactivated
            }
            currentlyInspectedProperties = null; // Clear the reference when deactivated
        }
        Debug.Log($"Inspector Mode Active: {isInspectorActive}"); // Optional debug log
        // Visual update is handled in Update based on showRaycastVisual flag
    }

    // Public method to toggle the laser pointer visual (for settings menu later)
    public void ToggleRaycastVisual(bool show)
    {
        showRaycastVisual = show;
        UpdateRaycastVisual();
    }

    private void UpdateRaycastVisual()
    {
         if (lineRenderer != null)
        {
            lineRenderer.enabled = showRaycastVisual;
        }
    }
} 