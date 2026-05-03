using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 2f;
    
    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime); // clean up memory
    }

    // called by PlayerCombat immediately after spawning
    public void Setup(float direction)
    {
        // flip bullet sprite if shooting left
        transform.localScale = new Vector3(direction, 1, 1);
        
        // set velocity straight forward
        rb.linearVelocity = new Vector2(speed * direction, 0);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // put damage logic here later
        Debug.Log("hit: " + hitInfo.name);
        
        // destroy bullet on impact
        Destroy(gameObject);
    }
}