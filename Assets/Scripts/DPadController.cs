using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DPadController : MonoBehaviour, ControlThings.IDPADActions
{
    [SerializeField] private GameObject dpadUI;
    private ControlThings inputActions;
    private Vector2 dPadInput;

    private void Awake()
    {
        inputActions = new ControlThings();
        inputActions.DPAD.SetCallbacks(this);
        
        if (dpadUI != null)
        {
            dpadUI.SetActive(Application.isMobilePlatform);
            if (Application.isMobilePlatform)
            {
                Debug.Log("User is on mobile");
            } else Debug.Log("User not on mobile lol");
        }
    }

    private void OnEnable()
    {
        inputActions.DPAD.Enable();
    }

    private void OnDisable()
    {
        inputActions.DPAD.Disable();
    }

    public void OnUPButton(InputAction.CallbackContext context)
    {
        if (context.performed) dPadInput.y = 1;
        if (context.canceled) dPadInput.y = 0;
    }

    public void OnDOWNButton(InputAction.CallbackContext context)
    {
        if (context.performed) dPadInput.y = -1;
        if (context.canceled) dPadInput.y = 0;
    }

    public void OnLEFTButton(InputAction.CallbackContext context)
    {
        if (context.performed) dPadInput.x = -1;
        if (context.canceled) dPadInput.x = 0;
    }

    public void OnRIGHTButton(InputAction.CallbackContext context)
    {
        if (context.performed) dPadInput.x = 1;
        if (context.canceled) dPadInput.x = 0;
    }

    public Vector2 GetDPadInput()
    {
        return dPadInput;
    }
}