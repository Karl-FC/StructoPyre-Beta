using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MaterialPropertyEditor : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject editorPanel;
    [SerializeField] private FaceInspector faceInspector; // MUST be assigned
    [SerializeField] private TMP_Text objectNameText;
    [SerializeField] private TMP_Dropdown elementTypeDropdown;
    [SerializeField] private GameObject parametersSection;
    [SerializeField] private GameObject restraintSection;
    [SerializeField] private TMP_Dropdown restraintDropdown;
    [SerializeField] private GameObject fireExposureSection;
    [SerializeField] private TMP_Dropdown columnFireExposureDropdown;
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
        
        // Start with the panel hidden
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
        
        // Add listeners for automatic updates
        elementTypeDropdown?.onValueChanged.AddListener(HandleElementTypeChanged);
        restraintDropdown?.onValueChanged.AddListener(HandleRestraintChanged);
        columnFireExposureDropdown?.onValueChanged.AddListener(HandleFireExposureChanged);
        actualCoverField?.onEndEdit.AddListener(HandleCoverChanged);
        actualThicknessField?.onEndEdit.AddListener(HandleThicknessChanged);
        actualLeastDimensionField?.onEndEdit.AddListener(HandleLeastDimensionChanged);
    }

    private void Update()
    {
        // Ensure dependencies are met
        if (faceInspector == null || editorPanel == null) return;

        // Check if Inspector Mode is active via the FaceInspector
        bool inspectorActive = faceInspector.IsInspectorCurrentlyActive;

        if (inspectorActive)
        {
            MaterialProperties targetProps = faceInspector.CurrentlyInspectedProperties;

            // Is there a valid target being inspected?
            if (targetProps != null)
            {
                // If this is a new target or the panel was hidden, show and populate
                if (currentTarget != targetProps || !editorPanel.activeSelf)
                {
                    ShowAndPopulateEditor(targetProps);
                }
                // If it's the same target, we could potentially skip repopulating every frame 
                // unless we suspect external changes, but repopulating is simpler for now.
                // Re-populate to ensure live data if values change externally (unlikely here but safer)
                // ShowAndPopulateEditor(targetProps); // Call this if constant refresh needed
            }
            else // Inspector active, but no valid target
            {
                // If the panel is currently shown, hide it
                if (editorPanel.activeSelf)
                {
                    HideEditor();
                }
            }
        }
        else // Inspector Mode is NOT active
        {
            // If the panel is currently shown, hide it
            if (editorPanel.activeSelf)
            {
                HideEditor();
            }
        }
    }

    // Renamed from OpenEditor - populates UI and ensures panel is visible
    private void ShowAndPopulateEditor(MaterialProperties target)
    {
        if (target == null) 
        {
            HideEditor();
            return;
        }
        
        currentTarget = target;
        isUpdatingUI = true; // Prevent listeners from firing

        if (objectNameText != null) objectNameText.text = $"Inspecting: {currentTarget.gameObject.name}";

        // Setup the form with current values
        elementTypeDropdown.value = (int)currentTarget.elementType;
        restraintDropdown.value = (int)currentTarget.restraint;
        columnFireExposureDropdown.value = (int)currentTarget.columnFireExposure;
        actualCoverField.text = (currentTarget.actualCover_u * 1000f).ToString("F1");
        actualThicknessField.text = (currentTarget.actualEquivalentThickness_te * 1000f).ToString("F1");
        actualLeastDimensionField.text = (currentTarget.actualLeastDimension * 1000f).ToString("F1");

        UpdateFieldVisibility((int)currentTarget.elementType);
        
        if (!editorPanel.activeSelf) editorPanel.SetActive(true);
        
        isUpdatingUI = false; // Re-enable listeners
    }

    // Renamed from CloseEditor - hides panel and clears target
    private void HideEditor()
    {
        if (editorPanel != null) editorPanel.SetActive(false);
        currentTarget = null;
    }

    // --- Listener Handlers (Remain the same) --- 
    private void HandleElementTypeChanged(int value) { if (isUpdatingUI || currentTarget == null) return; currentTarget.elementType = (AciElementType)value; UpdateFieldVisibility(value); RecalculateRating("ElementType"); }
    private void HandleRestraintChanged(int value) { if (isUpdatingUI || currentTarget == null) return; currentTarget.restraint = (AciRestraint)value; RecalculateRating("Restraint"); }
    private void HandleFireExposureChanged(int value) { if (isUpdatingUI || currentTarget == null) return; currentTarget.columnFireExposure = (AciColumnFireExposure)value; RecalculateRating("FireExposure"); }
    private void HandleCoverChanged(string value) { if (isUpdatingUI || currentTarget == null) return; if (float.TryParse(value, out float v)) { currentTarget.actualCover_u = v / 1000f; RecalculateRating("Cover"); } }
    private void HandleThicknessChanged(string value) { if (isUpdatingUI || currentTarget == null) return; if (float.TryParse(value, out float v)) { currentTarget.actualEquivalentThickness_te = v / 1000f; RecalculateRating("Thickness"); } }
    private void HandleLeastDimensionChanged(string value) { if (isUpdatingUI || currentTarget == null) return; if (float.TryParse(value, out float v)) { currentTarget.actualLeastDimension = v / 1000f; RecalculateRating("LeastDimension"); } }

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