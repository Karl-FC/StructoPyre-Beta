using UnityEngine;
using UnityEngine.InputSystem;

public class DPadController : MonoBehaviour

{
    [SerializeField] private GameObject dpadUI;

private void Awake()
    {
        if (dpadUI != null)
        {
            //dpadUI.SetActive(Application.isMobilePlatform);
            dpadUI.SetActive(true);
            if (Application.isMobilePlatform) Debug.Log("User using mobile"); 
            else
            {
                Debug.Log("User is NOT on mobile lol");
            }
        }
    }

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