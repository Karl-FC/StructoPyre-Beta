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
 private void SetVerticalInput(float value)
    {
        dPadInput.y = value;
        Debug.Log($"Vertical input: {value}");
    }

    private void SetHorizontalInput(float value)
    {
        dPadInput.x = value;
        Debug.Log($"Horizontal input: {value}");
    }

    // Input System methods
    public void OnDPadUpPressed(InputAction.CallbackContext context)
    {
        if (context.performed) SetVerticalInput(1);
        if (context.canceled) SetVerticalInput(0);
    }

    public void OnDPadDownPressed(InputAction.CallbackContext context)
    {
        if (context.performed) SetVerticalInput(-1);
        if (context.canceled) SetVerticalInput(0);
    }

    // UI Button methods
    public void OnUpButtonClick() => SetVerticalInput(1);
    public void OnUpButtonRelease() => SetVerticalInput(0);
    public void OnDownButtonClick() => SetVerticalInput(-1);
    public void OnDownButtonRelease() => SetVerticalInput(0);

    public Vector2 GetDPadInput()
    {
        return dPadInput;
    }
}