using UnityEngine;
using UnityEngine.InputSystem;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject prefabToSpawn;
    private ControlThings inputActions;

    private void Awake()
    {
        inputActions = new ControlThings();
    }

    private void OnEnable()
    {
        inputActions.CrossPlatform.Spawn.performed += OnSpawn;
        inputActions.CrossPlatform.Enable();
    }

    private void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.CrossPlatform.Spawn.performed -= OnSpawn;
            inputActions.CrossPlatform.Disable();
            inputActions = null;
        }
    }

    private void OnSpawn(InputAction.CallbackContext context)
    {
        if (prefabToSpawn != null)
        {
            Vector3 spawnPosition = transform.position;
            GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            Debug.Log($"Hello {spawnedObject.name}! from {spawnPosition}!");
        }
    }
}