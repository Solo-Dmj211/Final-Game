using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class SceneTransition : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Type the exact name of the scene you want to teleport to.")]
    public string sceneToLoad;

    [Header("Input Settings")]
    [Tooltip("Select your PlayerInteract action from the dropdown or drag it here.")]
    public InputActionReference interactAction;

    [Header("Exit Portal Settings")]
    [Tooltip("If enabled, this portal will be locked until all enemies in the scene are defeated.")]
    public bool isExitPortal = false;
    [Tooltip("Drag your TMP text object here. It will show the remaining enemy count.")]
    public TextMeshPro portalText;
    public int mapIndex = 0;

    public bool isDisabled = false;

    private bool isPlayerInRange = false;
    private int enemyLayer;

    private void Start()
    {
        enemyLayer = LayerMask.GetMask("Enemy");

        if (!isExitPortal && GameManager.Instance != null)
        {
            isDisabled = !GameManager.Instance.IsMapUnlocked(mapIndex);

            if (portalText != null)
                portalText.text = isDisabled ? "Locked" : "Press E to enter";
        }
    }

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

    private int EnemyCount()
    {
        return FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Count(go => go.activeInHierarchy && ((1 << go.layer) & enemyLayer) != 0);
    }

    private void Update()
    {
        if (!isExitPortal) 
        {
            return;
        }

        int remaining = EnemyCount();

        if (remaining > 0)
        {
            portalText.text = $"You can't leave yet! There are {remaining} enemies remaining before you can exit.";
        }
        else
        {
            portalText.text = "Press E to leave.";
        }
    }

    private void InteractPressed(InputAction.CallbackContext context)
    {
        if (!isPlayerInRange || isDisabled) return;

        if (isExitPortal && EnemyCount() > 0)
        {
            Debug.Log("Cannot exit — enemies still remain!");
            return;
        }

        if (isExitPortal && GameManager.Instance != null)
            GameManager.Instance.CompleteMap(mapIndex);

        SceneManager.LoadScene(sceneToLoad);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}