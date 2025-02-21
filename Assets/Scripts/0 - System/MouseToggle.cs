using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleMouseCursor : MonoBehaviour
{
    private ControlThings inputActions;
    [SerializeField] private MonoBehaviour playerMovements;
    

    private void Awake()
    {
        inputActions = new ControlThings();
        ToggleCursorFunc(true);
    }

    private void OnEnable()
    {
        inputActions.CrossPlatform.ActivateCursor.performed += ToggleMouse;
        inputActions.CrossPlatform.Enable();
    }

    private void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.CrossPlatform.ActivateCursor.performed -= ToggleMouse;
            inputActions.CrossPlatform.Disable();
            inputActions = null;
        }
    }

private void ToggleMouse(InputAction.CallbackContext context)
    {
        ToggleCursorFunc(!Cursor.visible);
    }

    public void ToggleCursorFunc(bool mayMouseba)
    {
            Cursor.visible = mayMouseba;
            Cursor.lockState = mayMouseba ? CursorLockMode.None : CursorLockMode.Locked;
            Debug.Log($"Cursor is now {mayMouseba}");
            GlobalVariables.isMouseVisible = mayMouseba;
           //Movement true 
            playerMovements.enabled = !mayMouseba;
            GlobalVariables.playerCanMove = !mayMouseba;

    }
}