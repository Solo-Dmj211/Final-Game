using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 20f;
    public float lifetime = 2f;
    public int damage = 10; // NEW: how much damage this bullet deals
    
    [Header("Collision")]
    public LayerMask enemyLayer; 
    public LayerMask environmentLayer; 
    
    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime); 
    }

    public void Setup(float direction)
    {
        float originalX = Mathf.Abs(transform.localScale.x);
        transform.localScale = new Vector3(originalX * direction, transform.localScale.y, transform.localScale.z);
        rb.linearVelocity = new Vector2(speed * direction, 0);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // check if the object's layer matches either of our masks
        bool hitEnemy = (enemyLayer.value & (1 << hitInfo.gameObject.layer)) > 0;
        bool hitEnv = (environmentLayer.value & (1 << hitInfo.gameObject.layer)) > 0;

        if (hitEnemy)
        {
            // Sarun: i am trying to deal damage to the enemy
            EnemyHealth eh = hitInfo.GetComponent<EnemyHealth>();
            if (eh != null)
            {
                eh.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (hitEnv)
        {
            Destroy(gameObject);
        }
    }
}
