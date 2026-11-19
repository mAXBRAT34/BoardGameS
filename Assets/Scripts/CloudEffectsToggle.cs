
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class CloudEffectsToggle : MonoBehaviour
{
    [Header("Toggle Appearance")]
    [SerializeField] private Image backgroundImage;

    [Header("Optional Sprites")]
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;

    [Header("Colors")]
    [SerializeField]
    private Color onColor =
        new Color(0.5f, 0.85f, 0.2f, 1f);

    [SerializeField]
    private Color offColor =
        new Color(0.5f, 0.5f, 0.5f, 1f);

    private Toggle cloudToggle;

    private const string SaveKey = "Cirks_CloudEffects";

    private void Awake()
    {
        cloudToggle = GetComponent<Toggle>();

        bool enabled = PlayerPrefs.GetInt(SaveKey, 1) == 1;

        cloudToggle.SetIsOnWithoutNotify(enabled);

        UpdateAppearance(enabled);

        cloudToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool enabled)
    {
        PlayerPrefs.SetInt(SaveKey, enabled ? 1 : 0);
        PlayerPrefs.Save();

        UpdateAppearance(enabled);
    }

    private void UpdateAppearance(bool enabled)
    {
        if (backgroundImage == null)
            return;

        Sprite selectedSprite = enabled ? onSprite : offSprite;

        if (selectedSprite != null)
        {
            backgroundImage.sprite = selectedSprite;
            backgroundImage.color = Color.white;
        }
        else
        {
            backgroundImage.color = enabled ? onColor : offColor;
        }
    }

    private void OnDestroy()
    {
        if (cloudToggle != null)
            cloudToggle.onValueChanged.RemoveListener(OnToggleChanged);
    }
}
