using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GraphicsQualityController : MonoBehaviour
{
    [Header("Settings Scene UI (optional in MainMenu)")]
    [SerializeField] private TMP_Dropdown qualityDropdown;

    private const string SavedQualityKey = "Cirks_GraphicsQualityName";

    private void Awake()
    {
        ApplySavedQuality();
    }

    private void Start()
    {
        if (qualityDropdown == null)
            return; // MainMenu only needs to restore the saved quality.

        string[] availableLevels = QualitySettings.names;
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(availableLevels));

        if (availableLevels.Length == 0)
        {
            qualityDropdown.interactable = false;
            Debug.LogWarning("No graphics quality levels found in Project Settings > Quality.");
            return;
        }

        qualityDropdown.SetValueWithoutNotify(QualitySettings.GetQualityLevel());
        qualityDropdown.RefreshShownValue();
        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
    }

    private void ApplySavedQuality()
    {
        if (!PlayerPrefs.HasKey(SavedQualityKey))
            return;

        string savedName = PlayerPrefs.GetString(SavedQualityKey);
        int index = Array.IndexOf(QualitySettings.names, savedName);
        if (index >= 0 && index != QualitySettings.GetQualityLevel())
            QualitySettings.SetQualityLevel(index, true);
    }

    private void OnQualityChanged(int index)
    {
        string[] levels = QualitySettings.names;
        if (index < 0 || index >= levels.Length)
            return;

        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetString(SavedQualityKey, levels[index]);
        PlayerPrefs.Save();

        Debug.Log("Graphics quality changed to: " + levels[index]);
    }

    private void OnDestroy()
    {
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.RemoveListener(OnQualityChanged);
    }
}
