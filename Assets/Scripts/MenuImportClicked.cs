using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject MainMenuGUI;
    [SerializeField] private GameObject MainMenuBG;
    [SerializeField] private GameObject UIHeader;
    [SerializeField] private GameObject UIDpad;
    [SerializeField] private MonoBehaviour playerMovements;

    private RectTransform rectTransHeader;

    public float headerHeight;




// Visible muna yun background, buttons at cursor. Yun DPad hindi pa
private void Awake()
{

    MainMenuGUI.SetActive(true);
    MainMenuBG.SetActive(true);
            if (UIDpad != null)
            {
                UIDpad.SetActive(GlobalVariables.isDPadEnabled); // initially FALSE
            }
    Cursor.visible = true; // TRUE sa main menu
    DisablePlayerControls();
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
    {Debug.LogError("NASAN MainMenu UI??? Null po");}

    if (MainMenuBG == null)
    {Debug.LogError("NASAN MainMenuBG UI??? Null po");}

    if (UIHeader == null)
    {Debug.LogError("NASAN UIHeader??? Null po");}

    if (UIDpad == null)
    {Debug.LogError("NASAN UIDpad??? Null po");}

    if (playerMovements == null)
    {Debug.LogError("NASAN PlayerMovements??? Null po");}
}


     private void DisablePlayerControls() //Disable controls
    {   
        playerMovements.enabled = false;
        GlobalVariables.playerCanMove = false;
    }

     private void EnablePlayerControls()
    {
        //Enable controls
            playerMovements.enabled = true;
            GlobalVariables.playerCanMove = true;  
        //Disable Mouse
                Cursor.visible = GlobalVariables.isMouseVisible;
        //Enable DPad
                    UIDpad.SetActive(true);
                    GlobalVariables.isDPadEnabled = true; //Set to TRUE
    }

        public void DisableMainMenu()
        {
            MainMenuGUI.SetActive(false);
            MainMenuBG.SetActive(false);            
        }

        public void HeaderAdjust(float height, float opacity)
        {
            rectTransHeader = UIHeader.GetComponent<RectTransform>();
            rectTransHeader.sizeDelta = new Vector2(rectTransHeader.sizeDelta.x, height);
            UIHeader.GetComponent<CanvasGroup>().alpha = opacity;
            
        }

    public void ImportModel()
    {
        //Import Script here...
        //Make the panel inactive pag tapos na iimport.
        DisableMainMenu();
        EnablePlayerControls();
                   /*UIDpad.SetActive(true);
                        GlobalVariables.isDPadEnabled = true; //Set to TRUE*/
    }
}