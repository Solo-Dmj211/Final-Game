using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Drives the Options Menu UI in MainMenu scene.
// Reads/writes through the AudioSettings singleton so all changes propagate
// across scenes immediately and persist across launches.
public class OptionsMenu : MonoBehaviour
{
    [Header("Volume")]
    [Tooltip("Slider that controls master volume (0 to 1).")]
    public Slider volumeSlider;

    [Header("Mute Button")]
    [Tooltip("Button that toggles mute on/off. Its label text swaps between two strings below.")]
    public Button muteButton;
    [Tooltip("TextMeshProUGUI label on the Mute button (used to show MUTE vs UNMUTE).")]
    public TextMeshProUGUI muteButtonLabel;
    public string mutedLabel   = "UNMUTE";
    public string unmutedLabel = "MUTE";

    void OnEnable()
    {
        // Whenever the panel is shown, sync UI with current settings.
        if (AudioSettings.Instance == null) return;

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.SetValueWithoutNotify(AudioSettings.Instance.Volume);
        }
        RefreshMuteLabel();
    }

    // Hook this to the slider's "On Value Changed" event (dynamic float).
    public void OnVolumeChanged(float value)
    {
        if (AudioSettings.Instance == null) return;
        AudioSettings.Instance.Volume = value;
    }

    // Hook this to the mute Button's "On Click" event.
    // Toggles the mute state every press and updates the label.
    public void OnMuteButtonPressed()
    {
        if (AudioSettings.Instance == null) return;
        AudioSettings.Instance.Muted = !AudioSettings.Instance.Muted;
        RefreshMuteLabel();
    }

    void RefreshMuteLabel()
    {
        if (muteButtonLabel == null || AudioSettings.Instance == null) return;
        muteButtonLabel.text = AudioSettings.Instance.Muted ? mutedLabel : unmutedLabel;
    }
}