using UnityEngine;
using UnityEngine.UI; // Required for Button

public enum UIMode { Simulator, Inspector } // Define the modes

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

    private UIMode currentMode;

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
        // InitUI(); // Keep or remove depending on whether you want main menu shown initially

        // Add listeners for the mode buttons
        if (inspectorModeButton != null)
            inspectorModeButton.onClick.AddListener(EnableInspectorMode);
        else
            Debug.LogError("Inspector Mode Button not assigned in UIManager.");

        if (simulatorModeButton != null)
            simulatorModeButton.onClick.AddListener(EnableSimulatorMode);
        else
            Debug.LogError("Simulator Mode Button not assigned in UIManager.");

        // Check if dependencies are assigned
        if (simulationControlsPanel == null)
             Debug.LogError("Simulation Controls Panel not assigned in UIManager.");
        if (faceInspector == null)
             Debug.LogError("Face Inspector component not assigned in UIManager.");

        // Set initial mode (e.g., Simulator mode) and update visuals
        SetMode(UIMode.Simulator); // Start in Simulator mode
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

    // --- Mode Switching Methods ---

    // Public methods called by button OnClick events
    public void EnableInspectorMode()
    {
        SetMode(UIMode.Inspector);
    }

    public void EnableSimulatorMode()
    {
        SetMode(UIMode.Simulator);
    }

    // Central method to handle mode switching logic
    private void SetMode(UIMode newMode)
    {
        // If the requested mode is already active, do nothing
        // if (newMode == currentMode) return; // Uncomment this line if you want clicking the active button to do nothing

        currentMode = newMode;
        Debug.Log($"Switching to {currentMode} Mode");

        if (faceInspector == null || simulationControlsPanel == null || inspectorModeButton == null || simulatorModeButton == null)
        {
            Debug.LogError("Cannot set mode, required components not assigned in UIManager.");
            return;
        }

        // Activate/Deactivate components based on mode
        faceInspector.SetInspectorActive(currentMode == UIMode.Inspector);
        simulationControlsPanel.SetActive(currentMode == UIMode.Simulator);

        // Update button visuals
        UpdateButtonVisuals();
    }

    // Helper to update button colors based on the current mode
    private void UpdateButtonVisuals()
    {
        SetButtonColor(inspectorModeButton, currentMode == UIMode.Inspector);
        SetButtonColor(simulatorModeButton, currentMode == UIMode.Simulator);
    }

    // Helper to set the color tint of a button
    private void SetButtonColor(Button button, bool isActive)
    {
        if (button == null) return;
        ColorBlock colors = button.colors;
        colors.colorMultiplier = isActive ? 1f : 0.8f; // Example: slightly dim inactive button
        // Or change the normalColor directly if preferred:
        // colors.normalColor = isActive ? activeModeColor : inactiveModeColor;
        // colors.highlightedColor = isActive ? activeModeColor * 0.9f : inactiveModeColor * 0.9f; // Adjust highlight too
        // colors.pressedColor = isActive ? activeModeColor * 0.7f : inactiveModeColor * 0.7f; // Adjust pressed too
        // colors.selectedColor = isActive ? activeModeColor : inactiveModeColor; // Adjust selected too
        button.colors = colors;

        // Alternative: Change Image component color if button has one
        Image img = button.GetComponent<Image>();
        if (img != null)
        {
            img.color = isActive ? activeModeColor : inactiveModeColor;
        }
    }
}
