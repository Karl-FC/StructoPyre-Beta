using UnityEngine;
using UnityEngine.InputSystem;

public class InspectorToggle : MonoBehaviour
{
    [SerializeField] private FaceInspector faceInspector;
    [SerializeField] private MaterialPropertyEditor propertyEditor;
    private ControlThings inputActions;

    private void Awake()
    {
        inputActions = new ControlThings();
    }

    private void OnEnable()
    {
        inputActions.CrossPlatform.OpenPropertyEditor.performed += OnToggleAction;
        inputActions.CrossPlatform.Enable();
    }

    private void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.CrossPlatform.OpenPropertyEditor.performed -= OnToggleAction;
            inputActions.CrossPlatform.Disable();
            inputActions = null;
        }
    }

    private void OnToggleAction(InputAction.CallbackContext context)
    {
        if (propertyEditor != null && faceInspector != null && faceInspector.IsInspectorCurrentlyActive)
        {
            propertyEditor.TogglePanel();
        }
    }
} 