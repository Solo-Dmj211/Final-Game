using UnityEngine;
using UnityEngine.InputSystem;

public class LeverSlot : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference interactAction;

    [Header("Visuals")]
    [Tooltip("The lever sprite that appears once installed.")]
    public GameObject installedLeverVisual;

    [Header("UI Feedback")]
    [Tooltip("Optional: same carrying indicator from LeverPickup. Will be hidden when installed.")]
    public GameObject carryingIndicator;

    bool playerInRange = false;
    bool isInstalled = false;

    void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractPressed;
        }

        if (installedLeverVisual != null) installedLeverVisual.SetActive(false);
    }

    void OnDisable()
    {
        if (interactAction != null)
            interactAction.action.performed -= OnInteractPressed;
    }

    void OnInteractPressed(InputAction.CallbackContext ctx)
    {
        if (!playerInRange || isInstalled) return;
        if (!LeverPickup.isCarrying) return; // must be carrying to install

        Install();
    }

    void Install()
    {
        isInstalled = true;
        LeverPickup.isCarrying = false;
        SceneTransition.puzzleSolved = true;

        if (installedLeverVisual != null) installedLeverVisual.SetActive(true);
        if (carryingIndicator != null) carryingIndicator.SetActive(false);

        Debug.Log("lever installed — puzzle solved");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (!isInstalled)
            {
                if (LeverPickup.isCarrying)
                    Debug.Log("at slot. press E to install the lever.");
                else
                    Debug.Log("at slot. you need to find the lever first.");
            }
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
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}