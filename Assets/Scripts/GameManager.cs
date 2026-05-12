using HighScore;
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
    public int tower_level = 0;

    [Header("Maps")]
    public MapData[] maps;

    void Start()
    {
        HS.Init(this, "System Failure");
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

    public void SubmitScore(string playerName, int score)
    {
        HS.SubmitHighScore(this, playerName, score);
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
}