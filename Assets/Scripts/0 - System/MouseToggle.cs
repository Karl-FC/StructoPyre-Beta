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

    public void ToggleCursorFunc(bool showCursor)
    {
        Cursor.visible = showCursor;
        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
        
        // Update muna yun global variable
        GlobalVariables.isMouseVisible = showCursor;
        GlobalVariables.playerCanMove = !showCursor;
        
        // THEN apply movement state
        if (playerMovements != null)
        {
            playerMovements.enabled = !showCursor;
            Debug.Log($"Player movement is now {(!showCursor ? "enabled" : "disabled")}");
        }
        else
        {
            Debug.LogWarning("playerMovements reference is null in ToggleCursorFunc");
        }
        
        Debug.Log($"Cursor is now {(showCursor ? "visible" : "hidden")}, PlayerCanMove={GlobalVariables.playerCanMove}");
    }
}