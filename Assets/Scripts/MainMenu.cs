using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    public string gameSceneName = "Home"; // name of the gameplay scene to load

    [Header("Panels")]
    public GameObject mainPanel;     // the main buttons (Play, Controls, Options, Quit)
    public GameObject controlsPanel; // the controls overlay
    public GameObject optionsPanel;  // the options overlay
    public GameObject highscorePanel; // where the user enters their highscore
    [Header("High Score")]
    public TMP_InputField playerNameInput;

    void Start() 
    {
        // start on the main panel, others hidden
        ShowMain();
    }

    public void PlayGame()
    {
        // Grab whatever the player typed; fall back to "Player" if left blank.
        string playerName = playerNameInput != null && playerNameInput.text.Trim() != ""
            ? playerNameInput.text.Trim()
            : "Player";

        GameManager.Instance.playerName = playerName;

        Debug.Log("Loading game scene: " + gameSceneName);
        GameManager.Instance.StartTimer();
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void ShowControls()
    {
        if (mainPanel != null)     mainPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);
        if (optionsPanel != null)  optionsPanel.SetActive(false);
        if (highscorePanel != null)  highscorePanel.SetActive(false);
    }

    public void ShowOptions()
    {
        if (mainPanel != null)     mainPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (optionsPanel != null)  optionsPanel.SetActive(true);
        if (highscorePanel != null)  highscorePanel.SetActive(false);
    }

    public void ShowHS()
    {
        if (mainPanel != null)     mainPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (optionsPanel != null)  optionsPanel.SetActive(false);
        if (highscorePanel != null)  highscorePanel.SetActive(true);
    }

    public void ShowMain()
    {
        if (mainPanel != null)     mainPanel.SetActive(true);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (optionsPanel != null)  optionsPanel.SetActive(false);
        if (highscorePanel != null)  highscorePanel.SetActive(false);
    }
}