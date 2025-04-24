using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SettingsMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] public GameObject settingsPanel;
    [SerializeField] private TMP_Text globalUnitsText;
    
    [Header("Simulation Settings")]
    [SerializeField] private TMP_Text simulationTimeText;
    [SerializeField] private TMP_InputField simulationTimeInputField;
    [SerializeField] private Slider simulationSpeedSlider;
    [SerializeField] private TMP_Text simulationSpeedValueText;
    
    [Header("Controls Settings")]
    [SerializeField] private Toggle dpadToggle;
    [SerializeField] private TMP_InputField cameraSpeedInputField;
    
    [Header("Fire Spread Settings")]
    [SerializeField] private Toggle fireSpreadingToggle;
    [SerializeField] private TMP_InputField fireSpreadRadiusInputField;
    [SerializeField] private TMP_InputField spreadThresholdInputField;
    [SerializeField] private TMP_InputField checkIntervalInputField;
    
    // References to managers
    private SimulationManager simulationManager;
    private CameraMovement cameraMovement;
    private DPadController dpadController;
    
    // Used to avoid triggering events when updating UI
    private bool isUpdatingUI = false;
    
    private void Start()
    {
        // Find necessary managers
        simulationManager = SimulationManager.Instance;
        if (simulationManager == null)
        {
            Debug.LogError("SettingsMenuManager requires SimulationManager to be in the scene!");
        }
        
        // Find camera movement component in the scene
        cameraMovement = FindObjectOfType<CameraMovement>();
        if (cameraMovement == null)
        {
            Debug.LogWarning("Could not find CameraMovement in the scene!");
        }
        
        // Find DPad controller in the scene
        dpadController = FindObjectOfType<DPadController>();
        if (dpadController == null)
        {
            Debug.LogWarning("Could not find DPadController in the scene!");
        }
        
        SetupInputFields();
        SetupListeners();
        
        // Initialize panel state
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        // Populate initial values after everything is set up
        RefreshAllSettings();
    }
    
    private void SetupInputFields()
    {
        // Set content type for input fields to allow only decimal numbers
        if (simulationTimeInputField != null) simulationTimeInputField.contentType = TMP_InputField.ContentType.DecimalNumber;
        if (cameraSpeedInputField != null) cameraSpeedInputField.contentType = TMP_InputField.ContentType.DecimalNumber;
        if (fireSpreadRadiusInputField != null) fireSpreadRadiusInputField.contentType = TMP_InputField.ContentType.DecimalNumber;
        if (spreadThresholdInputField != null) spreadThresholdInputField.contentType = TMP_InputField.ContentType.DecimalNumber;
        if (checkIntervalInputField != null) checkIntervalInputField.contentType = TMP_InputField.ContentType.DecimalNumber;
    }
    
    private void SetupListeners()
    {
        // Add listeners to UI controls
        if (simulationTimeInputField != null)
        {
            simulationTimeInputField.onEndEdit.AddListener(OnSimulationTimeChanged);
        }
        
        if (simulationSpeedSlider != null)
        {
            simulationSpeedSlider.onValueChanged.AddListener(OnSimulationSpeedChanged);
        }
        
        if (dpadToggle != null)
        {
            dpadToggle.onValueChanged.AddListener(OnDpadToggleChanged);
        }
        
        if (cameraSpeedInputField != null)
        {
            cameraSpeedInputField.onEndEdit.AddListener(OnCameraSpeedChanged);
        }
        
        if (fireSpreadingToggle != null)
        {
            fireSpreadingToggle.onValueChanged.AddListener(OnFireSpreadingToggleChanged);
        }
        
        if (fireSpreadRadiusInputField != null)
        {
            fireSpreadRadiusInputField.onEndEdit.AddListener(OnFireSpreadRadiusChanged);
        }
        
        if (spreadThresholdInputField != null)
        {
            spreadThresholdInputField.onEndEdit.AddListener(OnSpreadThresholdChanged);
        }
        
        if (checkIntervalInputField != null)
        {
            checkIntervalInputField.onEndEdit.AddListener(OnCheckIntervalChanged);
        }
    }
    
    private void Update()
    {
        // Update dynamic display label for simulation time ONLY if the input field isn't focused
        if (simulationManager != null && simulationTimeText != null)
        {
             if (simulationTimeInputField == null || !simulationTimeInputField.isFocused)
             {
                 simulationTimeText.text = $"Sim Time: {simulationManager.simulationTimeSeconds:F1} s";
                 // Also update the input field if not focused, to reflect ongoing time
                 if (simulationTimeInputField != null && !isUpdatingUI) // Avoid self-triggering from Refresh
                 {
                    // Temporarily remove listener to prevent loop
                    simulationTimeInputField.onEndEdit.RemoveListener(OnSimulationTimeChanged);
                    simulationTimeInputField.text = simulationManager.simulationTimeSeconds.ToString("F1");
                    // Re-add listener
                    simulationTimeInputField.onEndEdit.AddListener(OnSimulationTimeChanged);
                 }
             }
             // If focused, the label might show the 'live' time while user edits the field
             else if (simulationTimeInputField != null && simulationTimeInputField.isFocused)
             {
                 simulationTimeText.text = $"Sim Time: {simulationManager.simulationTimeSeconds:F1} s (Editing...)";
             }
        }
    }
    
    public void TogglePanel()
    {
        if (settingsPanel != null)
        {
            bool newState = !settingsPanel.activeSelf;
            settingsPanel.SetActive(newState);
            
            if (newState)
            {
                RefreshAllSettings();
            }
        }
    }
    
    public void ShowPanel()
    {
        if (settingsPanel != null && !settingsPanel.activeSelf)
        {
            settingsPanel.SetActive(true);
            RefreshAllSettings();
        }
    }
    
    public void HidePanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
    
    private void RefreshAllSettings()
    {
        isUpdatingUI = true;
        
        // Update global units display
        if (globalUnitsText != null)
        {
            globalUnitsText.text = $"Units: {GlobalVariables.DisplayUnitSystem}";
        }
        
        // Update simulation time and speed
        if (simulationManager != null)
        {
            if (simulationTimeInputField != null)
            {
                simulationTimeInputField.text = simulationManager.simulationTimeSeconds.ToString("F1");
            }
             if (simulationTimeText != null) // Update label on refresh too
            {
                 simulationTimeText.text = $"Sim Time: {simulationManager.simulationTimeSeconds:F1} s";
            }
            if (simulationSpeedSlider != null)
            {
                simulationSpeedSlider.value = simulationManager.simulationTimeScale;
                 if (simulationSpeedValueText != null)
                 {
                    simulationSpeedValueText.text = simulationManager.simulationTimeScale.ToString("F1") + "x";
                 }
            }
        }
        
        // Update camera and DPad controls
        if (dpadToggle != null)
        {
            dpadToggle.isOn = GlobalVariables.isDPadEnabled;
        }
        
        if (cameraMovement != null && cameraSpeedInputField != null)
        {
            cameraSpeedInputField.text = cameraMovement.walkSpeed.ToString("F1");
        }
        
        // Update fire spread settings
        FireIntegrityTracker[] trackers = FindObjectsOfType<FireIntegrityTracker>(true);
        if (trackers.Length > 0)
        {
            FireIntegrityTracker referenceTracker = trackers[0];
            
            if (fireSpreadingToggle != null)
            {
                fireSpreadingToggle.isOn = referenceTracker.canSpreadFire;
            }
            
            if (fireSpreadRadiusInputField != null)
            {
                fireSpreadRadiusInputField.text = referenceTracker.spreadRadius.ToString("F1");
            }
            
            if (spreadThresholdInputField != null)
            {
                spreadThresholdInputField.text = referenceTracker.spreadThresholdSeconds.ToString("F1");
            }
            
            if (checkIntervalInputField != null)
            {
                checkIntervalInputField.text = referenceTracker.spreadCheckInterval.ToString("F1");
            }
        }
        else
        {
            Debug.LogWarning("SettingsMenuManager: No FireIntegrityTrackers found to refresh settings from.");
            // Optionally disable or clear fire setting UI elements if no trackers exist
        }
        
        isUpdatingUI = false;
    }
    
    #region Event Handlers
    
    private void OnSimulationTimeChanged(string value)
    {
        if (isUpdatingUI || simulationManager == null) return;

        if (float.TryParse(value, out float timeValue))
        {
            // Ensure time is non-negative
            timeValue = Mathf.Max(0f, timeValue);
            simulationManager.simulationTimeSeconds = timeValue;
            // Optionally, reset simulation if time is set back?
            // simulationManager.ResetSimulation(); 
        }
        else
        {
            Debug.LogWarning($"Invalid simulation time input: {value}. Reverting.");
            // Revert input field to current simulation time
            simulationTimeInputField.text = simulationManager.simulationTimeSeconds.ToString("F1");
        }
        // Update the dynamic label immediately after change
        if (simulationTimeText != null)
        {
            simulationTimeText.text = $"Sim Time: {simulationManager.simulationTimeSeconds:F1} s";
        }
    }
    
    private void OnSimulationSpeedChanged(float value)
    {
        if (isUpdatingUI || simulationManager == null) return;
        
        // Value comes directly from slider, clamping happens via slider settings
        simulationManager.simulationTimeScale = value;
        
        if (simulationSpeedValueText != null)
        {
            simulationSpeedValueText.text = value.ToString("F1") + "x";
        }
    }
    
    private void OnDpadToggleChanged(bool isOn)
    {
        if (isUpdatingUI) return;
        
        GlobalVariables.isDPadEnabled = isOn;
        
        if (dpadController != null)
        {
            dpadController.SetDPadForcedState(isOn);
        }
        else
        {
            Debug.LogWarning("Cannot toggle DPad - DPadController reference is missing");
        }
    }
    
    private void OnCameraSpeedChanged(string value)
    {
        if (isUpdatingUI || cameraMovement == null) return;
        
        if (float.TryParse(value, out float speedValue))
        {
            speedValue = Mathf.Clamp(speedValue, 0.1f, 100f); 
            cameraMovement.walkSpeed = speedValue;
             // Update the input field to show the potentially clamped value immediately
            cameraSpeedInputField.text = speedValue.ToString("F1");
        }
        else
        {
            Debug.LogWarning($"Invalid camera speed input: {value}. Reverting.");
            cameraSpeedInputField.text = cameraMovement.walkSpeed.ToString("F1");
        }
    }
    
    private void OnFireSpreadingToggleChanged(bool isOn)
    {
        if (isUpdatingUI) return;
        
        FireIntegrityTracker[] trackers = FindObjectsOfType<FireIntegrityTracker>(true);
        foreach (FireIntegrityTracker tracker in trackers)
        {
            tracker.canSpreadFire = isOn;
        }
    }
    
    private void OnFireSpreadRadiusChanged(string value)
    {
        if (isUpdatingUI) return;
        
        if (float.TryParse(value, out float radiusValue))
        {
            radiusValue = Mathf.Max(0f, radiusValue);
             // Update the input field to show the potentially clamped value immediately
            fireSpreadRadiusInputField.text = radiusValue.ToString("F1");
            
            FireIntegrityTracker[] trackers = FindObjectsOfType<FireIntegrityTracker>(true);
            foreach (FireIntegrityTracker tracker in trackers)
            {
                tracker.spreadRadius = radiusValue;
            }
        }
        else
        {
            Debug.LogWarning($"Invalid fire spread radius input: {value}. Reverting.");
            FireIntegrityTracker[] trackers = FindObjectsOfType<FireIntegrityTracker>(true);
            if (trackers.Length > 0)
            {
                 fireSpreadRadiusInputField.text = trackers[0].spreadRadius.ToString("F1");
            }
        }
    }
    
    private void OnSpreadThresholdChanged(string value)
    {
        if (isUpdatingUI) return;
        
        if (float.TryParse(value, out float thresholdValue))
        {
            thresholdValue = Mathf.Max(0f, thresholdValue); 
             // Update the input field to show the potentially clamped value immediately
            spreadThresholdInputField.text = thresholdValue.ToString("F1");
            
            FireIntegrityTracker[] trackers = FindObjectsOfType<FireIntegrityTracker>(true);
            foreach (FireIntegrityTracker tracker in trackers)
            {
                tracker.spreadThresholdSeconds = thresholdValue;
            }
        }
        else
        {
            Debug.LogWarning($"Invalid spread threshold input: {value}. Reverting.");
            FireIntegrityTracker[] trackers = FindObjectsOfType<FireIntegrityTracker>(true);
            if (trackers.Length > 0)
            {
                 spreadThresholdInputField.text = trackers[0].spreadThresholdSeconds.ToString("F1");
            }
        }
    }
    
    private void OnCheckIntervalChanged(string value)
    {
        if (isUpdatingUI) return;
        
        if (float.TryParse(value, out float intervalValue))
        {
            intervalValue = Mathf.Max(0.1f, intervalValue);
             // Update the input field to show the potentially clamped value immediately
            checkIntervalInputField.text = intervalValue.ToString("F1");
            
            FireIntegrityTracker[] trackers = FindObjectsOfType<FireIntegrityTracker>(true);
            foreach (FireIntegrityTracker tracker in trackers)
            {
                tracker.spreadCheckInterval = intervalValue;
            }
        }
        else
        {
            Debug.LogWarning($"Invalid check interval input: {value}. Reverting.");
            FireIntegrityTracker[] trackers = FindObjectsOfType<FireIntegrityTracker>(true);
            if (trackers.Length > 0)
            {
                 checkIntervalInputField.text = trackers[0].spreadCheckInterval.ToString("F1");
            }
        }
    }
    
    #endregion
} 