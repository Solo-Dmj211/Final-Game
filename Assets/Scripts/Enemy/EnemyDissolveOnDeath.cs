using UnityEngine;

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

    static readonly int ID_DissolveAmount = Shader.PropertyToID("_DissolveAmount");
    static readonly int ID_EdgeColor      = Shader.PropertyToID("_EdgeColor");
    static readonly int ID_EdgeWidth      = Shader.PropertyToID("_EdgeWidth");
    static readonly int ID_NoiseScale     = Shader.PropertyToID("_NoiseScale");

    SpriteRenderer sr;
    Material runtimeMat;
    Enemy enemy; // was EnemyHealth
    bool dissolving = false;
    bool dropped = false;
    float timer = 0f;

    void Awake()
    {
        sr     = GetComponent<SpriteRenderer>();
        enemy  = GetComponent<Enemy>(); // was EnemyHealth
    }

    public void BeginDissolve()
    {
        if (dissolving) return;
        dissolving = true;

        if (sr == null || dissolveMaterial == null)
        {
            if (enemy != null) enemy.DropResources(); // was health
            Destroy(gameObject);
            return;
        }

        runtimeMat = new Material(dissolveMaterial);
        sr.material = runtimeMat;

        runtimeMat.SetFloat(ID_DissolveAmount, 0f);
        if (overrideShaderValues)
        {
            runtimeMat.SetColor(ID_EdgeColor,  edgeColor);
            runtimeMat.SetFloat(ID_EdgeWidth,  edgeWidth);
            runtimeMat.SetFloat(ID_NoiseScale, noiseScale);
        }

        Collider2D[] cols = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D c in cols) c.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour s in scripts)
        {
            if (s == this) continue;
            if (s is Enemy) continue; // was EnemyHealth — keep Enemy alive for DropResources
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

        if (!dropped && t >= dropAt)
        {
            dropped = true;
            if (enemy != null) enemy.DropResources(); // was health
        }

        if (t >= 1f)
        {
            if (!dropped && enemy != null) enemy.DropResources(); // was health
            if (runtimeMat != null) Destroy(runtimeMat);
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (runtimeMat != null) Destroy(runtimeMat);
    }
}