using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject MainMenuGUI;
    [SerializeField] private GameObject MainMenuBG;
    [SerializeField] private GameObject dpadUI;
    [SerializeField] private MonoBehaviour playerMovements;


    private void Awake()
    {
// Initial setup
        MainMenuGUI.SetActive(true);
        MainMenuBG.SetActive(true);
                if (dpadUI != null)
                {
                    dpadUI.SetActive(GlobalVariables.isDPadEnabled); // initially FALSE
                }
        Cursor.visible = true; // TRUE sa main menu
       DisablePlayerControls();
    }



//Ilagay to sa buttons or kung ano pang script


     private void DisablePlayerControls()
    {
        //Disable controls
        if (playerMovements!= null)
        {
            playerMovements.enabled = false;
            GlobalVariables.playerCanMove = false;
        }
    }

     private void EnablePlayerControls()
    {
        //Enable controls
            if (playerMovements!= null)
            {
                playerMovements.enabled = true;
                GlobalVariables.playerCanMove = true;    
            }
        //Disable Mouse
            Cursor.visible = GlobalVariables.isMouseVisible;
        //Enable DPad
            if (dpadUI != null)
                {
                    dpadUI.SetActive(true);
                    GlobalVariables.isDPadEnabled = true; //Set to TRUE
                }
    }

        public void DisableMainMenu()
        {
            MainMenuGUI.SetActive(false);
            MainMenuBG.SetActive(false);
        }


        public void EnableMainMenu(){
            MainMenuGUI.SetActive(true);
            MainMenuBG.SetActive(true);

                if (dpadUI != null){
                dpadUI.SetActive(false);
                }

                Cursor.visible = true;
                DisablePlayerControls();
            }



    public void ImportModel()
    {
        //Import Script here...
        //Make the panel inactive pag tapos na iimport.
        DisableMainMenu();
        EnablePlayerControls();
          /*if (dpadUI != null)
                {
                    dpadUI.SetActive(true);
                    GlobalVariables.isDPadEnabled = true; //Set to TRUE
                }*/
    }
}