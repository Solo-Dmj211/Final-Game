using UnityEngine;

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

    int currentTier = 0;

    void Start()
    {
        UpdateVisuals();
    }

    public void TryUpgrade(ref int playerCoins)
    {
        if (currentTier >= tiers.Length - 1)
        {
            Debug.Log("already max level");
            return;
        }

        int cost = tiers[currentTier + 1].costToReach;

        if (playerCoins >= cost)
        {
            playerCoins -= cost;
            currentTier++;
            UpdateVisuals();
            Debug.Log("upgraded! coins left: " + playerCoins);
        }
        else
        {
            Debug.Log("not enough coins! you need " + cost);
        }
    }

    void UpdateVisuals()
    {
        for (int i = 0; i < tiers.Length; i++)
        {
            if (tiers[i].visualObject != null)
            {
                tiers[i].visualObject.SetActive(i == currentTier);
            }
        }
    }
}