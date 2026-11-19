using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Automatically adds a player-count selector to CharacterSelect and
/// a player-count label to GameScene. No scene edits or button bindings needed.
/// Does not modify Leaderboard or create additional player characters.
/// </summary>
public sealed class CirksPlayerCount : MonoBehaviour
{
    public const string SaveKey = "Cirks_PlayerCount";
    public const int MinPlayers = 2;
    public const int MaxPlayers = 6;
    public const int DefaultPlayers = 2;

    private static readonly Color PanelColor = new Color(0.18f, 0.12f, 0.33f, 0.94f);
    private static readonly Color Green = new Color(0.59f, 0.89f, 0.27f, 1f);
    private static readonly Color Cream = new Color(1f, 0.96f, 0.77f, 1f);
    private static readonly Color White = Color.white;

    private TextMeshProUGUI countText;
    private Button minusButton;
    private Button plusButton;
    private int playerCount;
    private bool selectionMode;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneEvents()
    {
        // Avoid duplicate registration if Unity's domain reload is disabled.
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "CharacterSelect" && scene.name != "GameScene")
            return;

        Canvas canvas = null;
        string canvasName = scene.name == "CharacterSelect"
            ? "CharacterSelectCanvas"
            : "Canvas";

        GameObject canvasObject = GameObject.Find(canvasName);
        if (canvasObject != null)
            canvas = canvasObject.GetComponent<Canvas>();

        if (canvas == null)
        {
            Debug.LogWarning("CirksPlayerCount: Scene canvas not found: " + canvasName);
            return;
        }

        if (canvas.transform.Find("CirksPlayerCountUI") != null)
            return;

        GameObject host = new GameObject("CirksPlayerCountUI", typeof(RectTransform));
        host.layer = 5; // Unity UI layer
        host.transform.SetParent(canvas.transform, false);
        host.AddComponent<CirksPlayerCount>();
    }

    private void Awake()
    {
        selectionMode = SceneManager.GetActiveScene().name == "CharacterSelect";
        playerCount = Mathf.Clamp(PlayerPrefs.GetInt(SaveKey, DefaultPlayers), MinPlayers, MaxPlayers);

        if (selectionMode)
            BuildSelector();
        else
            BuildGameSceneLabel();

        Refresh();
    }

    private void BuildSelector()
    {
        RectTransform panel = GetComponent<RectTransform>();
        SetRect(panel, new Vector2(0.5f, 0.5f), new Vector2(570f, -145f), new Vector2(355f, 190f));
        AddPanel(panel.gameObject);

        TextMeshProUGUI title = CreateText("Title", panel, "NUMBER OF PLAYERS", 26f, Cream);
        SetRect(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 57f), new Vector2(335f, 48f));

        minusButton = CreateButton("MinusButton", panel, "−", new Vector2(-103f, -22f));
        minusButton.onClick.AddListener(() => ChangeCount(-1));

        countText = CreateText("Count", panel, "2", 49f, White);
        SetRect(countText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -22f), new Vector2(90f, 82f));

        plusButton = CreateButton("PlusButton", panel, "+", new Vector2(103f, -22f));
        plusButton.onClick.AddListener(() => ChangeCount(1));

        TextMeshProUGUI hint = CreateText("Hint", panel, "CHOOSE 2–6 PLAYERS", 17f, Cream);
        SetRect(hint.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -75f), new Vector2(335f, 32f));
    }

    private void BuildGameSceneLabel()
    {
        RectTransform banner = GetComponent<RectTransform>();
        SetRect(banner, new Vector2(0.5f, 1f), new Vector2(0f, -25f), new Vector2(380f, 80f), new Vector2(0.5f, 1f));
        AddPanel(banner.gameObject);

        countText = CreateText("PlayersCountText", banner, "PLAYERS: 2", 34f, White);
        SetRect(countText.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(360f, 70f));
    }

    private void ChangeCount(int delta)
    {
        playerCount = Mathf.Clamp(playerCount + delta, MinPlayers, MaxPlayers);
        PlayerPrefs.SetInt(SaveKey, playerCount);
        PlayerPrefs.Save();
        Refresh();
    }

    private void Refresh()
    {
        if (countText != null)
            countText.text = selectionMode ? playerCount.ToString() : "PLAYERS: " + playerCount;

        if (selectionMode)
        {
            if (minusButton != null) minusButton.interactable = playerCount > MinPlayers;
            if (plusButton != null) plusButton.interactable = playerCount < MaxPlayers;
        }
    }

    private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
    {
        SetRect(rect, anchor, position, size, new Vector2(0.5f, 0.5f));
    }

    private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size, Vector2 pivot)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
    }

    private static void AddPanel(GameObject target)
    {
        Image image = target.AddComponent<Image>();
        image.color = PanelColor;
        image.raycastTarget = true;

        Outline outline = target.AddComponent<Outline>();
        outline.effectColor = Green;
        outline.effectDistance = new Vector2(3f, -3f);
        outline.useGraphicAlpha = false;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, string content, float size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = size;
        text.fontStyle = FontStyles.Bold;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        text.enableAutoSizing = true;
        text.fontSizeMin = 14f;
        text.fontSizeMax = size;
        return text;
    }

    private static Button CreateButton(string name, Transform parent, string title, Vector2 pos)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        SetRect((RectTransform)go.transform, new Vector2(0.5f, 0.5f), pos, new Vector2(75f, 72f));

        Image image = go.AddComponent<Image>();
        image.color = Green;

        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.84f, 0.97f, 0.71f, 1f);
        colors.pressedColor = new Color(0.63f, 0.82f, 0.48f, 1f);
        colors.disabledColor = new Color(0.44f, 0.49f, 0.44f, 0.70f);
        button.colors = colors;

        TextMeshProUGUI label = CreateText("Label", go.transform, title, 52f, new Color(0.11f, 0.18f, 0.08f, 1f));
        SetRect(label.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(75f, 72f));
        return button;
    }
}
