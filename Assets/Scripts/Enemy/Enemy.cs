using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRangeX = 8f;
    public float detectionRangeY = 3f;
    public float attackRange = 1f;
    public LayerMask wallLayer;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;

    [Header("Patrol")]
    public bool stationary = false;
    public Transform[] patrolPoints;
    int patrolIndex = 0;

    [Header("Wake Up")]
    public float wakeUpDuration = 1f;

    [Header("Health")]
    public int maxHP = 30;
    public int currentHP;

    [Header("Effects")]
    public float flashDuration = 0.1f;
    public Color flashColor = Color.red;

    [Header("Drops")]
    public GameObject resourcePrefab;
    public int resourceDropAmount = 1;
    public Vector2 dropForce = new Vector2(2f, 4f);

    [Header("Score")]
    public int scoreToGive = 10;

    [Header("Attack")]
    public int attackDamage = 25;
    public float attackCooldown = 1f;

    float nextAttackTime = 0f;

    enum State { Patrol, WakingUp, Chase, Attack }
    State currentState = State.Patrol;

    Transform player;
    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;
    Color originalColor;

    bool isFacingRight = true;
    bool isDead = false;

    float wakeUpTimer = 0f;
    float knockbackEndTime = 0f;
    float flashTimer = 0f;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr   = GetComponentInChildren<SpriteRenderer>();

        currentHP = maxHP;
        if (sr != null) originalColor = sr.color;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning($"{gameObject.name}: Could not find Player tag in scene.");

        if (anim != null)
        {
            anim.SetBool("PlayerSpotted", false);
            anim.SetBool("Chasing",       false);
            anim.SetBool("Patrolling",    !stationary);
        }
    }

    void Update()
    {
        HandleFlash();

        if (isDead) return;
        if (player == null) return;
        if (Time.time < knockbackEndTime) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (currentState == State.WakingUp)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            wakeUpTimer -= Time.deltaTime;
            if (wakeUpTimer <= 0f)
                SetState(State.Chase);
            return;
        }

        if (distToPlayer <= attackRange)
            SetState(State.Attack);
        else if (CanSeePlayer())
        {
            if (currentState == State.Patrol)
                SetState(State.WakingUp);
            else
                SetState(State.Chase);
        }
        else
            SetState(State.Patrol);

        switch (currentState)
        {
            case State.Patrol: HandlePatrol(); break;
            case State.Chase:  HandleChase();  break;
            case State.Attack: HandleAttack(); break;
        }
    }

    // =========================================================================
    // State machine
    // =========================================================================

    void SetState(State newState)
    {
        if (currentState == State.WakingUp && newState == State.WakingUp) return;

        bool stateChanged = newState != currentState;
        currentState = newState;

        if (anim == null) return;

        switch (currentState)
        {
            case State.Patrol:
                if (!stateChanged) return;
                anim.SetBool("PlayerSpotted", false);
                anim.SetBool("Chasing",       false);
                anim.SetBool("Patrolling",    !stationary);
                break;

            case State.WakingUp:
                if (!stateChanged) return;
                anim.SetBool("PlayerSpotted", true);
                anim.SetBool("Chasing",       false);
                anim.SetBool("Patrolling",    false);
                wakeUpTimer = wakeUpDuration;
                break;

            case State.Chase:
                if (!stateChanged) return;
                anim.SetBool("PlayerSpotted", true);
                anim.SetBool("Chasing",       true);
                anim.SetBool("Patrolling",    false);
                break;

            case State.Attack:
                if (stateChanged)
                {
                    anim.SetBool("PlayerSpotted", true);
                    anim.SetBool("Chasing",       false);
                    anim.SetBool("Patrolling",    false);
                }
                if (Time.time >= nextAttackTime
                    && player != null
                    && Vector2.Distance(transform.position, player.position) <= attackRange)
                {
                    anim.SetTrigger("Attack");
                    nextAttackTime = Time.time + attackCooldown;
                }
                break;
        }
    }

    // =========================================================================
    // Movement
    // =========================================================================

    void HandlePatrol()
    {
        if (stationary || patrolPoints.Length == 0) return;

        Transform target = patrolPoints[patrolIndex];
        MoveToward(target.position, patrolSpeed);

        if (Mathf.Abs(transform.position.x - target.position.x) < 0.2f)
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    void HandleChase()
    {
        MoveToward(player.position, chaseSpeed);
    }

    void HandleAttack()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    void MoveToward(Vector3 target, float speed)
    {
        Vector2 dir = (target - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * speed, rb.linearVelocity.y);
        HandleFlip(dir.x);
    }

    void HandleFlip(float dirX)
    {
        if ((dirX > 0 && !isFacingRight) || (dirX < 0 && isFacingRight))
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    // =========================================================================
    // Health
    // =========================================================================

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHP -= amount;
        anim?.SetTrigger("Hit");

        if (sr != null)
        {
            sr.color = flashColor;
            flashTimer = flashDuration;
        }

        Debug.Log(gameObject.name + " took " + amount + " damage. hp: " + currentHP);

        if (currentHP <= 0)
            Die();
    }

    public void ApplyKnockback(Vector2 force)
    {
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);
        knockbackEndTime = Time.time + 0.3f;
    }

    void HandleFlash()
    {
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f && sr != null)
                sr.color = originalColor;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayEnemyDeath();
        GameManager.Instance.AddScore(scoreToGive);

        Debug.Log(gameObject.name + " died");
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        anim?.SetTrigger("Die");

        if (anim != null)
        {
            yield return null;
            while (!anim.GetCurrentAnimatorStateInfo(0).IsName("Die"))
                yield return null;
            while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                yield return null;
        }

        EnemyDissolveOnDeath dissolve = GetComponent<EnemyDissolveOnDeath>();
        if (dissolve != null)
            dissolve.BeginDissolve();
        else
        {
            DropResources();
            Destroy(gameObject);
        }
    }

    // =========================================================================
    // Drops
    // =========================================================================

    public void DropResources()
    {
        if (resourcePrefab == null) return;

        for (int i = 0; i < resourceDropAmount; i++)
        {
            GameObject drop = Instantiate(resourcePrefab, transform.position, Quaternion.identity);
            Rigidbody2D dropRb = drop.GetComponent<Rigidbody2D>();
            if (dropRb != null)
            {
                float randomX = Random.Range(-dropForce.x, dropForce.x);
                dropRb.linearVelocity = new Vector2(randomX, dropForce.y);
            }
        }
    }

    // =========================================================================
    // Attack damage — called by Animation Event, not by collision
    // =========================================================================

    public void DealAttackDamage()
    {
        if (player == null) return;
        if (Vector2.Distance(transform.position, player.position) > attackRange) return;

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
            ph.TakeDamage(attackDamage);
    }

        bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector2 toPlayer = player.position - transform.position;

        float ellipse = (toPlayer.x / detectionRangeX) * (toPlayer.x / detectionRangeX)
                    + (toPlayer.y / detectionRangeY) * (toPlayer.y / detectionRangeY);

        if (ellipse > 1f) return false;

        // Cast a ray toward the player and bail if a wall is in the way.
        float dist = toPlayer.magnitude;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer.normalized, dist, wallLayer);
        if (hit.collider != null) return false;

        return true;
    }

    // =========================================================================
    // Gizmos
    // =========================================================================

    void OnDrawGizmosSelected()
    {
        // Draw the elliptical detection range using line segments
        Gizmos.color = Color.yellow;
        int segments = 40;
        Vector3 prevPoint = transform.position + new Vector3(detectionRangeX, 0, 0);
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector3 nextPoint = transform.position + new Vector3(
                Mathf.Cos(angle) * detectionRangeX,
                Mathf.Sin(angle) * detectionRangeY,
                0);
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}