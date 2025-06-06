using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject MainMenuGUI;
    [SerializeField] private GameObject MainMenuBG;
    [SerializeField] private GameObject UIHeader;
    [SerializeField] private GameObject UIDpad;
    [SerializeField] private MonoBehaviour playerMovements;

    [SerializeField] private ToggleMouseCursor ToggleMouseScript;

    private RectTransform rectTransHeader;

    [SerializeField] private float headerHeight = 36f;
    [SerializeField] private float headerOpacity = 0.75f;

    // Visible muna yun background, buttons at cursor. Yun DPad hindi pa
    private void Awake()
    {
        // Only UIManager controls UI panels
        UIManager.Instance.ShowMainMenu();
        if (UIDpad != null)
        {
            UIDpad.SetActive(GlobalVariables.isDPadEnabled); // initially FALSE
        }
        Cursor.visible = true; // TRUE sa main menu
        DisablePlayerControls();
    }

    private void Start()
    {
        // Find the OpenFile component and subscribe to its OnModelLoaded event
        OpenFile fileOpener = FindObjectOfType<OpenFile>();
        if (fileOpener != null)
        {
            fileOpener.OnModelLoaded += OnModelImported;
        }
    }

    // Method to handle model import completion
    private void OnModelImported(GameObject loadedModel)
    {
        Debug.Log("Model imported SUMAKSES: " + loadedModel.name);
        // Enable player controls now that the model is ready
        EnablePlayerControls();
    }

    //Ilagay to sa buttons or kung ano pang script
    //STEP -1: Checking
    //STEP 0: DisablePlayerControls
    //STEP 1: DisableMainMenu
    //STEP 2: EnablePlayerControls
    //STEP 3: ImportModel

    private void CheckingMuna()
    {
        if (MainMenuGUI == null)
        {
            Debug.LogError("NASAN MainMenu UI??? Null po");
        }

        if (MainMenuBG == null)
        {
            Debug.LogError("NASAN MainMenuBG UI??? Null po");
        }

        if (UIHeader == null)
        {
            Debug.LogError("NASAN UIHeader??? Null po");
        }

        if (UIDpad == null)
        {
            Debug.LogError("NASAN UIDpad??? Null po");
        }

        if (playerMovements == null)
        {
            Debug.LogError("NASAN PlayerMovements??? Null po");
        }
    }

    private void DisablePlayerControls() //Disable controls
    {
        //Disable movement controls
        playerMovements.enabled = false;
        //Pang variable kasi trip ko
        GlobalVariables.playerCanMove = false;
        GlobalVariables.isMouseVisible = true;
        //Enable Mouse
        ToggleMouseScript.SetMouseVisible(true);
        //GlobalVariables.isDPadEnabled = false;
    }

    private void EnablePlayerControls()
    {
        // 1) enable the DPad UI. DAPAT nakaactivate
        UIDpad.SetActive(true);
        GlobalVariables.isDPadEnabled = true;
        
        // Then hide the cursor and enable movement sa SetMouseVisible
        ToggleMouseScript.SetMouseVisible(false);
        
        // Double-check the movement component is enabled
        if (!playerMovements.enabled)
        {
            playerMovements.enabled = true;
            Debug.Log("Forced enable of player movement component");
        }
        
        // Add logging for debugging
        Debug.Log("Player controls enabled: Movement=" + playerMovements.enabled + 
                  ", CanMove=" + GlobalVariables.playerCanMove + 
                  ", MouseVisible=" + GlobalVariables.isMouseVisible);
    }

    public void DisableMainMenu()
    {
        // Only UIManager controls UI panels
        UIManager.Instance.ShowMaterialMapper();
        HeaderAdjust();
    }

    public void HeaderAdjust()
    {
        rectTransHeader = UIHeader.GetComponent<RectTransform>();
        rectTransHeader.sizeDelta = new Vector2(rectTransHeader.sizeDelta.x, headerHeight);
        UIHeader.GetComponent<CanvasGroup>().alpha = headerOpacity;
    }

    public void ImportModel()
    {
        // First hide the menu UI
        DisableMainMenu();

        // Find and activate the OpenFile component
        OpenFile fileOpener = FindObjectOfType<OpenFile>();
        if (fileOpener != null)
        {
            // Make sure the GameObject with OpenFile script is active
            if (!fileOpener.gameObject.activeInHierarchy)
            {
                fileOpener.gameObject.SetActive(true);
            }

            // Trigger the file dialog
            fileOpener.OnClickOpen();
        }
        else
        {
            Debug.LogError("OpenFile component not found in the scene!");
        }
    }
}