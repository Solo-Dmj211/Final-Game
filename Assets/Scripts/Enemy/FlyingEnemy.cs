using UnityEngine;

public class FlyingEnemy : EnemyBase
{
    [Header("Flying")]
    public float hoverAmplitude = 0.3f;
    public float hoverFrequency = 2f;

    [Header("Obstacle Avoidance")]
    public float avoidanceRange = 1.5f;    // how far ahead to check for walls
    public float avoidanceStrength = 5f;   // how hard it steers away
    public LayerMask obstacleLayer;

    private float startY;
    private Vector2 currentVelocity;

    protected override void Awake()
    {
        base.Awake();
        startY = transform.position.y;
        rb.gravityScale = 0f;
    }

    protected override void MoveToward(Vector3 target, float speed)
    {
        Vector2 desiredDir = (target - transform.position).normalized;
        Vector2 avoidance = GetAvoidanceForce();

        // Blend desired direction with avoidance steering
        Vector2 finalDir = (desiredDir + avoidance).normalized;

        rb.linearVelocity = finalDir * speed;
        HandleFlip(finalDir.x);
    }

    private Vector2 GetAvoidanceForce()
    {
        Vector2 avoidance = Vector2.zero;

        // Cast rays in a fan around the enemy's facing direction
        Vector2[] directions = {
            transform.right,                           // forward
            Quaternion.Euler(0,0, 45)  * transform.right,  // up-forward
            Quaternion.Euler(0,0,-45)  * transform.right,  // down-forward
            Quaternion.Euler(0,0, 90)  * transform.right,  // up
            Quaternion.Euler(0,0,-90)  * transform.right,  // down
        };

        foreach (Vector2 dir in directions)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                dir,
                avoidanceRange,
                obstacleLayer
            );

            if (hit.collider != null)
            {
                float proximity = 1f - (hit.distance / avoidanceRange);
                avoidance -= dir * proximity * avoidanceStrength;
            }
        }

        return avoidance;
    }

    protected override void HandlePatrol()
    {
        if (stationary || patrolPoints.Length == 0)
        {
            float hoverY = startY + Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
            transform.position = new Vector3(transform.position.x, hoverY, transform.position.z);
            rb.linearVelocity = Vector2.zero;
            return;
        }

        base.HandlePatrol();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, avoidanceRange);
    }
}