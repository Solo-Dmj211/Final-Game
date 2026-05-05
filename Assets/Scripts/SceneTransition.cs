using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SceneTransition : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Type the exact name of the scene you want to teleport to.")]
    public string sceneToLoad;

    [Header("Input Settings")]
    [Tooltip("Select your PlayerInteract action from the dropdown or drag it here.")]
    public InputActionReference interactAction;

    private bool isPlayerInRange = false;

    private void OnEnable()
    {
        interactAction.action.Enable();
        interactAction.action.performed += InteractPressed;
    }

    private void OnDisable()
    {
        interactAction.action.Disable();
        interactAction.action.performed -= InteractPressed;
    }

    private void InteractPressed(InputAction.CallbackContext context)
    {
        // Only teleport if they pressed E AND they are inside the trigger zone
        if (isPlayerInRange)
        {
            Debug.Log("Player pressed E in range! Loading map: " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("Player in range. Press E to travel.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log("Player left the transition zone.");
        }
    }
}