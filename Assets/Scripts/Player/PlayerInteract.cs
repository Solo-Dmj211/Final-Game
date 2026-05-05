using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    [Header("Inventory")]
    public int coins = 10;

    [Header("References")]
    public TextMeshProUGUI debugMoney;
    
    UpgradeableBuilding buildingInRange;

    void Awake()
    {
        UpdateMoneyText();
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && buildingInRange != null)
        {
            buildingInRange.TryUpgrade(ref coins);
            UpdateMoneyText();
        }
    }

    // Sarun: called by Resource pickups
    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateMoneyText();
    }

    void UpdateMoneyText()
    {
        if (debugMoney != null)
            debugMoney.text = "Money: " + coins.ToString();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        UpgradeableBuilding building = other.GetComponent<UpgradeableBuilding>();
        if (building != null)
        {
            buildingInRange = building;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<UpgradeableBuilding>() == buildingInRange)
        {
            buildingInRange = null;
        }
    }
}
