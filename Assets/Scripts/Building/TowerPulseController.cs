using UnityEngine;

// Drives the TowerPulse shader on each tier visual of an UpgradeableBuilding.
// Reads the building's current tier (via reflection of its visual states)
// and pushes per-tier shader values using a MaterialPropertyBlock so we
// don't instantiate materials and break SRP batching.
[RequireComponent(typeof(UpgradeableBuilding))]
public class TowerPulseController : MonoBehaviour
{
    [System.Serializable]
    public struct TierGlow
    {
        public Color glowColor;
        [Range(0f, 5f)] public float glowIntensity;
        [Range(0f, 20f)] public float pulseSpeed;
        [Range(0f, 1f)]  public float pulseMin;
        [Range(0f, 2f)]  public float pulseMax;
    }

    [Header("Per-Tier Glow Settings")]
    [Tooltip("One entry per tier. Should match the size of UpgradeableBuilding.tiers.")]
    public TierGlow[] tierSettings;

    [Header("Update Behavior")]
    [Tooltip("How often (seconds) to re-check the active tier. 0 = every frame.")]
    public float pollInterval = 0.1f;

    UpgradeableBuilding building;
    MaterialPropertyBlock mpb;
    int lastActiveTier = -1;
    float pollTimer = 0f;

    // Shader property IDs (cached for performance).
    static readonly int ID_GlowColor     = Shader.PropertyToID("_GlowColor");
    static readonly int ID_GlowIntensity = Shader.PropertyToID("_GlowIntensity");
    static readonly int ID_PulseSpeed    = Shader.PropertyToID("_PulseSpeed");
    static readonly int ID_PulseMin      = Shader.PropertyToID("_PulseMin");
    static readonly int ID_PulseMax      = Shader.PropertyToID("_PulseMax");

    void Awake()
    {
        building = GetComponent<UpgradeableBuilding>();
        mpb = new MaterialPropertyBlock();
    }

    void Start()
    {
        ApplyForCurrentTier();
    }

    void Update()
    {
        // Poll the building to detect tier changes from upgrades.
        pollTimer -= Time.deltaTime;
        if (pollTimer <= 0f)
        {
            pollTimer = pollInterval;
            int active = GetActiveTierIndex();
            if (active != lastActiveTier)
            {
                ApplyForCurrentTier();
            }
        }
    }

    int GetActiveTierIndex()
    {
        if (building == null || building.tiers == null) return -1;
        for (int i = 0; i < building.tiers.Length; i++)
        {
            GameObject vo = building.tiers[i].visualObject;
            if (vo != null && vo.activeSelf) return i;
        }
        return -1;
    }

    void ApplyForCurrentTier()
    {
        int tier = GetActiveTierIndex();
        lastActiveTier = tier;
        if (tier < 0) return;
        if (tierSettings == null || tier >= tierSettings.Length) return;

        TierGlow s = tierSettings[tier];

        // Apply to the active tier's SpriteRenderer (and any nested sprite renderers
        // under it, in case a tier visual is built from multiple sprites).
        GameObject vo = building.tiers[tier].visualObject;
        if (vo == null) return;

        SpriteRenderer[] renderers = vo.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer sr in renderers)
        {
            sr.GetPropertyBlock(mpb);
            mpb.SetColor(ID_GlowColor,     s.glowColor);
            mpb.SetFloat(ID_GlowIntensity, s.glowIntensity);
            mpb.SetFloat(ID_PulseSpeed,    s.pulseSpeed);
            mpb.SetFloat(ID_PulseMin,      s.pulseMin);
            mpb.SetFloat(ID_PulseMax,      s.pulseMax);
            sr.SetPropertyBlock(mpb);
        }
    }

    // Public hook in case you want to refresh immediately after an upgrade
    // (e.g. call from UpgradeableBuilding.TryUpgrade right after UpdateVisuals).
    public void RefreshNow()
    {
        ApplyForCurrentTier();
    }
}
