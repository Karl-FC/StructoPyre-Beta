using UnityEngine;
using UnityEngine.InputSystem;

public class InspectorToggle : MonoBehaviour
{
    [SerializeField] private FaceInspector faceInspector;
    private ControlThings inputActions;
    private bool inspectorActive = false;

    private void Awake()
    {
        inputActions = new ControlThings();
    }

    private void OnEnable()
    {
        inputActions.CrossPlatform.OpenPropertyEditor.performed += OnToggleInspector;
        inputActions.CrossPlatform.Enable();
    }

    private void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.CrossPlatform.OpenPropertyEditor.performed -= OnToggleInspector;
            inputActions.CrossPlatform.Disable();
            inputActions = null;
        }
    }

    private void OnToggleInspector(InputAction.CallbackContext context)
    {
        inspectorActive = !inspectorActive;
        if (faceInspector != null)
        {
            faceInspector.SetInspectorActive(inspectorActive);
        }
    }
} 