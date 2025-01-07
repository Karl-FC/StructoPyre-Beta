using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    private float yaw = 0.0f, pitch = 0.0f;
    private Rigidbody rb;
    private InputControlThings inputActions;

    [SerializeField] float walkSpeed = 3.0f, CamSensitivity = 1.0f;

    private void Awake()
    {
        inputActions = new InputControlThings();
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    void Start () {
        Cursor.lockState = CursorLockMode.Locked;
        rb = gameObject.GetComponent<Rigidbody>();
    }

    void Update () {
        Pagtingin();
    }

    private void FixedUpdate()
    {
        MoveitMoveit();
    }

    void Pagtingin() { //Camera looking function
        Vector2 lookInput = inputActions.Player.Look.ReadValue<Vector2>();
        yaw += lookInput.x * CamSensitivity
;
        pitch -= lookInput.y * CamSensitivity
;
        pitch = Mathf.Clamp(pitch, -90.0f, 90.0f);
        transform.localRotation = Quaternion.Euler(pitch, yaw, 0);
    }

    void MoveitMoveit() { //Movement controls
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y) * walkSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + transform.TransformDirection(move));
    }
}