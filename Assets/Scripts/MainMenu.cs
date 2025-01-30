using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject MainMenuGUI;
    [SerializeField] private GameObject dpadUI;
    [SerializeField] private MonoBehaviour playerMovements;


    private void Awake()
    {
        // Initial setup
        MainMenuGUI.SetActive(true);
        if (dpadUI != null)
        {
            dpadUI.SetActive(false);
        }
        Cursor.visible = true; // Show cursor on main menu
       DisablePlayerControls();
    }

    public void ImportModel()
    {
        //Import Script here...
        //Make the panel inactive pag tapos na iimport.
        DisableMainMenu();
        EnablePlayerControls();
         if (dpadUI != null)
        {
            dpadUI.SetActive(true);
        }
         Cursor.visible = false;
    }


    //Ilagay to sa buttons or kung ano pang script
    public void DisableMainMenu()
    {
        MainMenuGUI.SetActive(false);
    }

     public void EnableMainMenu(){
           MainMenuGUI.SetActive(true);

            if (dpadUI != null){
             dpadUI.SetActive(false);
            }

            Cursor.visible = true; // Show cursor when main menu is active.
            DisablePlayerControls();
        }

     private void DisablePlayerControls()
    {
        //Disable your camera and player movement here.
        if (playerMovements
         != null)
        {
            playerMovements
            .enabled = false;
        }

    }

     private void EnablePlayerControls()
    {
        //Enable your camera and player movement here.
        if (playerMovements
         != null)
        {
            playerMovements
            .enabled = true;
        }


    }
}