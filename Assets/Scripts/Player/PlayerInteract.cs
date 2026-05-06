using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
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
            buildingInRange.TryUpgrade(ref GameManager.Instance.money);
            UpdateMoneyText();
        }
    }

    // Sarun: called by Resource pickups
    public void AddCoins(int amount)
    {
        GameManager.Instance.money += amount;
        UpdateMoneyText();
    }

    void UpdateMoneyText()
    {
        if (debugMoney != null)
            debugMoney.text = "Money: " + GameManager.Instance.money.ToString();
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
