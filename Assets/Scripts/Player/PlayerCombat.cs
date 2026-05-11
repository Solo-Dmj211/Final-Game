using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("State")]
    public bool isMeleeEquipped = true; // true = melee, false = charge

    [Header("Hitbox")]
    public BoxCollider2D attackHitbox;
    public LayerMask enemyLayer;

    [Header("Melee")]
    public float meleeRate = 0.5f;
    public int meleeDamage = 20;

    [Header("Charge Attack")]
    public float chargeRate = 1.5f;       // cooldown after a charge releases
    public int chargeDamage = 50;
    public float minChargeTime = 0.3f;    // must hold at least this long
    public float maxChargeTime = 1.5f;    // full charge reached at this point

    [Header("Animation")]
    public Animator animator;

    [Header("References")]
    public PlayerController playerController;

    float nextMeleeTime;
    float nextChargeTime;

    bool isCharging = false;
    float chargeStartTime;

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (isMeleeEquipped)
        {
            if (ctx.performed && Time.time >= nextMeleeTime)
            {
                MeleeAttack();
                nextMeleeTime = Time.time + meleeRate;
            }
        }
        else
        {
            // charge begins on press, releases on button up
            if (ctx.performed && !isCharging && Time.time >= nextChargeTime)
            {
                BeginCharge();
            }
            else if (ctx.canceled && isCharging)
            {
                ReleaseCharge();
                nextChargeTime = Time.time + chargeRate;
            }
        }
    }

    public void OnSwapWeapon(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (isCharging)
            {
                isCharging = false;
                playerController.enabled = true; // unlock movement if swapping out mid-charge
                animator?.SetBool("IsCharging", false);
            }

            isMeleeEquipped = !isMeleeEquipped;
            Debug.Log(isMeleeEquipped ? "swapped to: melee" : "swapped to: charge");
        }
    }

    void MeleeAttack()
    {
        animator?.SetTrigger("Attack");
        HitEnemiesInBox(meleeDamage);
    }

    void BeginCharge()
    {
        isCharging = true;
        chargeStartTime = Time.time;
        playerController.enabled = false;
        animator?.SetBool("IsCharging", true);
    }

    void ReleaseCharge()
    {
        isCharging = false;
        playerController.enabled = true;
        animator?.SetBool("IsCharging", false);

        float heldTime = Time.time - chargeStartTime;

        if (heldTime < minChargeTime)
        {
            Debug.Log("charge released too early — no attack");
            return;
        }

        float chargeRatio = Mathf.Clamp01((heldTime - minChargeTime) / (maxChargeTime - minChargeTime));
        int damage = Mathf.RoundToInt(Mathf.Lerp(chargeDamage * 0.5f, chargeDamage, chargeRatio));

        animator?.SetTrigger("ChargeReleased");
        HitEnemiesInBox(damage);
        Debug.Log($"charge released after {heldTime:F2}s for {damage} damage");
    }

    void HitEnemiesInBox(int damage)
    {
        if (attackHitbox == null) return;

        Vector2 center = attackHitbox.bounds.center;
        Vector2 size   = attackHitbox.bounds.size;

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, enemyLayer);

        foreach (Collider2D enemy in hits)
        {
            EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
            if (eh != null)
                eh.TakeDamage(damage);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackHitbox == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackHitbox.bounds.center, attackHitbox.bounds.size);
    }
}