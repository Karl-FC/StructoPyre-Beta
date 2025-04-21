using UnityEngine;
using UnityEngine.InputSystem;

public class DPadController : MonoBehaviour

{
    [SerializeField] private GameObject dpadUI;
    private Vector2 dPadInput;
    private ControlThings inputActions;



private void Awake()
    {
        if (dpadUI == null)
            Debug.LogWarning("DPad UI reference is missing!");
            
        inputActions = new ControlThings();
        inputActions.UI.Click.Enable();
    }
private void OnEnable()
    {
        inputActions?.Enable();
    }

    private void OnDisable()
    {
        inputActions?.Disable();
    }


//INPUTSSSSSSSSSSSSSSSS
 private void SetForwardInput(float value) // Changed from SetVerticalInput
    {
        dPadInput.y = value; // This will be used as Z in CameraMovement
        Debug.Log($"Forward/Back input: {value}");
    }

    private void SetHorizontalInput(float value)
    {
        dPadInput.x = value;
        Debug.Log($"Left/Right input: {value}");
    }

    // Input System methods
    public void OnDPadUpPressed(InputAction.CallbackContext context)
    {
        if (context.performed) SetForwardInput(1); // Forward
        if (context.canceled) SetForwardInput(0);
    }

    public void OnDPadDownPressed(InputAction.CallbackContext context)
    {
        if (context.performed) SetForwardInput(-1); // Back
        if (context.canceled) SetForwardInput(0);
    }

    public void OnDPadLeftPressed(InputAction.CallbackContext context)
    {
        if (context.performed) SetHorizontalInput(-1); // Left
        if (context.canceled) SetHorizontalInput(0);
    }

    public void OnDPadRightPressed(InputAction.CallbackContext context)
    {
        if (context.performed) SetHorizontalInput(1); // Right
        if (context.canceled) SetHorizontalInput(0);
    }

    // UI Button methods for mouse/touch
    public void OnUpButtonClick() => SetForwardInput(1);    // Forward
    public void OnUpButtonRelease() => SetForwardInput(0);
    public void OnDownButtonClick() => SetForwardInput(-1); // Back
    public void OnDownButtonRelease() => SetForwardInput(0);
    public void OnLeftButtonClick() => SetHorizontalInput(-1);  // Left
    public void OnLeftButtonRelease() => SetHorizontalInput(0);
    public void OnRightButtonClick() => SetHorizontalInput(1);  // Right
    public void OnRightButtonRelease() => SetHorizontalInput(0);

    public Vector2 GetDPadInput()
    {
        return dPadInput; // x = left/right, y = forward/back
    }
}