using UnityEngine;

public class GroundEnemy : EnemyBase
{

    protected override void HandleAttack()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        // trigger attack animation, deal damage
    }
}