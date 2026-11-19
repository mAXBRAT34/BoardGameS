using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows the player name and the original selected hero in GameScene.
/// Does not access leaderboard scores or modify the saved choice.
/// </summary>
[RequireComponent(typeof(Canvas))]
public sealed class GameSceneLoadData : MonoBehaviour
{
    [Header("Seven portraits, in CharacterSelect order")]
    [SerializeField] private Sprite[] heroPortraits = new Sprite[7];

    private const string PlayerNameKey = "Cirks_PlayerName";
    private const string CharacterIdKey = "Cirks_CharacterId";
    private const string CharacterNameKey = "Cirks_CharacterName";

    private GameObject dataPanel;
    private TextMeshProUGUI playerNameLabel;
    private TextMeshProUGUI heroNameLabel;
    private TextMeshProUGUI messageLabel;
    private Image heroImage;

    private static readonly Color JellyGreen = new Color(0.54f, 0.84f, 0.20f, 1f);
    private static readonly Color DeepPurple = new Color(0.16f, 0.11f, 0.29f, 0.97f);
    private static readonly Color Cream = new Color(1f, 0.96f, 0.85f, 1f);

    private void Awake()
    {
        Canvas canvas = GetComponent<Canvas>();
        RectTransform layer = CreateRect("Load Data UI", canvas.transform);
        StretchFull(layer);
        layer.SetAsLastSibling(); // Keep the button above the existing GameScene graphics.

        RectTransform loadButton = CreateRect("LoadDataButton", layer);
        AtTopRight(loadButton, new Vector2(-24f, -24f), new Vector2(235f, 70f));
        Image buttonImage = loadButton.gameObject.AddComponent<Image>();
        buttonImage.color = JellyGreen;
        AddOutline(loadButton.gameObject, Color.white, 3f);

        Button button = loadButton.gameObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.83f, 0.93f, 0.77f, 1f);
        colors.pressedColor = new Color(0.59f, 0.78f, 0.46f, 1f);
        colors.selectedColor = Color.white;
        colors.fadeDuration = 0.12f;
        button.colors = colors;
        button.onClick.AddListener(ShowSavedPlayer);

        RectTransform loadText = CreateRect("Label", loadButton);
        StretchFull(loadText);
        CreateText(loadText.gameObject, "LOAD DATA", 31f,
            new Color(0.10f, 0.17f, 0.08f), TextAlignmentOptions.Center);

        RectTransform frame = CreateRect("PlayerDataPanel", layer);
        AtTopRight(frame, new Vector2(-24f, -114f), new Vector2(390f, 490f));
        dataPanel = frame.gameObject;
        Image frameImage = frame.gameObject.AddComponent<Image>();
        frameImage.color = DeepPurple;
        AddOutline(frame.gameObject, Cream, 3f);

        RectTransform title = CreateRect("Title", frame);
        AtTopCenter(title, -22f, new Vector2(330f, 52f));
        CreateText(title.gameObject, "PLAYER DATA", 33f, Cream, TextAlignmentOptions.Center);

        RectTransform close = CreateRect("CloseButton", frame);
        SetAnchored(close, Vector2.one, Vector2.one,
            Vector2.one, new Vector2(-12f, -12f), new Vector2(44f, 44f));
        Image closeImage = close.gameObject.AddComponent<Image>();
        closeImage.color = new Color(0.90f, 0.34f, 0.39f, 1f);
        Button closeButton = close.gameObject.AddComponent<Button>();
        closeButton.targetGraphic = closeImage;
        closeButton.onClick.AddListener(() => dataPanel.SetActive(false));
        RectTransform closeText = CreateRect("X", close);
        StretchFull(closeText);
        CreateText(closeText.gameObject, "X", 25f, Color.white, TextAlignmentOptions.Center);

        RectTransform name = CreateRect("PlayerName", frame);
        AtTopCenter(name, -87f, new Vector2(345f, 56f));
        playerNameLabel = CreateText(name.gameObject, "", 30f, Color.white, TextAlignmentOptions.Center);

        RectTransform portrait = CreateRect("SelectedHero", frame);
        AtTopCenter(portrait, -163f, new Vector2(235f, 225f));
        heroImage = portrait.gameObject.AddComponent<Image>();
        heroImage.preserveAspect = true;
        heroImage.raycastTarget = false;

        RectTransform heroName = CreateRect("HeroName", frame);
        AtTopCenter(heroName, -394f, new Vector2(345f, 45f));
        heroNameLabel = CreateText(heroName.gameObject, "", 25f, Cream, TextAlignmentOptions.Center);

        RectTransform message = CreateRect("Message", frame);
        AtTopCenter(message, -445f, new Vector2(365f, 35f));
        messageLabel = CreateText(message.gameObject, "", 17f,
            new Color(0.77f, 0.86f, 0.95f), TextAlignmentOptions.Center);

        // Do not show player data before the LOAD DATA click.
        dataPanel.SetActive(false);
    }

    public void ShowSavedPlayer()
    {
        dataPanel.SetActive(true);

        string playerName = PlayerPrefs.GetString(PlayerNameKey, "").Trim();
        int characterId = PlayerPrefs.GetInt(CharacterIdKey, -1);
        string characterName = PlayerPrefs.GetString(CharacterNameKey, "");

        if (string.IsNullOrEmpty(playerName) || characterId < 0)
        {
            playerNameLabel.text = "NO SAVED PLAYER";
            heroNameLabel.text = "";
            messageLabel.text = "Choose a hero and enter a name first.";
            heroImage.sprite = null;
            heroImage.enabled = false;
            return;
        }

        playerNameLabel.text = playerName;
        heroNameLabel.text = characterName;
        messageLabel.text = "";

        if (heroPortraits != null && characterId < heroPortraits.Length &&
            heroPortraits[characterId] != null)
        {
            heroImage.sprite = heroPortraits[characterId];
            heroImage.enabled = true;
        }
        else
        {
            heroImage.sprite = null;
            heroImage.enabled = false;
            messageLabel.text = "Hero image not assigned.";
        }
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetAnchored(RectTransform rect, Vector2 min, Vector2 max,
        Vector2 pivot, Vector2 position, Vector2 size)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void AtTopRight(RectTransform rect, Vector2 position, Vector2 size)
    {
        SetAnchored(rect, Vector2.one, Vector2.one, Vector2.one, position, size);
    }

    private static void AtTopCenter(RectTransform rect, float top, Vector2 size)
    {
        Vector2 anchor = new Vector2(0.5f, 1f);
        SetAnchored(rect, anchor, anchor, anchor, new Vector2(0f, top), size);
    }

    private static TextMeshProUGUI CreateText(GameObject target, string value,
        float fontSize, Color color, TextAlignmentOptions alignment)
    {
        TextMeshProUGUI text = target.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.enableAutoSizing = true;
        text.fontSizeMin = 14f;
        text.fontSizeMax = fontSize;
        text.richText = false;
        return text;
    }

    private static void AddOutline(GameObject target, Color color, float size)
    {
        Outline outline = target.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = new Vector2(size, -size);
        outline.useGraphicAlpha = false;
    }
}
