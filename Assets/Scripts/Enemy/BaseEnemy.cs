using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Detection")]
    public Transform player;
    public float detectionRange = 5f;
    public float attackRange = 1f;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;

    [Header("Patrol")]
    public bool stationary = false;
    public Transform[] patrolPoints;
    private int patrolIndex = 0;

    protected Rigidbody2D rb;
    protected bool isFacingRight = true;
    protected enum State { Patrol, Chase, Attack }
    protected State state = State.Patrol;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer <= attackRange)
            state = State.Attack;
        else if (distToPlayer <= detectionRange)
            state = State.Chase;
        else
            state = State.Patrol;

        switch (state)
        {
            case State.Patrol: HandlePatrol(); break;
            case State.Chase:  HandleChase();  break;
            case State.Attack: HandleAttack(); break;
        }
    }

    protected virtual void HandlePatrol()
    {
        if (stationary || patrolPoints.Length == 0) return;

        Transform target = patrolPoints[patrolIndex];
        MoveToward(target.position, patrolSpeed);

        if (Vector2.Distance(transform.position, target.position) < 0.2f)
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    protected virtual void HandleChase()
    {
        MoveToward(player.position, chaseSpeed);
    }

    protected virtual void HandleAttack()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    protected virtual void MoveToward(Vector3 target, float speed)
    {
        Vector2 dir = (target - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * speed, rb.linearVelocity.y);
        HandleFlip(dir.x);
    }

    protected void HandleFlip(float dirX)
    {
        if ((dirX > 0 && !isFacingRight) || (dirX < 0 && isFacingRight))
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}