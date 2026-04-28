using HighScore;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        HS.Init(this, "System Failure");
    }

    public void SubmitScore(string playerName, int score)
    {
        HS.SubmitHighScore(this, playerName, score);
    }
}