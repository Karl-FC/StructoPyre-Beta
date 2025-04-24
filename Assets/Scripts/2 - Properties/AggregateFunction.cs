using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class RealMaterialMapperUI : MonoBehaviour
{//e
    [SerializeField] private GameObject mappingRowPrefab;
    [SerializeField] private Transform scrollViewContent;
    [SerializeField] private Button confirmButton;
    
    // Remove the serialized list since we'll create materials dynamically
    private List<AggregateType> availableRealWorldMaterials = new List<AggregateType>();

    private Dictionary<string, TMP_Dropdown> mappingDropdowns = new Dictionary<string, TMP_Dropdown>();
    
    // Event for when mappings are confirmed
    public event Action<Dictionary<string, AggregateType>> OnMappingsConfirmed;

    private void Awake()
    {
        confirmButton.onClick.AddListener(ConfirmMappings);
        // Create materials for all types in the enum
        CreateMaterialInstances();
    }

    private void CreateMaterialInstances()
    {
        // Clear existing list
        availableRealWorldMaterials.Clear();
        
        // Create a material instance for each AciAggregateCategory value
        foreach (AciAggregateCategory category in Enum.GetValues(typeof(AciAggregateCategory)))
        {
            // Skip the "Unknown" category
            if (category == AciAggregateCategory.Unknown) continue;
            
            // Create a new material instance
            AggregateType material = ScriptableObject.CreateInstance<AggregateType>();
            material.aggregateCategory = category;
            material.realmaterialName = category.ToString();
            
            // Set appropriate default values
            material.defaultThickness = 0.1f;
            material.flammabilityRating = 0.5f;
            
            // Add to our list
            availableRealWorldMaterials.Add(material);
        }
        
        Debug.Log($"Created {availableRealWorldMaterials.Count} material instances for mapping UI");
    }

    private void Start()
    {
        gameObject.SetActive(false); // Start hidden
    }

    public void PopulateMappings(List<string> importedMaterialNames)
    {
        // Make sure we have material instances
        if (availableRealWorldMaterials.Count == 0)
        {
            CreateMaterialInstances();
        }
        
        // Clear any existing rows
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }
        mappingDropdowns.Clear();

        // Create dropdown options
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        foreach (var material in availableRealWorldMaterials)
        {
            options.Add(new TMP_Dropdown.OptionData(material.realmaterialName));
        }

        // Create a row for each imported material
        foreach (string materialName in importedMaterialNames)
        {
            GameObject row = Instantiate(mappingRowPrefab, scrollViewContent);
            
            // Set the material name text
            TextMeshProUGUI nameText = row.transform.Find("MaterialNameText").GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = materialName;
            }

            // Set up the dropdown
            TMP_Dropdown dropdown = row.transform.Find("RealMaterialDropdown").GetComponent<TMP_Dropdown>();
            if (dropdown != null)
            {
                dropdown.ClearOptions();
                dropdown.AddOptions(options);
                mappingDropdowns[materialName] = dropdown;
            }
        }
    }

    private void ConfirmMappings()
    {
        Dictionary<string, AggregateType> materialMappings = new Dictionary<string, AggregateType>();

        foreach (var mapping in mappingDropdowns)
        {
            string importedMaterialName = mapping.Key;
            TMP_Dropdown dropdown = mapping.Value;
            
            if (dropdown.value >= 0 && dropdown.value < availableRealWorldMaterials.Count)
            {
                AggregateType selectedMaterial = availableRealWorldMaterials[dropdown.value];
                materialMappings[importedMaterialName] = selectedMaterial;
                Debug.Log($"Mapped {importedMaterialName} to {selectedMaterial.realmaterialName}");
            }
            else
            {
                Debug.LogWarning($"No valid mapping for {importedMaterialName}");
            }
        }

        // Trigger the event with the mappings
        OnMappingsConfirmed?.Invoke(materialMappings);
        
        // Hide the panel
        UIManager.Instance.ShowSimulationGUI();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
