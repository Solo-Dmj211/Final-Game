using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 100;
    public int currentHP;

    [Header("Lives")]
    public int maxLives = 3;
    public int currentLives;

    [Header("Respawn")]
    public Transform respawnPoint;
    public float invincibilityDuration = 1.5f;

    [Header("Game Over")]
    public string mainMenuSceneName = "MainMenu"; // scene to load on game over

    [Header("UI")]
    public Slider hpSlider;
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

        Respawn();
    }

    void Respawn()
    {
        currentHP = maxHP;

        Vector3 target = respawnPoint != null ? respawnPoint.position : startPosition;
        transform.position = target;

        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        Debug.Log("respawned with full hp");
        UpdateUI();
    }

    void GameOver()
    {
        Debug.Log("game over! returning to main menu...");
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void UpdateUI()
    {
        if (hpText != null)
            hpText.text = "HP: " + currentHP;
        if (hpSlider != null)
            hpSlider.value = currentHP;
        if (livesText != null)
            livesText.text = "Lives: " + currentLives;
    }

    // SARUN: for testing purposes by sarun - press K to take 25 damage
    void OnGUI()
    {
        if (Application.isEditor && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.K)
        {
            TakeDamage(25);
        }
    }
}