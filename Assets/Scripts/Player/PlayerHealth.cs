using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

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

    Animator anim;
    Vector3 startPosition;
    bool isInvincible = false;
    float invincibilityTimer = 0f;
    bool isDead = false;

    void Awake()
    {
        anim = GetComponent<Animator>();
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
        if (isInvincible || isDead) return;

        currentHP = Mathf.Max(0, currentHP - amount);
        anim.SetTrigger("Hit");

        Debug.Log("player took " + amount + " damage. hp: " + currentHP);

        if (currentHP <= 0)
        {
            isDead = true;
            anim.SetTrigger("Died");
            StartCoroutine(WaitForDeathAnim());
        }
        else
        {
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
        }

        UpdateUI();
    }

    // dirty way we wait for anim to finish then respawn :P
    IEnumerator WaitForDeathAnim()
    {
        yield return null;
        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;

        yield return new WaitForSeconds(2f);

        LoseLife();
        anim.Play("Idle", 0, 0f);
    }

    void LoseLife()
    {
        currentLives--;
        Debug.Log("lost a life. lives left: " + currentLives);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayLoseLife();    // play lose life sound effect

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
        isDead = false;

        Debug.Log("respawned with full hp");
        UpdateUI();
    }

    void GameOver()
    {
        Debug.Log("game over! returning to main menu...");
        if (AudioManager.Instance != null) AudioManager.Instance.PlayGameOver(); // play game over sound effect
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