using UnityEngine;

// Global audio settings. Lives as a DontDestroyOnLoad singleton so the user's
// choices persist across all scenes (MainMenu, Home, Map1, Map2, Map3).
// Values are stored in PlayerPrefs so they also persist across launches.
//
// Applied via AudioListener.volume which is the global multiplier Unity uses
// for ALL audio sources, regardless of scene. One source of truth.
public class AudioSettings : MonoBehaviour
{
    public static AudioSettings Instance { get; private set; }

    // PlayerPrefs keys
    const string KEY_VOLUME = "Audio.MasterVolume";
    const string KEY_MUTED  = "Audio.Muted";

    [Header("Defaults")]
    [Range(0f, 1f)] public float defaultVolume = 0.8f;

    float currentVolume;
    bool currentMuted;

    void Awake()
    {
        // Singleton: only one allowed across the whole game.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load saved settings on startup.
        currentVolume = PlayerPrefs.GetFloat(KEY_VOLUME, defaultVolume);
        currentMuted  = PlayerPrefs.GetInt(KEY_MUTED, 0) == 1;

        ApplyToUnity();
    }

    // ----- Public API used by the UI -----

    public float Volume
    {
        get { return currentVolume; }
        set
        {
            currentVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(KEY_VOLUME, currentVolume);
            PlayerPrefs.Save();
            ApplyToUnity();
        }
    }

    public bool Muted
    {
        get { return currentMuted; }
        set
        {
            currentMuted = value;
            PlayerPrefs.SetInt(KEY_MUTED, currentMuted ? 1 : 0);
            PlayerPrefs.Save();
            ApplyToUnity();
        }
    }

    void ApplyToUnity()
    {
        // AudioListener.volume is the global multiplier applied to ALL audio.
        // Setting it here means every scene immediately picks up the change.
        AudioListener.volume = currentMuted ? 0f : currentVolume;
    }
}
