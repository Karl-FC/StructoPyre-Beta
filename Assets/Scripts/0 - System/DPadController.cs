using UnityEngine;
using UnityEngine.InputSystem;
using System.Runtime.InteropServices; // Required for DllImport

public class DPadController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject dpadUIRoot; // Assign the PARENT GameObject of the DPad UI
    [SerializeField] private CameraMovement cameraMovement; // Assign the CameraMovement script

    [Header("Settings")]
    [Tooltip("Key used to save the DPad toggle state across sessions")]
    [SerializeField] private string dPadPlayerPrefsKey = "ForceDPad";

    private Vector2 dPadInput;
    private ControlThings inputActions;
    private bool isDPadActive = false; // Track the current state

    // Import the JavaScript function from MobileDetect.jslib
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern bool IsMobileBrowser();
#endif

    private void Awake()
    {
        if (dpadUIRoot == null)
            Debug.LogError("DPad UI Root reference is missing in DPadController!");
        if (cameraMovement == null)
             Debug.LogError("Camera Movement reference is missing in DPadController!");

        inputActions = new ControlThings();
        // inputActions.UI.Click.Enable(); // Consider if UI.Click is still needed here
        // Enable the specific DPad actions if needed (might be auto-enabled by PlayerInput component?)
        inputActions.Player.DPadMove.Enable();

        // Determine initial DPad state
        bool activateInitially = false;
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            activateInitially = IsMobileBrowser();
            Debug.Log($"Mobile Browser Detected: {activateInitially}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error calling IsMobileBrowser(). Ensure MobileDetect.jslib is in Plugins/WebGL. " + e.Message);
            activateInitially = false; // Default to false if JS call fails
        }
#else
        // Not in WebGL build, default to false unless overridden by setting
        activateInitially = false;
        Debug.Log("Not a WebGL build, DPad defaults to off.");
#endif

        // Check PlayerPrefs for user override setting
        if (PlayerPrefs.HasKey(dPadPlayerPrefsKey))
        {
            bool forcedState = PlayerPrefs.GetInt(dPadPlayerPrefsKey) == 1;
            activateInitially = forcedState; // Setting overrides detection
            Debug.Log($"DPad state loaded from PlayerPrefs: {activateInitially}");
        }

        // Apply the initial state
        SetDPadActive(activateInitially);
    }

    private void OnEnable()
    {
        inputActions?.Enable();
    }

    private void OnDisable()
    {
        inputActions?.Disable();
    }

    private void Update()
    {
        // If DPad is active, send its input to the camera movement
        if (isDPadActive && cameraMovement != null)
        {
            // Read DPad input if using Action Map polling (less common)
            // dPadInput = inputActions.Player.DPadMove.ReadValue<Vector2>();

            // Send the input (dPadInput is updated by callbacks)
            cameraMovement.SetDPadMovementInput(dPadInput);
        }
        else if (cameraMovement != null)
        {
             // Ensure input is zeroed when DPad is inactive
             cameraMovement.SetDPadMovementInput(Vector2.zero);
        }
    }

    // --- Public Methods --- 

    // Call this from your Settings UI Button/Toggle
    public void ToggleDPadViaSetting(bool activate)
    {
        PlayerPrefs.SetInt(dPadPlayerPrefsKey, activate ? 1 : 0); // Save the setting
        PlayerPrefs.Save(); // Ensure it's written
        SetDPadActive(activate);
    }

    // --- Internal Logic --- 

    private void SetDPadActive(bool activate)
    {
        if (dpadUIRoot != null)
        {
            dpadUIRoot.SetActive(activate);
        }
        isDPadActive = activate;

        // Reset input when deactivated
        if (!activate)
        {
            dPadInput = Vector2.zero;
        }

        Debug.Log($"DPad Active set to: {isDPadActive}");
    }

    // --- Input Callbacks --- (Ensure these match your Input Actions asset) ---

    // Combined callback for DPadMove Action (Vector2)
    public void OnDPadMove(InputAction.CallbackContext context)
    {
        if (isDPadActive) // Only process input if DPad is active
        {
            dPadInput = context.ReadValue<Vector2>();
             Debug.Log($"DPad Move Input: {dPadInput}");
        }
        else
        {
            dPadInput = Vector2.zero; // Ensure zero if called while inactive
        }
    }

    // Need methods to be called by UI Buttons (EventTrigger PointerDown/Up)
    // These now directly set the dPadInput variable used in Update
    public void OnPointerDown_Up() => dPadInput.y = 1;    // Forward
    public void OnPointerUp_Up() => dPadInput.y = 0;
    public void OnPointerDown_Down() => dPadInput.y = -1;  // Back
    public void OnPointerUp_Down() => dPadInput.y = 0;
    public void OnPointerDown_Left() => dPadInput.x = -1;   // Left
    public void OnPointerUp_Left() => dPadInput.x = 0;
    public void OnPointerDown_Right() => dPadInput.x = 1;  // Right
    public void OnPointerUp_Right() => dPadInput.x = 0;

    // No longer need GetDPadInput(), Update sends it directly
    // public Vector2 GetDPadInput()
    // {
    //     return dPadInput;
    // }
}