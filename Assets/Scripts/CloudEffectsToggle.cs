using UnityEngine;
using UnityEngine.UI;

// Attach to the CloudEffectsToggle UI object in the Settings scene.
[RequireComponent(typeof(Toggle))]
public class CloudEffectsToggle : MonoBehaviour
{
    private const string SaveKey = "Cirks_CloudEffects";
    private Toggle cloudToggle;

    private void Awake()
    {
        cloudToggle = GetComponent<Toggle>();

        // Clouds are animated by default, until a user turns them off.
        bool animationEnabled = PlayerPrefs.GetInt(SaveKey, 1) == 1;
        cloudToggle.SetIsOnWithoutNotify(animationEnabled);

        cloudToggle.onValueChanged.AddListener(SaveCloudPreference);
    }

    private void SaveCloudPreference(bool enabled)
    {
        PlayerPrefs.SetInt(SaveKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (cloudToggle != null)
            cloudToggle.onValueChanged.RemoveListener(SaveCloudPreference);
    }
}
