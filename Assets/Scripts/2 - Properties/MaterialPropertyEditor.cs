using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.InputSystem; // Required for New Input System

public class MaterialPropertyEditor : MonoBehaviour
{
    // ... (Keep existing UI References and fields) ...
    [SerializeField] private GameObject editorPanel;
    [SerializeField] private FaceInspector faceInspector;
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

    private ControlThings controls; // Reference to the Input Actions asset wrapper
    private InputAction openEditorAction; // Reference to the specific action

    private void Awake() // Changed from Start to Awake for input setup
    {
        controls = new ControlThings();

        if (editorPanel != null) editorPanel.SetActive(false);
        InitializeDropdownsAndListeners();
    }

    private void OnEnable()
    {
        Debug.Log("MaterialPropertyEditor: OnEnable() called.");
        // Find the action in the "Cross Platform" map
        openEditorAction = controls.CrossPlatform.OpenPropertyEditor;
        if (openEditorAction != null)
        {
            openEditorAction.Enable();
            openEditorAction.performed += OnOpenEditorInput;
            Debug.Log("MaterialPropertyEditor: Subscribed to CrossPlatform/OpenPropertyEditor action.");
        }
        else 
        {
            Debug.LogError("Could not find 'OpenPropertyEditor' action in 'Cross Platform' map. Did you add it and save the Input Actions asset?");
        }

        // CRITICAL CHECK: Let's see if the map itself is enabled somewhere else
        // Explicitly enable the Cross Platform map here for simplicity, 
        // assuming this script is the primary user of this map for this specific action.
        controls.CrossPlatform.Enable(); 
        Debug.Log($"MaterialPropertyEditor: Cross Platform Action Map Enabled State: {controls.CrossPlatform.enabled}"); 
    }

    private void OnDisable()
    {
        Debug.Log("MaterialPropertyEditor: OnDisable() called.");
        if (openEditorAction != null)
        {
            openEditorAction.performed -= OnOpenEditorInput;
            openEditorAction.Disable();
            Debug.Log("MaterialPropertyEditor: Unsubscribed from CrossPlatform/OpenPropertyEditor action.");
        }
        // Disable the map when this object is disabled, IF this script is solely responsible for enabling it.
        // If other scripts use CrossPlatform map, disabling it here might break them.
        // Consider a central input manager if multiple scripts use the same map.
        controls.CrossPlatform.Disable(); 
    }

    private void InitializeDropdownsAndListeners()
    {
        // ... (Existing dropdown initialization code remains the same) ...
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

        elementTypeDropdown?.onValueChanged.AddListener(HandleElementTypeChanged);
        restraintDropdown?.onValueChanged.AddListener(HandleRestraintChanged);
        columnFireExposureDropdown?.onValueChanged.AddListener(HandleFireExposureChanged);
        actualCoverField?.onEndEdit.AddListener(HandleCoverChanged); 
        actualThicknessField?.onEndEdit.AddListener(HandleThicknessChanged);
        actualLeastDimensionField?.onEndEdit.AddListener(HandleLeastDimensionChanged);
    }

    private void Update()
    {
        // REMOVED: E key check is now handled by Input Action callback (OnOpenEditorInput)

        // Check for Escape key to close editor (Still using old input for now - consider moving to Input System UI Cancel later)
        if (Input.GetKeyDown(KeyCode.Escape) && editorPanel != null && editorPanel.activeSelf)
        {
            CloseEditor();
        }
    }

    // Callback method triggered by the 'OpenPropertyEditor' input action
    private void OnOpenEditorInput(InputAction.CallbackContext context)
    {
        if (faceInspector == null || !faceInspector.IsInspectorCurrentlyActive) return;

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

    // ... (OpenEditor, CloseEditor, Handler methods, UpdateFieldVisibility, RecalculateRating remain largely the same) ...
    public void OpenEditor(MaterialProperties target) // (No changes needed inside this method)
    {
        if (target == null || editorPanel == null) return;
        currentTarget = target;
        isUpdatingUI = true;
        if (objectNameText != null) objectNameText.text = $"Editing: {currentTarget.gameObject.name}";
        elementTypeDropdown.value = (int)currentTarget.elementType;
        restraintDropdown.value = (int)currentTarget.restraint;
        columnFireExposureDropdown.value = (int)currentTarget.columnFireExposure;
        actualCoverField.text = (currentTarget.actualCover_u * 1000f).ToString("F1");
        actualThicknessField.text = (currentTarget.actualEquivalentThickness_te * 1000f).ToString("F1");
        actualLeastDimensionField.text = (currentTarget.actualLeastDimension * 1000f).ToString("F1");
        UpdateFieldVisibility((int)currentTarget.elementType);
        isUpdatingUI = false;
        editorPanel.SetActive(true);
    }

    public void CloseEditor() // (No changes needed inside this method)
    {
        if (editorPanel != null) editorPanel.SetActive(false);
        currentTarget = null;
    }
    
    private void HandleElementTypeChanged(int value) // (No changes needed inside this method)
    {
        if (isUpdatingUI || currentTarget == null) return;
        currentTarget.elementType = (AciElementType)value;
        UpdateFieldVisibility(value);
        RecalculateRating("ElementType");
    }
    private void HandleRestraintChanged(int value) // (No changes needed inside this method)
    {
        if (isUpdatingUI || currentTarget == null) return;
        currentTarget.restraint = (AciRestraint)value;
        RecalculateRating("Restraint");
    }
    private void HandleFireExposureChanged(int value) // (No changes needed inside this method)
    {
        if (isUpdatingUI || currentTarget == null) return;
        currentTarget.columnFireExposure = (AciColumnFireExposure)value;
        RecalculateRating("FireExposure");
    }
    private void HandleCoverChanged(string value) // (No changes needed inside this method)
    {
        if (isUpdatingUI || currentTarget == null) return;
        if (float.TryParse(value, out float coverValueMm))
        {
            currentTarget.actualCover_u = coverValueMm / 1000f;
            RecalculateRating("Cover");
        }
        else { Debug.LogWarning($"Invalid input for Cover: {value}"); }
    }
    private void HandleThicknessChanged(string value) // (No changes needed inside this method)
    {
        if (isUpdatingUI || currentTarget == null) return;
        if (float.TryParse(value, out float thicknessValueMm))
        {
            currentTarget.actualEquivalentThickness_te = thicknessValueMm / 1000f;
            RecalculateRating("Thickness");
        }
         else { Debug.LogWarning($"Invalid input for Thickness: {value}"); }
    }
    private void HandleLeastDimensionChanged(string value) // (No changes needed inside this method)
    {
        if (isUpdatingUI || currentTarget == null) return;
        if (float.TryParse(value, out float dimensionValueMm))
        {
            currentTarget.actualLeastDimension = dimensionValueMm / 1000f;
            RecalculateRating("LeastDimension");
        }
         else { Debug.LogWarning($"Invalid input for Least Dimension: {value}"); }
    }
    private void UpdateFieldVisibility(int elementTypeIndex) // (No changes needed inside this method)
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
    private void RecalculateRating(string changedProperty = "Unknown") // (No changes needed inside this method)
    {
        if (currentTarget == null) return;
        float oldRating = currentTarget.achievedFireResistanceRating;
        currentTarget.achievedFireResistanceRating = AciRatingCalculator.CalculateRating(currentTarget);
        Debug.Log($"Recalculated rating for {currentTarget.gameObject.name} due to change in '{changedProperty}'. New rating: {currentTarget.achievedFireResistanceRating:F2} hours (was {oldRating:F2})");
        // Update inspector text logic can be added here if needed
    }
} 