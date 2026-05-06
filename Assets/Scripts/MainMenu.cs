using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    public string gameSceneName = "NewScene"; // name of the gameplay scene to load

    [Header("Panels")]
    public GameObject mainPanel;     // the main buttons (Play, Controls, Quit)
    public GameObject controlsPanel; // the controls overlay

    void Start()
    {
        // start on the main panel, controls hidden
        ShowMain();
    }

    public void PlayGame()
    {
        Debug.Log("Loading game scene: " + gameSceneName);
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
        if (mainPanel != null) mainPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    public void ShowMain()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }
}
