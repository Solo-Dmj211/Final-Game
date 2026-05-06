using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 30;
    public int currentHP;

    [Header("Drops")]
    public GameObject resourcePrefab; // assign in inspector
    public int resourceDropAmount = 1; // how many resources to drop
    public Vector2 dropForce = new Vector2(2f, 4f); // pop force on drop

    [Header("Effects")]
    public float flashDuration = 0.1f; // visual feedback on hit
    public Color flashColor = Color.red;

    SpriteRenderer sr;
    Color originalColor;
    float flashTimer = 0f;

    void Awake()
    {
        currentHP = maxHP;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
    }

    void Update()
    {
        // damage flash effect
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f && sr != null)
            {
                sr.color = originalColor;
            }
        }
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        Debug.Log(gameObject.name + " took " + amount + " damage. hp: " + currentHP);

        // flash red briefly
        if (sr != null)
        {
            sr.color = flashColor;
            flashTimer = flashDuration;
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " died");
        DropResources();
        Destroy(gameObject);
    }

    void DropResources()
    {
        if (resourcePrefab == null) return;

        for (int i = 0; i < resourceDropAmount; i++)
        {
            GameObject drop = Instantiate(resourcePrefab, transform.position, Quaternion.identity);

            // give it a little pop so it doesn't stack on top of itself
            Rigidbody2D dropRb = drop.GetComponent<Rigidbody2D>();
            if (dropRb != null)
            {
                float randomX = Random.Range(-dropForce.x, dropForce.x);
                dropRb.linearVelocity = new Vector2(randomX, dropForce.y);
            }
        }
    }
}
