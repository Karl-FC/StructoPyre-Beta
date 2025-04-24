using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    private float yaw = 0.0f, pitch = 0.0f;
    private Rigidbody rb;
    private ControlThings inputActions;

    [SerializeField] public float walkSpeed = 3.0f, CamSensitivity = 1.0f;
    [SerializeField] private DPadController dPadController;

    private void Awake()
    {
        inputActions = new ControlThings();
        if (dPadController == null)
        {
            Debug.LogWarning("ERROR: DPadController reference is missing.");
        }

        if (walkSpeed <= 0 || CamSensitivity <= 0) 
        {
            Debug.LogWarning("ERROR: WalkSpeed and Camera Sensitivity is invalid");
        }

        
    }

    private void OnEnable()
    {
        if (inputActions != null)
        {
            inputActions.Player.Enable();
            Debug.Log("Player input actions enabled");
        }
    }

    private void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Player.Disable();
            Debug.Log("Player input actions disabled");
        }
    }

    void Start () {
        Cursor.lockState = CursorLockMode.Locked;
        rb = gameObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Dapat makakagalaw muna si player bago mag look around
        if (GlobalVariables.playerCanMove)
        {
            Pagtingin();
        }
    }

    private void FixedUpdate()
    {
        // Only process movement if player can move
        if (GlobalVariables.playerCanMove)
        {
            MoveitMoveit();
        }
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