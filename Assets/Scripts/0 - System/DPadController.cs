using UnityEngine;
using UnityEngine.InputSystem;
using System.Runtime.InteropServices; // Needed for DllImport

public class DPadController : MonoBehaviour
{
    [Header("UI - Toggleable DPad UI")]
    [SerializeField] private GameObject dpadUIRoot; // Assign the root GameObject of your DPad UI

    [Header("Activation Settings - Toggleable DPad Settings")]
    [SerializeField] private bool forceEnableDPad = false; // Manual override via settings

    private Vector2 dPadInput;
    private ControlThings inputActions;
    private bool isDPadActive = false; // Final state determining if DPad is active

    // --- Mobile Browser Detection --- //
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern bool IsMobileBrowser(); // Maps to the function in PlatformCheck.jslib
#endif

    private void Awake()
    {
        if (dpadUIRoot == null)
            Debug.LogError("DPad UI Root reference is missing in DPadController!");

        inputActions = new ControlThings();
        // Enable actions used by DPad (assuming UI map) ?
        inputActions.UI.Click.Enable(); 

        // Determine initial DPad state
        UpdateDPadActivationState();
    }

    private void UpdateDPadActivationState()
    {
        bool isMobile = false;
#if UNITY_WEBGL && !UNITY_EDITOR
        try {
            isMobile = IsMobileBrowser();
            Debug.Log($"Mobile browser detected via JS: {isMobile}");
        } catch (System.EntryPointNotFoundException) {
            Debug.LogError("IsMobileBrowser function not found. Make sure PlatformCheck.jslib is in Assets/Plugins/WebGL.");
        }
#else
        // In Editor or non-WebGL builds, assume not mobile unless forced
        isMobile = false;
        Debug.Log("Not a WebGL build or in Editor, mobile detection skipped.");
#endif

        isDPadActive = isMobile || forceEnableDPad;

        // Activate/Deactivate the DPad UI GameObject
        if (dpadUIRoot != null)
        {
            dpadUIRoot.SetActive(isDPadActive);
        }

        // Enable/disable input actions if needed (optional, depends if actions are exclusive to DPad)
        // Example: if (isDPadActive) inputActions.UI.Enable(); else inputActions.UI.Disable();

        Debug.Log($"DPad Active State Updated: {isDPadActive} (IsMobile: {isMobile}, ForceEnabled: {forceEnableDPad})");
    }


    private void OnEnable()
    {
        // Actions are enabled in Awake based on initial state, could re-evaluate here if needed
        inputActions?.Enable(); // Keep input system active generally
    }

    private void OnDisable()
    {
        inputActions?.Disable();
    }

    // --- Input Handling --- (Only process if active? Or just hide UI?)
    // Current logic assumes CameraMovement checks GetDPadInput, so we don't need to block here,
    // just ensure the UI is hidden/shown correctly via UpdateDPadActivationState.

    // INPUTSSSSSSSSSSSSSSSS
    private void SetForwardInput(float value) // Changed from SetVerticalInput
    {
        // Optional: Could add check 'if (!isDPadActive) return;' if input should be fully disabled
        dPadInput.y = value; // This will be used as Z in CameraMovement
        // Debug.Log($"Forward/Back input: {value}"); // Reduce log spam
    }

    private void SetHorizontalInput(float value)
    {
        // Optional: Could add check 'if (!isDPadActive) return;'
        dPadInput.x = value;
        // Debug.Log($"Left/Right input: {value}"); // Reduce log spam
    }

    // Input System methods (no changes needed here unless blocking input when inactive)
    public void OnDPadUpPressed(InputAction.CallbackContext context)
    {
        if (!isDPadActive) return; // Add guard if needed
        if (context.performed) SetForwardInput(1);
        if (context.canceled) SetForwardInput(0);
    }

    public void OnDPadDownPressed(InputAction.CallbackContext context)
    {
         if (!isDPadActive) return; // Add guard if needed
        if (context.performed) SetForwardInput(-1);
        if (context.canceled) SetForwardInput(0);
    }

    public void OnDPadLeftPressed(InputAction.CallbackContext context)
    {
         if (!isDPadActive) return; // Add guard if needed
        if (context.performed) SetHorizontalInput(-1);
        if (context.canceled) SetHorizontalInput(0);
    }

    public void OnDPadRightPressed(InputAction.CallbackContext context)
    {
         if (!isDPadActive) return; // Add guard if needed
        if (context.performed) SetHorizontalInput(1);
        if (context.canceled) SetHorizontalInput(0);
    }

    // UI Button methods (no changes needed here, rely on GameObject being inactive)
    public void OnUpButtonClick() => SetForwardInput(1);
    public void OnUpButtonRelease() => SetForwardInput(0);
    public void OnDownButtonClick() => SetForwardInput(-1);
    public void OnDownButtonRelease() => SetForwardInput(0);
    public void OnLeftButtonClick() => SetHorizontalInput(-1);
    public void OnLeftButtonRelease() => SetHorizontalInput(0);
    public void OnRightButtonClick() => SetHorizontalInput(1);
    public void OnRightButtonRelease() => SetHorizontalInput(0);

    public Vector2 GetDPadInput()
    {
        // Return input regardless of active state; let consumer decide?
        // Or: return isDPadActive ? dPadInput : Vector2.zero;
        return dPadInput;
    }

    // --- Public Toggle Method for Settings --- //
    public void SetDPadForcedState(bool shouldBeForcedOn)
    {
        if (forceEnableDPad != shouldBeForcedOn)
        {
            forceEnableDPad = shouldBeForcedOn;
            UpdateDPadActivationState(); // Re-evaluate and update UI/state
        }
    }
}