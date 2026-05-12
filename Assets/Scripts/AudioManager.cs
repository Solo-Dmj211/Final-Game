using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    public AudioClip gameplayMusic;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    [Header("SFX Clips")]
    public AudioClip enemyDeath;
    public AudioClip gameOver;
    public AudioClip gameWon;
    public AudioClip levelUp;
    public AudioClip melee;
    public AudioClip loseLife;

    [Header("Scenes That Play Gameplay Music")]
    public string[] gameplayScenes = { "Map1", "Map2", "Map3", "Home" };

    AudioSource musicSource;
    AudioSource sfxSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // create the two audio sources on this gameobject
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        HandleMusicForScene(SceneManager.GetActiveScene().name);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandleMusicForScene(scene.name);
    }

    void HandleMusicForScene(string sceneName)
    {
        bool isGameplay = System.Array.Exists(gameplayScenes, s => s == sceneName);

        if (isGameplay && gameplayMusic != null)
        {
            if (musicSource.clip != gameplayMusic || !musicSource.isPlaying)
            {
                musicSource.clip = gameplayMusic;
                musicSource.volume = musicVolume;
                musicSource.Play();
            }
        }
        else
        {
            musicSource.Stop();
        }
    }
    public static void PlaySFX(AudioClip clip)
    {
        if (Instance == null || clip == null) return;
        Instance.sfxSource.PlayOneShot(clip);
    }

    public void PlayEnemyDeath() => PlaySFX(enemyDeath);
    public void PlayGameOver() => PlaySFX(gameOver);
    public void PlayGameWon() => PlaySFX(gameWon);
    public void PlayLevelUp() => PlaySFX(levelUp);
    public void PlayMelee() => PlaySFX(melee);
    public void PlayLoseLife() => PlaySFX(loseLife);
}