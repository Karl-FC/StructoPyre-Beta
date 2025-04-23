using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MaterialPropertyEditor : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] public GameObject editorPanel; // Make public or add getter if needed elsewhere
    [SerializeField] private FaceInspector faceInspector; // MUST be assigned
    [SerializeField] private TMP_Text objectNameText;
    [SerializeField] private TMP_Dropdown elementTypeDropdown;
    [SerializeField] private GameObject parametersSection;
    [SerializeField] private GameObject restraintSection;
    [SerializeField] private TMP_Dropdown restraintDropdown;
    [SerializeField] private GameObject fireExposureSection;
    [SerializeField] private TMP_Dropdown columnFireExposureDropdown;
    [SerializeField] private TMP_Dropdown materialTypeDropdown;
    [SerializeField] private GameObject dimensionsSection;
    [SerializeField] private GameObject coverSection;
    [SerializeField] private TMP_InputField actualCoverField;
    [SerializeField] private GameObject thicknessSection;
    [SerializeField] private TMP_InputField actualThicknessField;
    [SerializeField] private GameObject leastDimensionSection;
    [SerializeField] private TMP_InputField actualLeastDimensionField;

    private MaterialProperties currentTarget;
    private bool isUpdatingUI = false;

    private void Start() // Changed back to Start from Awake
    {
        // Ensure FaceInspector is assigned
        if (faceInspector == null)
        {
            Debug.LogError("MaterialPropertyEditor requires the FaceInspector reference to be assigned in the Inspector!");
            this.enabled = false; // Disable if reference is missing
            return;
        }
        
        // Setup dropdowns and listeners once
        InitializeDropdownsAndListeners();
        
        // Start with the panel hidden (This should be handled by UIManager now, but setting here is safe)
        if (editorPanel != null) editorPanel.SetActive(false);
    }

    private void InitializeDropdownsAndListeners()
    {
        // Initialize dropdowns (options)
        if (elementTypeDropdown != null)
        {
            elementTypeDropdown.ClearOptions();
            elementTypeDropdown.AddOptions(new System.Collections.Generic.List<string>(Enum.GetNames(typeof(AciElementType))));
        }
        if (restraintDropdown != null)
        {
            restraintDropdown.ClearOptions();
            restraintDropdown.AddOptions(new System.Collections.Generic.List<string>(Enum.GetNames(typeof(AciRestraint))));
        }
        if (columnFireExposureDropdown != null)
        {
            columnFireExposureDropdown.ClearOptions();
            columnFireExposureDropdown.AddOptions(new System.Collections.Generic.List<string>(Enum.GetNames(typeof(AciColumnFireExposure))));
        }
        // Initialize Material Type Dropdown
        if (materialTypeDropdown != null)
        {
            materialTypeDropdown.ClearOptions();
            materialTypeDropdown.AddOptions(new System.Collections.Generic.List<string>(Enum.GetNames(typeof(AciAggregateCategory))));
        }
        
        // Add listeners for automatic updates
        elementTypeDropdown?.onValueChanged.AddListener(HandleElementTypeChanged);
        restraintDropdown?.onValueChanged.AddListener(HandleRestraintChanged);
        columnFireExposureDropdown?.onValueChanged.AddListener(HandleFireExposureChanged);
        actualCoverField?.onEndEdit.AddListener(HandleCoverChanged);
        actualThicknessField?.onEndEdit.AddListener(HandleThicknessChanged);
        actualLeastDimensionField?.onEndEdit.AddListener(HandleLeastDimensionChanged);
        materialTypeDropdown?.onValueChanged.AddListener(HandleMaterialTypeChanged);
    }

    // Update only populates if the panel is visible and inspector is active
    private void Update()
    {
        if (faceInspector == null || editorPanel == null || !editorPanel.activeSelf)
        {
             // If panel is hidden or dependencies missing, ensure currentTarget is cleared
             // This prevents listeners firing if panel is toggled off then on quickly.
             // currentTarget = null; // Optional: Consider if needed
             return;
        }

        // Panel is visible, check inspector state and target
        bool inspectorActive = faceInspector.IsInspectorCurrentlyActive;

        if (inspectorActive)
        {
            MaterialProperties targetProps = faceInspector.CurrentlyInspectedProperties;

            // If target changed OR panel just became visible, populate
            if (targetProps != null && currentTarget != targetProps)
            {
                 PopulateEditorFields(targetProps); // Populate with new target
            }
            else if (targetProps == null && currentTarget != null)
            {
                 // Inspector active, panel visible, but no target selected (or target lost)
                 // Keep panel visible (as per user request), but maybe clear fields or show "No target"?
                 // For now, just clear the internal target reference.
                 currentTarget = null;
                 // Optionally clear fields here if desired
                 // ClearEditorFields();
            }
            // If targetProps == null and currentTarget == null, do nothing (panel visible, no target)
            // If targetProps != null and currentTarget == targetProps, do nothing (panel visible, same target)
        }
        else // Inspector Mode is NOT active, but panel is somehow visible? Hide it.
        {
            // This case should ideally be prevented by the UIManager logic,
            // but as a safeguard:
            HideEditor();
        }
    }

    // Renamed - just populates fields, doesn't control visibility
    private void PopulateEditorFields(MaterialProperties target)
    {
        if (target == null)
        {
            // Maybe clear fields or show placeholder text?
            Debug.LogWarning("PopulateEditorFields called with null target.");
             currentTarget = null; // Ensure internal target is cleared
            return;
        }

        currentTarget = target;
        isUpdatingUI = true; // Prevent listeners from firing

        if (objectNameText != null) objectNameText.text = $"{currentTarget.gameObject.name}";

        // Setup the form with current values
        elementTypeDropdown.value = (int)currentTarget.elementType;
        restraintDropdown.value = (int)currentTarget.restraint;
        columnFireExposureDropdown.value = (int)currentTarget.columnFireExposure;
        actualCoverField.text = (currentTarget.actualCover_u * 1000f).ToString("F1");
        actualThicknessField.text = (currentTarget.actualEquivalentThickness_te * 1000f).ToString("F1");
        actualLeastDimensionField.text = (currentTarget.actualLeastDimension * 1000f).ToString("F1");
        if (materialTypeDropdown != null)
        {
            if (currentTarget.realMaterial != null)
            {
                materialTypeDropdown.value = (int)currentTarget.realMaterial.aggregateCategory;
            }
            else
            {
                materialTypeDropdown.value = (int)AciAggregateCategory.Unknown;
                Debug.LogWarning($"MaterialProperties on {currentTarget.gameObject.name} does not have a 'realMaterial' assigned.");
            }
        }
        UpdateFieldVisibility((int)currentTarget.elementType);

        isUpdatingUI = false; // Re-enable listeners
    }

    // --- Methods Called Externally ---

    // Called by UIManager when InspectorMode is turned ON
    public void ShowPanelIfTargetSelected()
    {
        if (editorPanel == null || faceInspector == null) return;

        if (faceInspector.CurrentlyInspectedProperties != null)
        {
             // A target is selected, show and populate
             editorPanel.SetActive(true);
             PopulateEditorFields(faceInspector.CurrentlyInspectedProperties);
        }
        else
        {
             // No target selected, ensure panel is hidden (or keep visible but empty?)
             // For now, keep it hidden if no target when mode turns ON.
             editorPanel.SetActive(false);
             currentTarget = null;
        }
    }

    // Called by UIManager when InspectorMode is turned OFF, or by 'E' key logic if needed
    public void HideEditor()
    {
        if (editorPanel != null) editorPanel.SetActive(false);
        currentTarget = null; // Clear target when hiding
    }

    // Called by 'E' key toggle logic
    public void TogglePanel()
    {
         if (editorPanel != null)
         {
            bool shouldBeActive = !editorPanel.activeSelf;
            editorPanel.SetActive(shouldBeActive);
            

            // If panel becomes visible, ensure it's populated if there's a target
            if (shouldBeActive && faceInspector != null && faceInspector.IsInspectorCurrentlyActive)
            {
                 PopulateEditorFields(faceInspector.CurrentlyInspectedProperties);
            }
            else if (!shouldBeActive)
            {
                 currentTarget = null; // Clear target if hiding via togglse
            }
         }
    }

    // --- Listener Handlers (Remain the same) --- 
    private void HandleElementTypeChanged(int value) { if (isUpdatingUI || currentTarget == null) return; currentTarget.elementType = (AciElementType)value; UpdateFieldVisibility(value); RecalculateRating("ElementType"); }
    private void HandleRestraintChanged(int value) { if (isUpdatingUI || currentTarget == null) return; currentTarget.restraint = (AciRestraint)value; RecalculateRating("Restraint"); }
    private void HandleFireExposureChanged(int value) { if (isUpdatingUI || currentTarget == null) return; currentTarget.columnFireExposure = (AciColumnFireExposure)value; RecalculateRating("FireExposure"); }
    private void HandleCoverChanged(string value) { if (isUpdatingUI || currentTarget == null) return; if (float.TryParse(value, out float v)) { currentTarget.actualCover_u = v / 1000f; RecalculateRating("Cover"); } }
    private void HandleThicknessChanged(string value) { if (isUpdatingUI || currentTarget == null) return; if (float.TryParse(value, out float v)) { currentTarget.actualEquivalentThickness_te = v / 1000f; RecalculateRating("Thickness"); } }
    private void HandleLeastDimensionChanged(string value) { if (isUpdatingUI || currentTarget == null) return; if (float.TryParse(value, out float v)) { currentTarget.actualLeastDimension = v / 1000f; RecalculateRating("LeastDimension"); } }
    // --- New Handler for Material Type Dropdown ---
    private void HandleMaterialTypeChanged(int value)
    {
        if (isUpdatingUI || currentTarget == null || currentTarget.realMaterial == null)
        {
             if(currentTarget != null && currentTarget.realMaterial == null)
             {
                 Debug.LogWarning($"Cannot change Aggregate Category for {currentTarget.gameObject.name} because 'realMaterial' is not assigned.");
             }
            return;
        }
        currentTarget.realMaterial.aggregateCategory = (AciAggregateCategory)value;
        RecalculateRating("AggregateCategory");
    }

    // --- Helper Methods (Remain the same) --- 
    private void UpdateFieldVisibility(int elementTypeIndex)
    {
        AciElementType selectedType = (AciElementType)elementTypeIndex;
        bool isSlab = selectedType == AciElementType.Slab;
        bool isBeam = selectedType == AciElementType.Beam;
        bool isWall = selectedType == AciElementType.Wall;
        bool isColumn = selectedType == AciElementType.ConcreteColumn;
        bool showRestraint = isSlab || isBeam;
        bool showFireExposure = isColumn;
        if (restraintSection != null) restraintSection.SetActive(showRestraint);
        if (fireExposureSection != null) fireExposureSection.SetActive(showFireExposure);
        if (parametersSection != null) parametersSection.SetActive(showRestraint || showFireExposure);
        bool showCover = isSlab || isBeam;
        bool showThickness = isSlab || isWall;
        bool showLeastDimension = isColumn;
        if (coverSection != null) coverSection.SetActive(showCover);
        if (thicknessSection != null) thicknessSection.SetActive(showThickness);
        if (leastDimensionSection != null) leastDimensionSection.SetActive(showLeastDimension);
        if (dimensionsSection != null) dimensionsSection.SetActive(showCover || showThickness || showLeastDimension);
        // Material Type Dropdown is always part of the parameters section if it's visible
        if (materialTypeDropdown != null) materialTypeDropdown.gameObject.SetActive(parametersSection != null && parametersSection.activeSelf);
    }
    private void RecalculateRating(string changedProperty = "Unknown")
    {
        if (currentTarget == null) return;
        float oldRating = currentTarget.achievedFireResistanceRating;
        currentTarget.achievedFireResistanceRating = AciRatingCalculator.CalculateRating(currentTarget);
        Debug.Log($"Recalculated rating for {currentTarget.gameObject.name} due to change in '{changedProperty}'. New rating: {currentTarget.achievedFireResistanceRating:F2} hours (was {oldRating:F2})");
        // Update inspector text logic can be added here if needed
    }
} 