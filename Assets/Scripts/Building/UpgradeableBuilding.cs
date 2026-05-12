using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[System.Serializable]
public struct BuildingTier
{
    public GameObject visualObject;
    public int costToReach;
    public int scoreToGive;
}

public class UpgradeableBuilding : MonoBehaviour
{
    [Header("Tiers")]
    public BuildingTier[] tiers;

    [Header("Win")]
    public GameObject winScreenUI;
    public TextMeshProUGUI winText;

    int currentTier = 0;

    void Start()
    {
        if (GameManager.Instance != null)
            currentTier = GameManager.Instance.towerLevel;

        UpdateVisuals();
    }

    public void TryUpgrade(ref int playerCoins)
    {
        if (currentTier >= tiers.Length - 1)
        {
            Debug.Log("Already max level");
            return;
        }

        int cost = tiers[currentTier + 1].costToReach;

        if (playerCoins >= cost)
        {
            playerCoins -= cost;
            currentTier++;
            GameManager.Instance.AddScore(tiers[currentTier].scoreToGive); // add score when upgrading the tower

            if (GameManager.Instance != null)
                GameManager.Instance.towerLevel = currentTier;

            UpdateVisuals();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayLevelUp(); // play level up sound effect

            if (currentTier >= tiers.Length - 1)
                StartCoroutine(WinSequence());
        }
        else
        {
            Debug.Log("Not enough coins! You need " + cost);
        }
    }

    IEnumerator WinSequence()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayGameWon(); // play game won sound effect
        Time.timeScale = 0f;
        GameManager.Instance.ApplyTimeBonus();
        
        if (winText != null)
            winText.text = $"You win!\nFinal score: {GameManager.Instance.GetFinalScore()}\nYour score has been submitted.\nThe game will reset soon.";
        if (winScreenUI != null)
            winScreenUI.SetActive(true);

        yield return new WaitForSecondsRealtime(8f);

        Time.timeScale = 1f;
        
        GameManager.Instance.ResetGame();
        SceneManager.LoadScene("MainMenu");
    }

    void UpdateVisuals()
    {
        for (int i = 0; i < tiers.Length; i++)
        {
            if (tiers[i].visualObject != null)
                tiers[i].visualObject.SetActive(i == currentTier);
        }
    }
}