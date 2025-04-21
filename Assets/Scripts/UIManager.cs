using UnityEngine;
using UnityEngine.UI; // Required for Button

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject MainImportPanel;
    public GameObject MaterialMapperPanel;
    public GameObject BackgroundPanel;
    public GameObject SimulationGUIPanel;

    [Header("Mode Controls")]
    [SerializeField] private Button inspectorModeButton;
    [SerializeField] private Button simulatorModeButton;
    [SerializeField] private GameObject simulationControlsPanel; // Parent object for Start/Pause/Reset etc.
    [SerializeField] private FaceInspector faceInspector; // Assign the Camera's FaceInspector component

    [Header("Button Visuals")]
    [SerializeField] private Color activeModeColor = Color.grey;   // Color for the active button
    [SerializeField] private Color inactiveModeColor = Color.white; // Color for the inactive button

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // InitUI(); // Optional: Set initial panel visibility

        // Add listeners for the mode buttons
        if (inspectorModeButton != null)
            inspectorModeButton.onClick.AddListener(ToggleInspectorMode); // Changed listener
        else
            Debug.LogError("Inspector Mode Button not assigned in UIManager.");

        if (simulatorModeButton != null)
            simulatorModeButton.onClick.AddListener(ToggleSimulatorMode); // Changed listener
        else
            Debug.LogError("Simulator Mode Button not assigned in UIManager.");

        // Check if dependencies are assigned!
        if (simulationControlsPanel == null)
             Debug.LogError("Simulation Controls Panel not assigned in UIManager.");
        if (faceInspector == null)
             Debug.LogError("Face Inspector component not assigned in UIManager.");

        // Set initial visual state based on component defaults
        if (faceInspector != null && inspectorModeButton != null)
            SetButtonColor(inspectorModeButton, faceInspector.gameObject.activeSelf); // Assuming FaceInspector starts enabled/disabled via its GameObject or internal flag
        if (simulationControlsPanel != null && simulatorModeButton != null)
            SetButtonColor(simulatorModeButton, simulationControlsPanel.activeSelf);
    }

    /// <summary>
    /// Set the initial UI state: show main menu and background, hide others.
    /// </summary>
    public void InitUI()
    {
        SetPanel(MainMenu: true, Mapper: false, Background: true, SimGUI: false);
    }

    /// <summary>
    /// Show the main menu (Import button) and background.
    /// </summary>
    public void ShowMainMenu()
    {
        SetPanel(MainMenu: true, Mapper: false, Background: true, SimGUI: false);
    }

    /// <summary>
    /// Show the material mapping screen and background.
    /// </summary>
    public void ShowMaterialMapper()
    {
        SetPanel(MainMenu: false, Mapper: true, Background: true, SimGUI: false);
    }

    /// <summary>
    /// Show the simulation GUI only (enable player controls here if needed).
    /// </summary>
    public void ShowSimulationGUI()
    {
        SetPanel(MainMenu: false, Mapper: false, Background: false, SimGUI: true);
    }

    /// <summary>
    /// Helper to set panel visibility.
    /// </summary>
    private void SetPanel(bool MainMenu, bool Mapper, bool Background, bool SimGUI)
    {
        if (MainImportPanel != null) MainImportPanel.SetActive(MainMenu);
        if (MaterialMapperPanel != null) MaterialMapperPanel.SetActive(Mapper);
        if (BackgroundPanel != null) BackgroundPanel.SetActive(Background);
        if (SimulationGUIPanel != null) SimulationGUIPanel.SetActive(SimGUI);
    }

    // --- Mode Switching Methods (Now Independent Toggles) ---

    public void ToggleInspectorMode()
    {
        if (faceInspector == null || inspectorModeButton == null) return;

        bool newState = !faceInspector.IsInspectorCurrentlyActive;
        faceInspector.SetInspectorActive(newState);
        SetButtonColor(inspectorModeButton, newState);
        Debug.Log($"Inspector Mode Toggled: {newState}");
    }

    public void ToggleSimulatorMode()
    {
        if (simulationControlsPanel == null || simulatorModeButton == null) return;

        bool newState = !simulationControlsPanel.activeSelf; // Check current state and flip it
        simulationControlsPanel.SetActive(newState);
        SetButtonColor(simulatorModeButton, newState);
        Debug.Log($"Simulator Controls Toggled: {newState}");
    }

    // Helper to set the color tint of a button (Keep this helper)
    private void SetButtonColor(Button button, bool isActive)
    {
        if (button == null) return;
        // Using Image component color change - preferred for direct color setting
        Image img = button.GetComponent<Image>();
        if (img != null)
        {
            img.color = isActive ? activeModeColor : inactiveModeColor;
        }
        // Alternative using ColorBlock:
        // ColorBlock colors = button.colors;
        // colors.colorMultiplier = isActive ? 1f : 0.8f; // Adjust multiplier or colors directly
        // button.colors = colors;
    }
}
