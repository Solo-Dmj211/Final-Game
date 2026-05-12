using UnityEngine;

// Replaces an enemy's sprite material with the dissolve material and animates
// _DissolveAmount from 0 -> 1 over `duration` seconds, then drops resources
// and destroys the GameObject.
//
// Designed to be called from EnemyHealth.Die() instead of an immediate Destroy.
//
// Setup on the Enemy prefab:
//   1. Add this component to the prefab root (same GameObject as EnemyHealth + SpriteRenderer).
//   2. Assign the EnemyDissolve material to the "Dissolve Material" field.
//   3. Tweak Duration / Edge Color / Edge Width / Noise Scale in the Inspector.
//
// EnemyHealth.Die() will call BeginDissolve() on this component if present,
// and this script will call back into EnemyHealth.DropResources() at the end
// so coins pop out of the cloud of dissolved particles instead of at the moment of death.
public class EnemyDissolveOnDeath : MonoBehaviour
{
    [Header("Material")]
    [Tooltip("Material that uses the EnemyDissolve shader. Will be swapped onto the SpriteRenderer at death.")]
    public Material dissolveMaterial;

    [Header("Animation")]
    [Tooltip("Seconds for the dissolve to go from 0 (intact) to 1 (fully gone).")]
    public float duration = 0.6f;

    [Tooltip("When in the animation (0-1) to drop the resources. 1.0 = at the very end.")]
    [Range(0f, 1f)]
    public float dropAt = 0.85f;

    [Header("Shader Overrides (optional)")]
    [Tooltip("If true, overrides the material's Edge Color / Width / Noise Scale with the values below per-instance.")]
    public bool overrideShaderValues = false;
    public Color edgeColor = new Color(0f, 0.8f, 1f, 1f);
    [Range(0f, 0.3f)] public float edgeWidth = 0.08f;
    public float noiseScale = 15f;

    // Cached shader property IDs.
    static readonly int ID_DissolveAmount = Shader.PropertyToID("_DissolveAmount");
    static readonly int ID_EdgeColor      = Shader.PropertyToID("_EdgeColor");
    static readonly int ID_EdgeWidth      = Shader.PropertyToID("_EdgeWidth");
    static readonly int ID_NoiseScale     = Shader.PropertyToID("_NoiseScale");

    SpriteRenderer sr;
    Material runtimeMat;
    EnemyHealth health;
    bool dissolving = false;
    bool dropped = false;
    float timer = 0f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        health = GetComponent<EnemyHealth>();
    }

    // Called by EnemyHealth.Die() in place of Destroy(gameObject).
    public void BeginDissolve()
    {
        if (dissolving) return;
        dissolving = true;

        if (sr == null || dissolveMaterial == null)
        {
            // Fallback: if anything is missing, just drop+destroy immediately so
            // we don't leave a "stuck alive" enemy on the field.
            if (health != null) health.DropResources();
            Destroy(gameObject);
            return;
        }

        // Instance the dissolve material so per-enemy values don't bleed into
        // other enemies sharing the same material asset.
        runtimeMat = new Material(dissolveMaterial);
        sr.material = runtimeMat;

        // Initial shader values.
        runtimeMat.SetFloat(ID_DissolveAmount, 0f);
        if (overrideShaderValues)
        {
            runtimeMat.SetColor(ID_EdgeColor,  edgeColor);
            runtimeMat.SetFloat(ID_EdgeWidth,  edgeWidth);
            runtimeMat.SetFloat(ID_NoiseScale, noiseScale);
        }

        // Stop the enemy from continuing AI / collisions during the dissolve.
        // Disabling colliders prevents weird damage events; setting Rigidbody to
        // Kinematic keeps it from sliding around mid-fade.
        Collider2D[] cols = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D c in cols) c.enabled = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        // Optional: disable common AI scripts so the enemy stops trying to move.
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour s in scripts)
        {
            // Don't disable ourselves or the health driver (we still need health for the drop).
            if (s == this) continue;
            if (s is EnemyHealth) continue;
            s.enabled = false;
        }
    }

    void Update()
    {
        if (!dissolving) return;

        timer += Time.deltaTime;
        float t = duration > 0f ? Mathf.Clamp01(timer / duration) : 1f;

        if (runtimeMat != null)
            runtimeMat.SetFloat(ID_DissolveAmount, t);

        // Drop resources part-way through (or at the end) so they appear out of the puff.
        if (!dropped && t >= dropAt)
        {
            dropped = true;
            if (health != null) health.DropResources();
        }

        if (t >= 1f)
        {
            // Safety net: if for some reason we never hit the drop threshold, drop now.
            if (!dropped && health != null) health.DropResources();

            // Clean up the runtime material instance to avoid a tiny leak.
            if (runtimeMat != null) Destroy(runtimeMat);
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // Safety net in case the GameObject is destroyed by something else
        // before the dissolve finishes.
        if (runtimeMat != null) Destroy(runtimeMat);
    }
}