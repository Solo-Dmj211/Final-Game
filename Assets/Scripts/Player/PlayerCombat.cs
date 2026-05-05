using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("State")]
    public bool isMeleeEquipped = false; // false = ranged, true = melee

    [Header("Ranged")]
    public GameObject rangedPrefab;
    public Transform rangedPoint;
    public float rangedRate = 0.2f;
    
    float nextRangedTime;

    [Header("Melee")]
    public Transform meleePoint;
    public float meleeRange = 0.5f;
    public float meleeRate = 0.5f;
    public int meleeDamage = 20; // NEW: how much damage melee deals
    public LayerMask enemyLayer;
    
    float nextMeleeTime;

    // one unified attack input
    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (isMeleeEquipped && Time.time >= nextMeleeTime)
            {
                MeleeAttack();
                nextMeleeTime = Time.time + meleeRate;
            }
            else if (!isMeleeEquipped && Time.time >= nextRangedTime)
            {
                RangedAttack();
                nextRangedTime = Time.time + rangedRate;
            }
        }
    }

    // toggles between the two states
    public void OnSwapWeapon(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            isMeleeEquipped = !isMeleeEquipped;
            Debug.Log(isMeleeEquipped ? "swapped to: melee" : "swapped to: ranged");
        }
    }

    void RangedAttack()
    {
        // spawn the projectile
        GameObject projectile = Instantiate(rangedPrefab, rangedPoint.position, rangedPoint.rotation);

        // check which way the player is facing
        float facingDirection = Mathf.Sign(transform.localScale.x);

        // pass the direction to the projectile's script
        Bullet projectileScript = projectile.GetComponent<Bullet>();
        if (projectileScript != null)
        {
            projectileScript.Setup(facingDirection);
        }
    }

    void MeleeAttack()
    {
        // grab everything inside the hitbox
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(meleePoint.position, meleeRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("punched: " + enemy.name);
            
            // Sarun: actually trying to deal damage
            EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
            if (eh != null)
            {
                eh.TakeDamage(meleeDamage);
            }
        }
    }

    // draws the melee hitbox in the editor
    void OnDrawGizmosSelected()
    {
        if (meleePoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(meleePoint.position, meleeRange);
    }

}
