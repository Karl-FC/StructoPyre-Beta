using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelToggle : MonoBehaviour
{
    [Tooltip("Assign the GameObject containing the SettingsMenuManager script here.")]
    [SerializeField] private SettingsMenuManager settingsMenuManager;

    private Button toggleButton;

    void Start()
    {
        toggleButton = GetComponent<Button>();
        if (toggleButton == null)
        {
            Debug.LogError("SettingsPanelToggle requires a Button component on the same GameObject!", this);
            return;
        }

        if (settingsMenuManager == null)
        {
            Debug.LogError("SettingsMenuManager reference is not assigned in the Inspector for SettingsPanelToggle!", this);
            return;
        }

        // Add listener to the button's click event
        toggleButton.onClick.AddListener(ToggleSettingsPanel);
    }

    public void ToggleSettingsPanel()
    {
        if (settingsMenuManager != null)
        {
            settingsMenuManager.TogglePanel();
        }
        else
        {
             Debug.LogError("Cannot toggle settings panel - SettingsMenuManager reference is missing!", this);
        }
    }

    void OnDestroy()
    {
         // Clean up listener to prevent memory leaks
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveListener(ToggleSettingsPanel);
        }
    }
} 