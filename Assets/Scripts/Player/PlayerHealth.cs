using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 100;
    public int currentHP;

    [Header("Lives")]
    public int maxLives = 3;
    public int currentLives;

    [Header("Respawn")]
    public Transform respawnPoint; // optional - if null, respawns at starting position
    public float invincibilityDuration = 1.5f; // seconds of invincibility after taking a hit

    [Header("UI")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI livesText;

    Vector3 startPosition;
    bool isInvincible = false;
    float invincibilityTimer = 0f;

    void Awake()
    {
        currentHP = maxHP;
        currentLives = maxLives;
        startPosition = transform.position;
        UpdateUI();
    }

    void Update()
    {
        // tick down invincibility frames
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (isInvincible) return;

        currentHP -= amount;
        Debug.Log("player took " + amount + " damage. hp: " + currentHP);

        if (currentHP <= 0)
        {
            LoseLife();
        }
        else
        {
            // brief invincibility so you don't get vaporized by overlapping enemies
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
        }

        UpdateUI();
    }

    void LoseLife()
    {
        currentLives--;
        Debug.Log("lost a life. lives left: " + currentLives);

        if (currentLives <= 0)
        {
            GameOver();
            return;
        }

        // respawn with full HP
        Respawn();
    }

    void Respawn()
    {
        currentHP = maxHP;

        Vector3 target = respawnPoint != null ? respawnPoint.position : startPosition;
        transform.position = target;

        // give brief invincibility so enemies near respawn don't insta-kill
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        Debug.Log("respawned with full hp");
        UpdateUI();
    }

    void GameOver()
    {
        Debug.Log("game over! restarting scene...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void UpdateUI()
    {
        if (hpText != null)
            hpText.text = "HP: " + currentHP;
        if (livesText != null)
            livesText.text = "Lives: " + currentLives;
    }

    // for testing in editor - press K to take 25 damage
    void OnGUI()
    {
        if (Application.isEditor && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.K)
        {
            TakeDamage(25);
        }
    }
}
