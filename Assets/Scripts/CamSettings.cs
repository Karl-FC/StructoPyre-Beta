using UnityEngine;

public class CameraSetup : MonoBehaviour
{
    void Start()
    {
        Camera cam = GetComponent<Camera>();
        
        // Basic settings
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 1000f;
        cam.fieldOfView = 60f;

        Debug.Log("Camera settings initialized");
    }
}