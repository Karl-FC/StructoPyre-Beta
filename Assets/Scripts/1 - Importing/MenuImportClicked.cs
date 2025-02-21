using UnityEngine;
using System.Collections;
using Dummiesman;
using System.IO;
using SFB;
using UnityEngine.Networking;

#if UNITY_WEBGL && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    //Removed PRIVATE as it is not needed and can cause errors
    public static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);
#endif

public class MainMenu : MonoBehaviour
{
    //GUI THINGZ
    [SerializeField] private GameObject MainMenuGUI;
    [SerializeField] private GameObject MainMenuBG;
    [SerializeField] private GameObject UIHeader;
    [SerializeField] private GameObject UIDpad;
    [SerializeField] private MonoBehaviour playerMovements;

    [SerializeField] private ToggleMouseCursor ToggleMouseScript;

    private RectTransform rectTransHeader;

    [SerializeField] private float headerHeight = 36f;
    [SerializeField] private float headerOpacity = 0.75f;

    //IMPORT NAAAAA
    [SerializeField] private Transform modelSpawnPoint;
    private GameObject loadedObject;



// Visible muna yun background, buttons at cursor. Yun DPad hindi pa
private void Awake()
{
    // Ensure this GameObject is active
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

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
    {   //Disable movement controls
        playerMovements.enabled = false;
        //Pang variable kasi trip ko
        GlobalVariables.playerCanMove = false;
        GlobalVariables.isMouseVisible = true;
        //Enable Mouse
        ToggleMouseScript.ToggleCursorFunc(true);
            //GlobalVariables.isDPadEnabled = false;
    }

     private void EnablePlayerControls()
    {
        //Enable controls
            playerMovements.enabled = true;
            GlobalVariables.playerCanMove = true;  
        //Disable Mouse
                ToggleMouseScript.ToggleCursorFunc(false);
                GlobalVariables.playerCanMove = true;
                GlobalVariables.isMouseVisible = false;
        //Enable DPad
                    UIDpad.SetActive(true);
                    GlobalVariables.isDPadEnabled = true; //Set to TRUE
    }

        public void DisableMainMenu()
        {
            MainMenuGUI.SetActive(false);
            MainMenuBG.SetActive(false);
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
    if (!gameObject.activeSelf)
        gameObject.SetActive(true);

    #if UNITY_WEBGL && !UNITY_EDITOR
        UploadFile(gameObject.name, "OnFileUpload", ".obj", false);
    #else
        var extensions = new [] {
            new ExtensionFilter("3D Models", "obj"),
            new ExtensionFilter("All Files", "*"),
        };
        var paths = StandaloneFileBrowser.OpenFilePanel("Import 3D Model", "", extensions, false);
        if (paths.Length > 0)
        {
            StartCoroutine(LoadModel(new System.Uri(paths[0]).AbsoluteUri));
            DisableMainMenu();
            EnablePlayerControls();
        }
    #endif
    }

    // Called from browser in WebGL
    public void OnFileUpload(string url)
    {
        StartCoroutine(LoadModel(url));
    }

    private IEnumerator LoadModel(string url)
{
    Debug.Log($"Starting to load model from: {url}");
    using (UnityWebRequest www = UnityWebRequest.Get(url))
    {
        Debug.Log("Sending web request...");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error loading model: {www.error}");
            yield break;
        }

        try
        {
            Debug.Log($"Downloaded data size: {www.downloadHandler.data.Length} bytes");
            var textStream = new MemoryStream(www.downloadHandler.data);

            if (loadedObject != null)
            {
                Debug.Log("Destroying previous model");
                Destroy(loadedObject);
            }

            Debug.Log("Creating OBJLoader...");
            var loader = new OBJLoader();
            Debug.Log("Loading model from stream...");
            loadedObject = loader.Load(textStream);

            if (loadedObject != null)
{
    Debug.Log($"Model loaded: {loadedObject.name}");
    if (modelSpawnPoint != null)
    {
        loadedObject.transform.position = modelSpawnPoint.position;
        Debug.Log($"Model positioned at: {modelSpawnPoint.position}");
        // Only disable UI and enable controls after successful load
        DisableMainMenu();
        EnablePlayerControls();
    }
    else
    {
        Debug.LogWarning("modelSpawnPoint is null!");
    }
}
            else
            {
                Debug.LogError("LoadedObject is null after loading!");
            }

            DisableMainMenu();
            EnablePlayerControls();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error parsing model huhu: {e.Message}\nStack trace: {e.StackTrace}");
        }
    }
}
}