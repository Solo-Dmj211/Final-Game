using UnityEngine;

public class Resource : MonoBehaviour
{
    [Header("Value")]
    public int value = 1;

    [Header("Pickup Behavior")]
    public float magnetRange = 2f; // when player gets within this range, resource flies toward them
    public float magnetSpeed = 8f;
    public float lifetime = 30f; // resources disappear after 30 seconds so the world doesn't clutter

    Transform player;

    void Start()
    {
        // find the player (only do this once at spawn)
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // magnetism: when player is close, fly toward them
        if (dist <= magnetRange)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            transform.position += (Vector3)(dir * magnetSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // give resource to the player
            PlayerInteract pi = other.GetComponent<PlayerInteract>();
            if (pi != null)
            {
                pi.AddCoins(value);
                Debug.Log("picked up " + value + " resource(s). total coins: " + pi.coins);
            }

            Destroy(gameObject);
        }
    }
}