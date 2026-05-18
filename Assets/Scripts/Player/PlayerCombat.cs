using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("State")]
    public bool isMeleeEquipped = true;
    public bool isDisabled = false;

    [Header("Hitbox")]
    public BoxCollider2D attackHitbox;
    public LayerMask enemyLayer;

    [Header("Melee")]
    public float meleeRate = 0.5f;
    public int meleeDamage = 20;
    public float meleeKnockback = 5f;

    [Header("Charge Attack")]
    public float chargeRate = 1.5f;
    public int chargeDamage = 50;
    public float minChargeTime = 0.3f;
    public float maxChargeTime = 1.5f;
    public float chargeKnockback = 12f;

    [Header("Animation")]
    public Animator animator;

    [Header("References")]
    public PlayerController playerController;

    float nextMeleeTime;
    float nextChargeTime;
    bool isCharging = false;
    float chargeStartTime;

    public void SetCombatEnabled(bool enabled)
    {
        isDisabled = !enabled;

        if (isDisabled && isCharging)
        {
            isCharging = false;
            playerController.enabled = true;
            animator?.SetBool("IsCharging", false);
        }
    }

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (isDisabled) return;
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
            if (ctx.performed && !isCharging && Time.time >= nextChargeTime)
                BeginCharge();
            else if (ctx.canceled && isCharging)
            {
                ReleaseCharge();
                nextChargeTime = Time.time + chargeRate;
            }
        }
    }

    public void OnSwapWeapon(InputAction.CallbackContext ctx)
    {
        if (isDisabled) return;
        if (ctx.performed)
        {
            if (isCharging)
            {
                isCharging = false;
                playerController.enabled = true;
                animator?.SetBool("IsCharging", false);
            }

            isMeleeEquipped = !isMeleeEquipped;
            Debug.Log(isMeleeEquipped ? "swapped to: melee" : "swapped to: charge");
        }
    }

    void MeleeAttack()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMelee();   // Play sound effect
        animator?.SetTrigger("Attack");
        HitEnemiesInBox(meleeDamage, meleeKnockback);
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
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMelee();   // Play sound effect

        float chargeRatio = Mathf.Clamp01((heldTime - minChargeTime) / (maxChargeTime - minChargeTime));
        int damage = Mathf.RoundToInt(Mathf.Lerp(chargeDamage * 0.5f, chargeDamage, chargeRatio));

        // Knockback also scales with charge ratio
        float knockback = Mathf.Lerp(chargeKnockback * 0.5f, chargeKnockback, chargeRatio);

        animator?.SetTrigger("ChargeReleased");
        HitEnemiesInBox(damage, knockback);
        Debug.Log($"charge released after {heldTime:F2}s for {damage} damage");
    }

    void HitEnemiesInBox(int damage, float knockbackForce)
    {
        if (attackHitbox == null) return;

        Vector2 center = attackHitbox.bounds.center;
        Vector2 size   = attackHitbox.bounds.size;

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, enemyLayer);

        foreach (Collider2D enemy in hits)
        {
            // Damage and knockback
            Enemy en = enemy.GetComponent<Enemy>();
            if (en != null)
                en.TakeDamage(damage);
                Vector2 direction = ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;
                en.ApplyKnockback(direction * knockbackForce);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackHitbox == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackHitbox.bounds.center, attackHitbox.bounds.size);
    }
}