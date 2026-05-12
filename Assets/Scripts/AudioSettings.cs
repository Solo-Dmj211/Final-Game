using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Global audio settings. Lives as a DontDestroyOnLoad singleton so the user's
// choices persist across all scenes (MainMenu, Home, Map1, Map2, Map3).
// Values are stored in PlayerPrefs so they also persist across launches.
//
// Also handles Escape-to-pause: when Escape is pressed outside of MainMenu,
// the game freezes (Time.timeScale = 0) and all input action maps are disabled
// except the OpenSettings action itself, so only the audio settings panel
// remains interactive. Pressing Escape again resumes.
public class AudioSettings : MonoBehaviour
{
    public static AudioSettings Instance { get; private set; }

    const string KEY_VOLUME = "Audio.MasterVolume";
    const string KEY_MUTED  = "Audio.Muted";

    [Header("Defaults")]
    [Range(0f, 1f)] public float defaultVolume = 0.8f;

    [Header("Pause / Settings")]
    public InputActionReference openSettingsAction;
    public InputActionAsset inputActions;

    // Found automatically on each scene load — not set in Inspector.
    GameObject settingsPanel;

    public bool IsPaused { get; private set; }

    float currentVolume;
    bool  currentMuted;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentVolume = PlayerPrefs.GetFloat(KEY_VOLUME, defaultVolume);
        currentMuted  = PlayerPrefs.GetInt(KEY_MUTED, 0) == 1;

        ApplyToUnity();
    }

    void OnEnable()
    {
        if (openSettingsAction != null)
        {
            openSettingsAction.action.Enable();
            openSettingsAction.action.performed += OnOpenSettings;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        if (openSettingsAction != null)
            openSettingsAction.action.performed -= OnOpenSettings;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Called automatically by Unity whenever a new scene finishes loading.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Always reset pause state on scene transition.
        IsPaused       = false;
        Time.timeScale = 1f;

        // Recursively search the full scene hierarchy including inactive objects,
        // which GameObject.Find() silently skips.
        settingsPanel = FindInSceneIncludingInactive("OptionsPanel");

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            Debug.Log($"AudioSettings: Found OptionsPanel in '{scene.name}'.");
        }
        else
        {
            Debug.LogWarning($"AudioSettings: No 'OptionsPanel' found in '{scene.name}'. Escape-to-pause will not work.");
        }
    }

    // Searches every root object and all descendants, including inactive ones.
    GameObject FindInSceneIncludingInactive(string objectName)
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            Transform found = FindDeep(root.transform, objectName);
            if (found != null) return found.gameObject;
        }
        return null;
    }

    // Recursively walks the full transform tree looking for a matching name.
    Transform FindDeep(Transform parent, string objectName)
    {
        if (parent.name == objectName) return parent;

        foreach (Transform child in parent)
        {
            Transform found = FindDeep(child, objectName);
            if (found != null) return found;
        }

        return null;
    }

    // -------------------------------------------------------------------------
    // Pause logic
    // -------------------------------------------------------------------------

    void OnOpenSettings(InputAction.CallbackContext ctx)
    {
        // Never pause on MainMenu — the menu handles its own state.
        if (SceneManager.GetActiveScene().name == "MainMenu") return;

        if (IsPaused)
            Resume();
        else
            Pause();
    }

    void Pause()
    {
        IsPaused       = true;
        Time.timeScale = 0f;

        // Disable every action map EXCEPT the one driving this Escape button,
        // so that gameplay inputs (movement, attack, etc.) are fully frozen.
        if (inputActions != null)
        {
            foreach (InputActionMap map in inputActions.actionMaps)
            {
                if (openSettingsAction != null &&
                    map == openSettingsAction.action.actionMap)
                    continue;

                map.Disable();
            }
        }

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void Resume()
    {
        IsPaused       = false;
        Time.timeScale = 1f;

        // Re-enable all action maps that were disabled on pause.
        if (inputActions != null)
        {
            foreach (InputActionMap map in inputActions.actionMaps)
                map.Enable();
        }

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // Public API used by the settings UI
    // -------------------------------------------------------------------------

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
        AudioListener.volume = currentMuted ? 0f : currentVolume;
    }
}