using UnityEngine;
using UnityEngine.InputSystem;

public class DPadController : MonoBehaviour
{
    private Vector2 dPadInput;

    public void OnDPadUpPressed(InputAction.CallbackContext context)
    {
        if (context.performed) dPadInput.y = 1;
        if (context.canceled) dPadInput.y = 0;
    }

    public void OnDPadDownPressed(InputAction.CallbackContext context)
    {
        if (context.performed) dPadInput.y = -1;
        if (context.canceled) dPadInput.y = 0;
    }

    public void OnDPadLeftPressed(InputAction.CallbackContext context)
    {
        if (context.performed) dPadInput.x = -1;
        if (context.canceled) dPadInput.x = 0;
    }

    public void OnDPadRightPressed(InputAction.CallbackContext context)
    {
        if (context.performed) dPadInput.x = 1;
        if (context.canceled) dPadInput.x = 0;
    }

    public Vector2 GetDPadInput()
    {
        return dPadInput;
    }
}