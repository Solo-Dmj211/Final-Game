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
    [Tooltip("If enabled, this portal will be locked until enough enemies are defeated.")]
    public bool isExitPortal = false;
    [Tooltip("Drag your TMP text object here. It will show the remaining enemy count.")]
    public TextMeshPro portalText;
    public int mapIndex = 0;

    [Header("Kill Threshold")]
    [Tooltip("Percentage of enemies that must be killed before the exit unlocks. 1 = all, 0.5 = half, etc.")]
    [Range(0f, 1f)]
    public float killThreshold = 1f;

    public bool isDisabled = false;

    private bool isPlayerInRange = false;
    private int enemyLayer;
    private int initialEnemyCount;
    private int requiredKills; // calculated once on Start, rounded to whole number

    private void Start()
    {
        enemyLayer = LayerMask.GetMask("Enemy");

        if (isExitPortal)
        {
            initialEnemyCount = EnemyCount();
            requiredKills     = Mathf.RoundToInt(initialEnemyCount * killThreshold);

            Debug.Log($"SceneTransition: {requiredKills}/{initialEnemyCount} enemies must be killed to unlock exit.");
        }

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

    // How many enemies have been killed so far.
    private int KillsSoFar() => initialEnemyCount - EnemyCount();

    // True when the player has hit the required kill count.
    private bool ThresholdMet() => KillsSoFar() >= requiredKills;

    private void Update()
    {
        if (!isExitPortal) return;

        int killsSoFar  = KillsSoFar();
        int killsNeeded = requiredKills - killsSoFar;

        if (!ThresholdMet())
        {
            portalText.text = $"You can't leave yet! Kill {killsNeeded} more enemies to exit. ({killsSoFar}/{requiredKills})";
        }
        else
        {
            portalText.text = "Press E to leave.";
        }
    }

    private void InteractPressed(InputAction.CallbackContext context)
    {
        if (!isPlayerInRange || isDisabled) return;

        if (isExitPortal && !ThresholdMet())
        {
            Debug.Log($"Cannot exit — {requiredKills - KillsSoFar()} more enemies must be killed.");
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
            isPlayerInRange = false;
    }
}