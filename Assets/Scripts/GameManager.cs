using HighScore;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class MapData
{
    public string mapName;
    public string sceneName;
    public bool unlocked;
    public bool completed;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int money = 0;
    public int towerLevel = 0;
    public string playerName = "Player1";

    [Header("Maps")]
    public MapData[] maps;

    [Header("Score")]
    public float gameTimer = 0f;
    public bool timerRunning = false;

    [Header("Time Bonus")]
    public int timeBonusMax = 5000;
    public float timeBonusWindow = 300f;

    int finalScore = 0;

    void Start()
    {
        HS.Init(this, "Systems Failure");
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (timerRunning)
            gameTimer += Time.deltaTime;
    }

    public void StartTimer()
    {
        gameTimer = 0f;
        timerRunning = true;
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    public void ApplyTimeBonus()
    {
        float ratio = Mathf.Clamp01(1f - (gameTimer / timeBonusWindow));
        int bonus = Mathf.RoundToInt(timeBonusMax * ratio);
        finalScore += bonus;
        Debug.Log($"Time bonus: +{bonus} ({gameTimer:F1}s elapsed)");
    }

    public void SubmitScore()
    {
        HS.SubmitHighScore(this, playerName, math.max(0, finalScore));
    }

    public void LockMap(int mapIndex)
    {
        if (!IsValidMapIndex(mapIndex)) return;
        maps[mapIndex].unlocked = false;
        Debug.Log($"Locked map: {maps[mapIndex].mapName}");
    }

    public void UnlockMap(int mapIndex)
    {
        if (!IsValidMapIndex(mapIndex)) return;
        maps[mapIndex].unlocked = true;
        Debug.Log($"Unlocked map: {maps[mapIndex].mapName}");
    }

    public void CompleteMap(int mapIndex)
    {
        if (!IsValidMapIndex(mapIndex)) return;
        maps[mapIndex].completed = true;
        maps[mapIndex].unlocked = false;
        Debug.Log($"Completed map: {maps[mapIndex].mapName}");
        UnlockMap(mapIndex + 1);
    }
    public bool IsMapUnlocked(int mapIndex)
    {
        if (!IsValidMapIndex(mapIndex)) return false;
        return maps[mapIndex].unlocked;
    }

    bool IsValidMapIndex(int index)
    {
        if (index < 0 || index >= maps.Length)
        {
            Debug.LogWarning($"Map index {index} is out of range.");
            return false;
        }
        return true;
    }

    public void AddScore(int amount)
    {
        finalScore += amount;
    }

    public void RemoveScore(int amount)
    {
        finalScore -= amount;
    }

    public int GetFinalScore()
    {
        return finalScore;
    }

    public void ResetGame()
    {
        SubmitScore();
        StopTimer();
        finalScore      = 0;
        gameTimer       = 0f;
        timerRunning    = false;
        money           = 0;
        towerLevel      = 0;
        playerName      = "Player1";

        for (int i = 0; i < maps.Length; i++)
        {
            maps[i].completed = false;
            maps[i].unlocked  = i == 0;
        }

        Debug.Log("GameManager: Game state reset.");
    }
}