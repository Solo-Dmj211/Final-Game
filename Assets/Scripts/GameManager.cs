using HighScore;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int money = 0;
    void Start()
    {
        HS.Init(this, "System Failure");
    }

    void Awake()
    {
        // If one already exists, destroy this duplicate
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
}