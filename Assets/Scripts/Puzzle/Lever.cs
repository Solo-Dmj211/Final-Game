using UnityEngine;
using UnityEngine.InputSystem;

public class LeverPickup : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference interactAction;

    [Header("UI Feedback")]
    [Tooltip("Optional: a UI GameObject to show while the player is carrying the lever.")]
    public GameObject carryingIndicator;

    // Static state shared with LeverSlot.
    public static bool isCarrying = false;

    bool playerInRange = false;
    bool isPickedUp = false;

    void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractPressed;
        }

        // Always start NOT carrying when this scene loads.
        isCarrying = false;
        if (carryingIndicator != null) carryingIndicator.SetActive(false);
    }

    void OnDisable()
    {
        if (interactAction != null)
            interactAction.action.performed -= OnInteractPressed;
    }

    void OnInteractPressed(InputAction.CallbackContext ctx)
    {
        if (!playerInRange || isPickedUp) return;

        PickUp();
    }

    void PickUp()
    {
        isPickedUp = true;
        isCarrying = true;
        gameObject.SetActive(false); // hide the pickup sprite

        if (carryingIndicator != null) carryingIndicator.SetActive(true);
        Debug.Log("picked up the lever — carrying it now");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (!isPickedUp)
                Debug.Log("near lever. press E to pick it up.");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}