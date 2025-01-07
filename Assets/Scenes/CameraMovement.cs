using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraMovement : MonoBehaviour
{
    private float yaw = 0.0f, pitch = 0.0f;
    private Rigidbody rb;

    [SerializeField] float walkSpeed = 5.0f, sensitivity = 2.0f;

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
        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        pitch = Mathf.Clamp(pitch, -90.0f, 90.0f);
        transform.localRotation = Quaternion.Euler(pitch, yaw, 0);
    }

    void MoveitMoveit() { //Movement controls
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * walkSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + transform.TransformDirection(move));
    }
}

