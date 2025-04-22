using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MaterialPropertyEditor : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject editorPanel;
    [SerializeField] private FaceInspector faceInspector;
    [SerializeField] private TMP_Text objectNameText;

    [Header("Element Type")]
    [SerializeField] private TMP_Dropdown elementTypeDropdown;

    [Header("Parameter Sections & Fields")]
    [SerializeField] private GameObject parametersSection;
    [SerializeField] private GameObject restraintSection;
    [SerializeField] private TMP_Dropdown restraintDropdown;
    [SerializeField] private GameObject fireExposureSection;
    [SerializeField] private TMP_Dropdown columnFireExposureDropdown;

    [Header("Dimension Sections & Fields")]
    [SerializeField] private GameObject dimensionsSection;
    [SerializeField] private GameObject coverSection;
    [SerializeField] private TMP_InputField actualCoverField;
    [SerializeField] private GameObject thicknessSection;
    [SerializeField] private TMP_InputField actualThicknessField;
    [SerializeField] private GameObject leastDimensionSection;
    [SerializeField] private TMP_InputField actualLeastDimensionField;

    private MaterialProperties currentTarget;
    private bool isUpdatingUI = false; // Flag to prevent feedback loops

    private void Start()
    {
        if (editorPanel != null) editorPanel.SetActive(false);
        InitializeDropdownsAndListeners();
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
        actualCoverField?.onEndEdit.AddListener(HandleCoverChanged); // Use onEndEdit for input fields
        actualThicknessField?.onEndEdit.AddListener(HandleThicknessChanged);
        actualLeastDimensionField?.onEndEdit.AddListener(HandleLeastDimensionChanged);
    }

    private void Update()
    {
        // Check for E key press to open editor
        if (Input.GetKeyDown(KeyCode.E) && faceInspector != null && faceInspector.IsInspectorCurrentlyActive)
        {
            // Close if already open, otherwise try to open
            if (editorPanel != null && editorPanel.activeSelf)
            {
                 CloseEditor();
            }
            else
            {
                MaterialProperties targetProps = faceInspector.CurrentlyInspectedProperties;
                if (targetProps != null)
                {
                    OpenEditor(targetProps);
                }
                else
                {
                    Debug.Log("Cannot open editor: No inspectable object with MaterialProperties is currently being looked at.");
                }
            }
        }

        // Check for Escape key to close editor
        if (Input.GetKeyDown(KeyCode.Escape) && editorPanel != null && editorPanel.activeSelf)
        {
            CloseEditor();
        }
    }

    public void OpenEditor(MaterialProperties target)
    {
        if (target == null || editorPanel == null) return;
        currentTarget = target;
        isUpdatingUI = true; // Prevent listeners from firing while we set initial values

        if (objectNameText != null) objectNameText.text = $"Editing: {currentTarget.gameObject.name}";

        // Setup the form with current values
        elementTypeDropdown.value = (int)currentTarget.elementType;
        restraintDropdown.value = (int)currentTarget.restraint;
        columnFireExposureDropdown.value = (int)currentTarget.columnFireExposure;
        actualCoverField.text = (currentTarget.actualCover_u * 1000f).ToString("F1");
        actualThicknessField.text = (currentTarget.actualEquivalentThickness_te * 1000f).ToString("F1");
        actualLeastDimensionField.text = (currentTarget.actualLeastDimension * 1000f).ToString("F1");

        // Apply initial field visibility based on element type
        UpdateFieldVisibility((int)currentTarget.elementType);

        isUpdatingUI = false; // Re-enable listeners
        editorPanel.SetActive(true);
        // Consider pausing player movement/camera controls here
    }

    public void CloseEditor()
    {
        if (editorPanel != null) editorPanel.SetActive(false);
        currentTarget = null;
        // Consider resuming player movement/camera controls here
    }

    // --- Listener Handlers --- 

    private void HandleElementTypeChanged(int value)
    {
        if (isUpdatingUI || currentTarget == null) return;
        currentTarget.elementType = (AciElementType)value;
        UpdateFieldVisibility(value); // Show/hide relevant fields
        RecalculateRating("ElementType");
    }

    private void HandleRestraintChanged(int value)
    {
        if (isUpdatingUI || currentTarget == null) return;
        currentTarget.restraint = (AciRestraint)value;
        RecalculateRating("Restraint");
    }

    private void HandleFireExposureChanged(int value)
    {
        if (isUpdatingUI || currentTarget == null) return;
        currentTarget.columnFireExposure = (AciColumnFireExposure)value;
        RecalculateRating("FireExposure");
    }

    private void HandleCoverChanged(string value)
    {
        if (isUpdatingUI || currentTarget == null) return;
        if (float.TryParse(value, out float coverValueMm))
        {
            currentTarget.actualCover_u = coverValueMm / 1000f; // Convert mm to meters
            RecalculateRating("Cover");
        }
        else
        {
             Debug.LogWarning($"Invalid input for Cover: {value}");
             // Optionally revert UI field to previous value
             // actualCoverField.text = (currentTarget.actualCover_u * 1000f).ToString("F1");
        }
    }

    private void HandleThicknessChanged(string value)
    {
        if (isUpdatingUI || currentTarget == null) return;
        if (float.TryParse(value, out float thicknessValueMm))
        {
            currentTarget.actualEquivalentThickness_te = thicknessValueMm / 1000f;
            RecalculateRating("Thickness");
        }
         else
        {
             Debug.LogWarning($"Invalid input for Thickness: {value}");
        }
    }

    private void HandleLeastDimensionChanged(string value)
    {
        if (isUpdatingUI || currentTarget == null) return;
        if (float.TryParse(value, out float dimensionValueMm))
        {
            currentTarget.actualLeastDimension = dimensionValueMm / 1000f;
            RecalculateRating("LeastDimension");
        }
         else
        {
             Debug.LogWarning($"Invalid input for Least Dimension: {value}");
        }
    }

    // --- Helper Methods --- 

    private void UpdateFieldVisibility(int elementTypeIndex)
    {
        AciElementType selectedType = (AciElementType)elementTypeIndex;

        bool isSlab = selectedType == AciElementType.Slab;
        bool isBeam = selectedType == AciElementType.Beam;
        bool isWall = selectedType == AciElementType.Wall;
        bool isColumn = selectedType == AciElementType.ConcreteColumn;

        // Parameter Visibility
        bool showRestraint = isSlab || isBeam;
        bool showFireExposure = isColumn;

        if (restraintSection != null) restraintSection.SetActive(showRestraint);
        if (fireExposureSection != null) fireExposureSection.SetActive(showFireExposure);
        if (parametersSection != null) parametersSection.SetActive(showRestraint || showFireExposure);

        // Dimension Visibility
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

        // Optional: Update the inspector text immediately if it's active
        if (faceInspector != null && faceInspector.IsInspectorCurrentlyActive && faceInspector.CurrentlyInspectedProperties == currentTarget)
        {
            // Force FaceInspector to update its text display (needs a public method or logic adjustment there)
            // For now, the change will be visible next time FaceInspector updates.
        }
    }
}
