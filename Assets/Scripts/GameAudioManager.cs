using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer gameAudioMixer;

    [Header("Music Playlist - add 2 or more audio clips")]
    [SerializeField] private AudioClip[] musicTracks;

    [Header("Default Volumes (0-1)")]
    [Range(0f, 1f)][SerializeField] private float defaultMusicVolume = 0.3f;
    [Range(0f, 1f)][SerializeField] private float defaultSfxVolume = 0.7f;

    private const string MusicKey = "Cirks_MusicVolume";
    private const string SfxKey = "Cirks_SfxVolume";
    private const string TrackKey = "Cirks_SelectedMusicTrack";

    private AudioSource musicSource;
    private int currentTrackIndex;

    public float MusicVolume { get; private set; }
    public float SfxVolume { get; private set; }
    public int CurrentTrackIndex => currentTrackIndex;
    public int TrackCount => musicTracks != null ? musicTracks.Length : 0;
    public string CurrentTrackName =>
        musicSource != null && musicSource.clip != null
            ? musicSource.clip.name
            : "No music";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = GetComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;

        MusicVolume = PlayerPrefs.GetFloat(MusicKey, defaultMusicVolume);
        SfxVolume = PlayerPrefs.GetFloat(SfxKey, defaultSfxVolume);
        currentTrackIndex = PlayerPrefs.GetInt(TrackKey, 0);
    }

    private void Start()
    {
        ApplyMixerVolume("MusicVolume", MusicVolume);
        ApplyMixerVolume("SFXVolume", SfxVolume);

        if (TrackCount > 0)
        {
            currentTrackIndex = Mathf.Clamp(currentTrackIndex, 0, TrackCount - 1);
            PlayTrack(currentTrackIndex, false);
        }
        else if (musicSource.clip != null && !musicSource.isPlaying)
        {
            // Keep backward compatibility with the previous single-track setup.
            musicSource.Play();
        }
    }

    public void NextTrack()
    {
        if (TrackCount == 0) return;
        PlayTrack((currentTrackIndex + 1) % TrackCount);
    }

    public void PreviousTrack()
    {
        if (TrackCount == 0) return;
        PlayTrack((currentTrackIndex - 1 + TrackCount) % TrackCount);
    }

    public void PlayTrack(int index)
    {
        PlayTrack(index, true);
    }

    private void PlayTrack(int index, bool saveSelection)
    {
        if (index < 0 || index >= TrackCount || musicTracks[index] == null)
        {
            Debug.LogWarning("GameAudioManager: Track is missing or index is invalid.");
            return;
        }

        currentTrackIndex = index;
        musicSource.Stop();
        musicSource.clip = musicTracks[index];
        musicSource.Play();

        if (saveSelection)
        {
            PlayerPrefs.SetInt(TrackKey, currentTrackIndex);
            PlayerPrefs.Save();
        }
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MusicKey, MusicVolume);
        ApplyMixerVolume("MusicVolume", MusicVolume);
    }

    public void SetSfxVolume(float value)
    {
        SfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SfxKey, SfxVolume);
        ApplyMixerVolume("SFXVolume", SfxVolume);
    }

    private void ApplyMixerVolume(string parameterName, float sliderValue)
    {
        if (gameAudioMixer == null)
        {
            Debug.LogWarning("GameAudioMixer is not assigned on BackgroundMusic.");
            return;
        }

        float decibels = sliderValue <= 0.0001f
            ? -80f
            : Mathf.Log10(sliderValue) * 20f;

        if (!gameAudioMixer.SetFloat(parameterName, decibels))
            Debug.LogWarning("Audio Mixer parameter not found: " + parameterName);
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}
