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
    [SerializeField] private TMP_Text coverUnitLabel;
    [SerializeField] private GameObject thicknessSection;
    [SerializeField] private TMP_InputField actualThicknessField;
    [SerializeField] private TMP_Text thicknessUnitLabel;
    [SerializeField] private GameObject leastDimensionSection;
    [SerializeField] private TMP_InputField actualLeastDimensionField;
    [SerializeField] private TMP_Text leastDimensionUnitLabel;

    private MaterialProperties currentTarget;
    private bool isUpdatingUI = false;
    private const float METERS_TO_INCHES = 39.3701f;

    private void Start()
    {
        if (faceInspector == null)
        {
            Debug.LogError("MaterialPropertyEditor requires the FaceInspector reference to be assigned in the Inspector!");
            this.enabled = false;
            return;
        }
        
        InitializeDropdownsAndListeners();
        UpdateDimensionLabels();

        if (editorPanel != null) editorPanel.SetActive(false);
    }

    private void InitializeDropdownsAndListeners()
    {
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
        if (materialTypeDropdown != null)
        {
            materialTypeDropdown.ClearOptions();
            materialTypeDropdown.AddOptions(new System.Collections.Generic.List<string>(Enum.GetNames(typeof(AciAggregateCategory))));
        }
        
        elementTypeDropdown?.onValueChanged.AddListener(HandleElementTypeChanged);
        restraintDropdown?.onValueChanged.AddListener(HandleRestraintChanged);
        columnFireExposureDropdown?.onValueChanged.AddListener(HandleFireExposureChanged);
        actualCoverField?.onEndEdit.AddListener(HandleCoverChanged);
        actualThicknessField?.onEndEdit.AddListener(HandleThicknessChanged);
        actualLeastDimensionField?.onEndEdit.AddListener(HandleLeastDimensionChanged);
        materialTypeDropdown?.onValueChanged.AddListener(HandleMaterialTypeChanged);
    }

    private void Update()
    {
        if (faceInspector == null || editorPanel == null || !editorPanel.activeSelf)
        {
             return;
        }

        bool inspectorActive = faceInspector.IsInspectorCurrentlyActive;

        if (inspectorActive)
        {
            MaterialProperties targetProps = faceInspector.CurrentlyInspectedProperties;

            if (targetProps != null && currentTarget != targetProps)
            {
                 PopulateEditorFields(targetProps);
            }
            else if (targetProps == null && currentTarget != null)
            {
                 currentTarget = null;
            }
        }
        else
        {
            HideEditor();
        }
    }

    private void PopulateEditorFields(MaterialProperties target)
    {
        if (target == null)
        {
            Debug.LogWarning("PopulateEditorFields called with null target.");
             currentTarget = null;
            return;
        }

        currentTarget = target;
        isUpdatingUI = true;

        if (objectNameText != null)
        {
            if (currentTarget.realMaterial != null)
            {
                objectNameText.text = $"{currentTarget.gameObject.name} ({currentTarget.realMaterial.realmaterialName})";
            }
            else
            {
                objectNameText.text = $"{currentTarget.gameObject.name}";
            }
        }

        UpdateDimensionLabels();

        elementTypeDropdown.value = (int)currentTarget.elementType;
        restraintDropdown.value = (int)currentTarget.restraint;
        columnFireExposureDropdown.value = (int)currentTarget.columnFireExposure;

        if (GlobalVariables.DisplayUnitSystem == UnitSystem.Metric)
        {
            actualCoverField.text = (currentTarget.actualCover_u * 1000f).ToString("F1");
            actualThicknessField.text = (currentTarget.actualEquivalentThickness_te * 1000f).ToString("F1");
            actualLeastDimensionField.text = (currentTarget.actualLeastDimension * 1000f).ToString("F1");
        }
        else
        {
            actualCoverField.text = (currentTarget.actualCover_u * METERS_TO_INCHES).ToString("F2");
            actualThicknessField.text = (currentTarget.actualEquivalentThickness_te * METERS_TO_INCHES).ToString("F2");
            actualLeastDimensionField.text = (currentTarget.actualLeastDimension * METERS_TO_INCHES).ToString("F2");
        }

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

        isUpdatingUI = false;
    }

    public void ShowPanelIfTargetSelected()
    {
        if (editorPanel == null || faceInspector == null) return;

        if (faceInspector.CurrentlyInspectedProperties != null)
        {
             editorPanel.SetActive(true);
             PopulateEditorFields(faceInspector.CurrentlyInspectedProperties);
        }
        else
        {
             editorPanel.SetActive(false);
             currentTarget = null;
        }
    }

    public void HideEditor()
    {
        if (editorPanel != null) editorPanel.SetActive(false);
        currentTarget = null;
    }

    public void TogglePanel()
    {
         if (editorPanel != null)
         {
            bool shouldBeActive = !editorPanel.activeSelf;
            editorPanel.SetActive(shouldBeActive);
            

            if (shouldBeActive && faceInspector != null && faceInspector.IsInspectorCurrentlyActive)
            {
                 PopulateEditorFields(faceInspector.CurrentlyInspectedProperties);
            }
            else if (!shouldBeActive)
            {
                 currentTarget = null;
            }
         }
    }

    private void HandleElementTypeChanged(int value) { if (isUpdatingUI || currentTarget == null) return; currentTarget.elementType = (AciElementType)value; UpdateFieldVisibility(value); RecalculateRating("ElementType"); }
    private void HandleRestraintChanged(int value) { if (isUpdatingUI || currentTarget == null) return; currentTarget.restraint = (AciRestraint)value; RecalculateRating("Restraint"); }
    private void HandleFireExposureChanged(int value) { if (isUpdatingUI || currentTarget == null) return; currentTarget.columnFireExposure = (AciColumnFireExposure)value; RecalculateRating("FireExposure"); }
    private void HandleCoverChanged(string value)
    {
        if (isUpdatingUI || currentTarget == null) return;
        if (float.TryParse(value, out float v))
        {
            if (GlobalVariables.DisplayUnitSystem == UnitSystem.Metric)
            {
                currentTarget.actualCover_u = v / 1000f;
            }
            else
            {
                currentTarget.actualCover_u = v / METERS_TO_INCHES;
            }
            RecalculateRating("Cover");
        }
    }
    private void HandleThicknessChanged(string value)
    {
        if (isUpdatingUI || currentTarget == null) return;
        if (float.TryParse(value, out float v))
        {
            if (GlobalVariables.DisplayUnitSystem == UnitSystem.Metric)
            {
                currentTarget.actualEquivalentThickness_te = v / 1000f;
            }
            else
            {
                currentTarget.actualEquivalentThickness_te = v / METERS_TO_INCHES;
            }
            RecalculateRating("Thickness");
        }
    }
    private void HandleLeastDimensionChanged(string value)
    {
        if (isUpdatingUI || currentTarget == null) return;
        if (float.TryParse(value, out float v))
        {
            if (GlobalVariables.DisplayUnitSystem == UnitSystem.Metric)
            {
                currentTarget.actualLeastDimension = v / 1000f;
            }
            else
            {
                currentTarget.actualLeastDimension = v / METERS_TO_INCHES;
            }
            RecalculateRating("LeastDimension");
        }
    }
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
        
        // Create a new instance of the material to avoid changing the original asset
        AggregateType originalMaterial = currentTarget.realMaterial;
        AggregateType uniqueMaterial = ScriptableObject.CreateInstance<AggregateType>();
        
        // Copy all properties
        uniqueMaterial.realmaterialName = originalMaterial.realmaterialName;
        uniqueMaterial.defaultThickness = originalMaterial.defaultThickness;
        uniqueMaterial.flammabilityRating = originalMaterial.flammabilityRating;
        
        // Set the new aggregate category
        uniqueMaterial.aggregateCategory = (AciAggregateCategory)value;
        
        // Update the name to reflect the category
        uniqueMaterial.realmaterialName = Enum.GetName(typeof(AciAggregateCategory), uniqueMaterial.aggregateCategory);
        
        // Assign the new instance to this object
        currentTarget.realMaterial = uniqueMaterial;
        
        RecalculateRating("AggregateCategory");
        
        // Update the UI display
        objectNameText.text = $"{currentTarget.gameObject.name} ({uniqueMaterial.realmaterialName})";
        
        Debug.Log($"Created unique material for {currentTarget.gameObject.name} with category: {uniqueMaterial.aggregateCategory}");
    }

    private void UpdateDimensionLabels()
    {
        string unitSuffix = (GlobalVariables.DisplayUnitSystem == UnitSystem.Metric) ? "(mm)" : "(in)";
        if (coverUnitLabel != null) coverUnitLabel.text = unitSuffix;
        if (thicknessUnitLabel != null) thicknessUnitLabel.text = unitSuffix;
        if (leastDimensionUnitLabel != null) leastDimensionUnitLabel.text = unitSuffix;
    }

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
        if (materialTypeDropdown != null) materialTypeDropdown.gameObject.SetActive(parametersSection != null && parametersSection.activeSelf);

        UpdateDimensionLabels();
    }
    private void RecalculateRating(string changedProperty = "Unknown")
    {
        if (currentTarget == null) return;
        float oldRating = currentTarget.achievedFireResistanceRating;
        currentTarget.achievedFireResistanceRating = AciRatingCalculator.CalculateRating(currentTarget);
        Debug.Log($"Recalculated rating for {currentTarget.gameObject.name} due to change in '{changedProperty}'. New rating: {currentTarget.achievedFireResistanceRating:F2} hours (was {oldRating:F2})");
    }
} 