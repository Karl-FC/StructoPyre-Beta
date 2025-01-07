using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    private float yaw = 0.0f, pitch = 0.0f;
    private Rigidbody rb;
    private ControlThings inputActions;

    [SerializeField] float walkSpeed = 3.0f, CamSensitivity = 1.0f;
    [SerializeField] private DPadController dPadController;

    private void Awake()
    {
        inputActions = new ControlThings();
        if (dPadController == null)
        {
            Debug.LogWarning("ERROR: DPadController reference is missing.");
        }

        if (walkSpeed < 0 || CamSensitivity < 0) 
        {
            Debug.LogWarning("ERROR: WalkSpeed and Camera Sensitivity is invalid");
        }
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
        yaw += lookInput.x * CamSensitivity;
        pitch -= lookInput.y * CamSensitivity;
        pitch = Mathf.Clamp(pitch, -90.0f, 90.0f);
        transform.localRotation = Quaternion.Euler(pitch, yaw, 0);
    }

    void MoveitMoveit() { //Movement controls
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y) * walkSpeed * Time.deltaTime;

        Vector2 dPadInput = dPadController.GetDPadInput();
        move += new Vector3(dPadInput.x, 0, dPadInput.y) * walkSpeed * Time.deltaTime;

        rb.MovePosition(transform.position + transform.TransformDirection(move));
    }
}