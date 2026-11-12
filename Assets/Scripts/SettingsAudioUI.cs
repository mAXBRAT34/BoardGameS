using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsAudioUI : MonoBehaviour
{
    [Header("Volume Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Music Selection")]
    [SerializeField] private Button previousMusicButton;
    [SerializeField] private Button nextMusicButton;
    [SerializeField] private TMP_Text currentTrackLabel;

    private void Awake()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSfxChanged);

        if (previousMusicButton != null)
            previousMusicButton.onClick.AddListener(PreviousTrack);

        if (nextMusicButton != null)
            nextMusicButton.onClick.AddListener(NextTrack);
    }

    private void Start()
    {
        GameAudioManager manager = GameAudioManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning("GameAudioManager not found. Start from MainMenu scene.");
            return;
        }

        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            musicSlider.wholeNumbers = false;
            musicSlider.SetValueWithoutNotify(manager.MusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.wholeNumbers = false;
            sfxSlider.SetValueWithoutNotify(manager.SfxVolume);
        }

        UpdateTrackLabel();
    }

    private void NextTrack()
    {
        if (GameAudioManager.Instance == null) return;
        GameAudioManager.Instance.NextTrack();
        UpdateTrackLabel();
    }

    private void PreviousTrack()
    {
        if (GameAudioManager.Instance == null) return;
        GameAudioManager.Instance.PreviousTrack();
        UpdateTrackLabel();
    }

    private void UpdateTrackLabel()
    {
        if (currentTrackLabel == null) return;

        GameAudioManager manager = GameAudioManager.Instance;
        currentTrackLabel.text = manager != null
            ? manager.CurrentTrackName
            : "No music";
    }

    private void OnMusicChanged(float value)
    {
        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.SetMusicVolume(value);
    }

    private void OnSfxChanged(float value)
    {
        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.SetSfxVolume(value);
    }

    private void OnDestroy()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(OnMusicChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);

        if (previousMusicButton != null)
            previousMusicButton.onClick.RemoveListener(PreviousTrack);

        if (nextMusicButton != null)
            nextMusicButton.onClick.RemoveListener(NextTrack);
    }
}
