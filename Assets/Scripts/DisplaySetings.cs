using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplaySettings : MonoBehaviour
{
    [Header("Settings UI (leave empty in MainMenu)")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    private readonly List<Vector2Int> availableResolutions = new List<Vector2Int>();

    private const string WidthKey = "Cirks_ResolutionWidth";
    private const string HeightKey = "Cirks_ResolutionHeight";
    private const string FullscreenKey = "Cirks_Fullscreen";

    private static bool savedSettingsApplied;

    private void Awake()
    {
        // Restores the player's previous choice once at launch.
        // This component can be placed in both MainMenu and Settings.
        if (savedSettingsApplied)
            return;

        savedSettingsApplied = true;

        if (!PlayerPrefs.HasKey(WidthKey) || !PlayerPrefs.HasKey(HeightKey))
            return;

        int width = PlayerPrefs.GetInt(WidthKey);
        int height = PlayerPrefs.GetInt(HeightKey);
        bool fullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;

        if (width > 0 && height > 0)
            Screen.SetResolution(width, height, fullscreen);
    }

    private void Start()
    {
        BuildResolutionOptions();

        if (resolutionDropdown != null)
        {
            var options = new List<string>();
            foreach (Vector2Int resolution in availableResolutions)
                options.Add(resolution.x + " x " + resolution.y);

            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(options);

            int savedWidth = PlayerPrefs.GetInt(WidthKey, Screen.width);
            int savedHeight = PlayerPrefs.GetInt(HeightKey, Screen.height);
            int selectedIndex = FindResolutionIndex(savedWidth, savedHeight);

            resolutionDropdown.SetValueWithoutNotify(selectedIndex);
            resolutionDropdown.RefreshShownValue();
            resolutionDropdown.onValueChanged.AddListener(ChangeResolution);
        }

        if (fullscreenToggle != null)
        {
            bool isFullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
            fullscreenToggle.SetIsOnWithoutNotify(isFullscreen);
            fullscreenToggle.onValueChanged.AddListener(ChangeFullscreen);
        }
    }

    private void BuildResolutionOptions()
    {
        availableResolutions.Clear();
        var uniqueSizes = new HashSet<string>();

        foreach (Resolution resolution in Screen.resolutions)
            AddResolution(resolution.width, resolution.height, uniqueSizes);

        // In windowed mode the current window size may not be a listed monitor mode.
        AddResolution(Screen.width, Screen.height, uniqueSizes);

        availableResolutions.Sort((a, b) =>
        {
            int widthCompare = a.x.CompareTo(b.x);
            return widthCompare != 0 ? widthCompare : a.y.CompareTo(b.y);
        });
    }

    private void AddResolution(int width, int height, HashSet<string> uniqueSizes)
    {
        string key = width + "x" + height;
        if (width > 0 && height > 0 && uniqueSizes.Add(key))
            availableResolutions.Add(new Vector2Int(width, height));
    }

    private int FindResolutionIndex(int width, int height)
    {
        for (int i = 0; i < availableResolutions.Count; i++)
        {
            if (availableResolutions[i].x == width && availableResolutions[i].y == height)
                return i;
        }

        return availableResolutions.Count > 0 ? availableResolutions.Count - 1 : 0;
    }

    private void ChangeResolution(int index)
    {
        if (index < 0 || index >= availableResolutions.Count)
            return;

        Vector2Int selected = availableResolutions[index];
        bool fullscreen = fullscreenToggle != null
            ? fullscreenToggle.isOn
            : PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;

        ApplyAndSave(selected.x, selected.y, fullscreen);
    }

    private void ChangeFullscreen(bool fullscreen)
    {
        int index = resolutionDropdown != null
            ? resolutionDropdown.value
            : FindResolutionIndex(Screen.width, Screen.height);

        if (index < 0 || index >= availableResolutions.Count)
            return;

        Vector2Int selected = availableResolutions[index];
        ApplyAndSave(selected.x, selected.y, fullscreen);
    }

    private void ApplyAndSave(int width, int height, bool fullscreen)
    {
        Screen.SetResolution(width, height, fullscreen);

        PlayerPrefs.SetInt(WidthKey, width);
        PlayerPrefs.SetInt(HeightKey, height);
        PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.RemoveListener(ChangeResolution);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveListener(ChangeFullscreen);
    }
}
