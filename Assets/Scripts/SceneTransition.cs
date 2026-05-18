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
    public InputActionReference interactAction;

    [Header("Exit Portal Settings")]
    public bool isExitPortal = false;
    public TextMeshPro portalText;
    public int mapIndex = 0;

    // -------------------------------------------------------------------------
    // Kill threshold
    // -------------------------------------------------------------------------
    [Header("Kill Threshold")]
    public bool killThresholdEnabled = true;
    [Range(0f, 1f)]
    [Tooltip("Percentage of enemies that must be killed. 1 = all, 0.5 = half.")]
    public float killThreshold = 1f;

    // -------------------------------------------------------------------------
    // Resource threshold
    // -------------------------------------------------------------------------
    [Header("Resource Threshold")]
    public bool resourceThresholdEnabled = false;

    public enum ResourceMode
    {
        FixedAmount,   // player must collect at least X resources total
        Percentage     // player must collect X% of a designer-specified total
    }
    public ResourceMode resourceMode = ResourceMode.FixedAmount;

    [Tooltip("FixedAmount mode: exact number of resources the player must collect.")]
    public int requiredResourceCount = 5;

    [Tooltip("Percentage mode: the total number of resources you expect to exist " +
             "(including ones dropped by enemies). Set this in the Inspector.")]
    public int expectedTotalResources = 10;

    [Range(0f, 1f)]
    [Tooltip("Percentage mode: fraction of expectedTotalResources that must be collected.")]
    public float resourcePercentage = 1f;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------
    public bool isDisabled = false;

    bool isPlayerInRange = false;

    int enemyLayer;
    int initialEnemyCount;
    int requiredKills;
    int requiredResources; // resolved in Start based on mode

    // Tracks resources collected this session.
    // Call SceneTransition.NotifyResourceCollected() from your pickup script.
    static int resourcesCollectedThisScene = 0;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    void Start()
    {
        enemyLayer = LayerMask.GetMask("Enemy");
        resourcesCollectedThisScene = 0;

        if (isExitPortal)
        {
            // Kill threshold setup
            if (killThresholdEnabled)
            {
                initialEnemyCount = EnemyCount();
                requiredKills     = Mathf.RoundToInt(initialEnemyCount * killThreshold);
                Debug.Log($"SceneTransition: {requiredKills}/{initialEnemyCount} enemies required.");
            }

            // Resource threshold setup
            if (resourceThresholdEnabled)
            {
                if (resourceMode == ResourceMode.FixedAmount)
                {
                    requiredResources = requiredResourceCount;
                }
                else
                {
                    requiredResources = Mathf.RoundToInt(expectedTotalResources * resourcePercentage);
                }

                Debug.Log($"SceneTransition: {requiredResources} resources must be collected.");
            }
        }

        if (!isExitPortal && GameManager.Instance != null)
        {
            isDisabled = !GameManager.Instance.IsMapUnlocked(mapIndex);
            if (portalText != null)
                portalText.text = isDisabled ? "Locked" : "Press E to enter";
        }
    }

    void OnEnable()
    {
        interactAction.action.Enable();
        interactAction.action.performed += InteractPressed;
    }

    void OnDisable()
    {
        interactAction.action.Disable();
        interactAction.action.performed -= InteractPressed;
    }

    // =========================================================================
    // Public API — call this from your resource pickup script when collected
    // =========================================================================

    public static void NotifyResourceCollected()
    {
        resourcesCollectedThisScene++;
    }

    // =========================================================================
    // Counting helpers
    // =========================================================================

    int EnemyCount()
    {
        return FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Count(go => go.activeInHierarchy && ((1 << go.layer) & enemyLayer) != 0);
    }

    int KillsSoFar() => initialEnemyCount - EnemyCount();

    bool KillThresholdMet()
    {
        if (!killThresholdEnabled) return true;
        return KillsSoFar() >= requiredKills;
    }

    bool ResourceThresholdMet()
    {
        if (!resourceThresholdEnabled) return true;
        return resourcesCollectedThisScene >= requiredResources;
    }

    bool AllThresholdsMet() => KillThresholdMet() && ResourceThresholdMet();

    // =========================================================================
    // Update — portal text, enemies first then resources
    // =========================================================================

    void Update()
    {
        if (!isExitPortal) return;

        string text = "";

        // Enemy line — always shown first
        if (killThresholdEnabled && !KillThresholdMet())
        {
            int kills  = KillsSoFar();
            int needed = requiredKills - kills;
            text += $"Kill {needed} more {(needed == 1 ? "enemy" : "enemies")} to exit. ({kills}/{requiredKills})\n";
        }

        // Resource line — only shown after kill requirement is met so the
        // player focuses on enemies first, then resources
        if (resourceThresholdEnabled && KillThresholdMet() && !ResourceThresholdMet())
        {
            int collected = resourcesCollectedThisScene;
            int needed    = requiredResources - collected;
            text += $"Collect {needed} more {(needed == 1 ? "resource" : "resources")} to exit. ({collected}/{requiredResources})\n";
        }

        portalText.text = text.Length > 0 ? text.TrimEnd('\n') : "Press E to leave.";
    }

    // =========================================================================
    // Interaction
    // =========================================================================

    void InteractPressed(InputAction.CallbackContext context)
    {
        if (!isPlayerInRange || isDisabled) return;

        if (isExitPortal && !AllThresholdsMet())
        {
            Debug.Log("Cannot exit — conditions not met.");
            return;
        }

        if (isExitPortal && GameManager.Instance != null)
            GameManager.Instance.CompleteMap(mapIndex);

        SceneManager.LoadScene(sceneToLoad);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = false;
    }
}