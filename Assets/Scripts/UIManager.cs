 using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject MainImportPanel;
    public GameObject MaterialMapperPanel;
    public GameObject BackgroundPanel;
    public GameObject SimulationGUIPanel;

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
        InitUI();
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
}
