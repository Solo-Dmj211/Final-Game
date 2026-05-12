using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct BuildingTier
{
    public GameObject visualObject;
    public int costToReach;
}

public class UpgradeableBuilding : MonoBehaviour
{
    [Header("Tiers")]
    public BuildingTier[] tiers;

    [Header("Win")]
    public GameObject winScreenUI;

    int currentTier = 0;

    void Start()
    {
        if (GameManager.Instance != null)
            currentTier = GameManager.Instance.tower_level;

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

            if (GameManager.Instance != null)
                GameManager.Instance.tower_level = currentTier;

            UpdateVisuals();

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
        Time.timeScale = 0f;

        if (winScreenUI != null)
            winScreenUI.SetActive(true);

        yield return new WaitForSecondsRealtime(3f);

        Time.timeScale = 1f;
        GameManager.Instance.money = 0;
        GameManager.Instance.tower_level = 0; // reset for next run
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