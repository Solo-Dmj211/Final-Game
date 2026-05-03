using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Ranged")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireRate = 0.2f;

    [Header("Melee")]
    public Transform meleePoint;
    public float meleeRange = 0.5f;
    public float meleeRate = 0.5f;
    public LayerMask enemyLayer;

    float nextFireTime;
    float nextMeleeTime;

    // shooting input
    public void OnFire(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    // melee input
    public void OnMelee(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && Time.time >= nextMeleeTime)
        {
            MeleeAttack();
            nextMeleeTime = Time.time + meleeRate;
        }
    }

    void Shoot()
    {
        // spawn the bullet
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // check which way the player is facing using localscale
        float facingDirection = Mathf.Sign(transform.localScale.x);

        // pass the direction to the bullet's own script
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Setup(facingDirection);
        }
    }

    void MeleeAttack()
    {
        // draw an invisible circle and grab everything inside it on the enemy layer
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(meleePoint.position, meleeRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("punched: " + enemy.name);
            // apply damage here later, e.g., enemy.GetComponent<EnemyHealth>().TakeDamage(10);
        }
    }

    // draws a red circle in the editor so you can see your melee range
    void OnDrawGizmosSelected()
    {
        if (meleePoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(meleePoint.position, meleeRange);
    }
}