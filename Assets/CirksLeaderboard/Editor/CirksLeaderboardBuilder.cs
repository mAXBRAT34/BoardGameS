#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// One-click scene builder for the user's BoardGameS project (Unity 6000.0.49f1).
/// Creates Assets/Scenes/Leaderboard.unity and a TOP 10 button in MainMenu.
/// Uses existing sprites, scene fade, and UI button effects where available.
/// </summary>
public static class CirksLeaderboardBuilder
{
    private const string ScenePath = "Assets/Scenes/Leaderboard.unity";
    private const string MainMenuPath = "Assets/Scenes/MainMenu.unity";

    private static readonly Color White = Color.white;
    private static readonly Color Pale = new Color(0.95f, 0.94f, 1f, 1f);
    private static readonly Color Lime = new Color(0.61f, 0.93f, 0.26f, 1f);
    private static readonly Color DarkText = new Color(0.14f, 0.10f, 0.25f, 1f);

    [MenuItem("Tools/Cirks/Create Leaderboard Scene + MainMenu Button")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        if (System.IO.File.Exists(ScenePath) &&
            !EditorUtility.DisplayDialog("Leaderboard already exists",
                "Overwrite Assets/Scenes/Leaderboard.unity? Existing leaderboard scene layout will be replaced. Other scenes and stored scores are not deleted.",
                "Overwrite", "Cancel"))
            return;

        Scene leaderboard = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        leaderboard.name = "Leaderboard";
        CreateCamera();

        GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem));
        var inputModule = eventSystem.AddComponent<InputSystemUIInputModule>();
        inputModule.AssignDefaultActions();

        Transform root = canvasObject.transform;
        Image bg = CreateImage(root, "CosmicNightBackground", null, new Color(0.095f, 0.055f, 0.21f, 1f),
            Vector2.zero, new Vector2(1920, 1080));
        Stretch(bg.rectTransform);
        bg.raycastTarget = false;

        // Subtle bright spots and candy-colored space particles.
        AddDecoration(root, new Vector2(-700, 330), 300, new Color(0.36f, 0.20f, 0.60f, 0.22f));
        AddDecoration(root, new Vector2(720, -320), 390, new Color(0.18f, 0.65f, 0.79f, 0.14f));
        AddDecoration(root, new Vector2(730, 335), 160, new Color(0.49f, 0.84f, 0.21f, 0.11f));
        for (int i = 0; i < 20; i++)
        {
            float x = (i * 431 % 1840) - 920;
            float y = (i * 293 % 990) - 495;
            int size = i % 5 == 0 ? 9 : 4;
            Image star = CreateImage(root, "Star_" + i, null,
                new Color(0.89f, 0.88f, 1f, 0.40f), new Vector2(x, y), new Vector2(size, size));
            star.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
            star.raycastTarget = false;
        }

        // Reuse the game's own colorful frame when present.
        Sprite panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Photos/Panel_Settings.png");
        Image frame = CreateImage(root, "LeaderboardPanel", panelSprite,
            panelSprite != null ? White : new Color(0.25f, 0.17f, 0.42f, 1f),
            new Vector2(0, -8), new Vector2(1310, 910));
        frame.raycastTarget = false;

        // Dark readable layer, keeping the original decorative frame visible.
        Image inner = CreateImage(root, "LeaderboardInner", null,
            new Color(0.105f, 0.065f, 0.22f, panelSprite == null ? 0.94f : 0.88f),
            new Vector2(0, -13), new Vector2(1130, 745));
        inner.raycastTarget = false;

        CreateText(root, "Title", "LEADERBOARD", new Vector2(0, 390),
            new Vector2(1000, 105), 76, Pale, TextAlignmentOptions.Center, FontStyles.Bold);
        CreateText(root, "Subtitle", "TOP 10  •  JELLY CHAMPIONS", new Vector2(0, 322),
            new Vector2(960, 52), 28, Lime, TextAlignmentOptions.Center, FontStyles.Bold);

        CreateText(root, "RankColumn", "RANK", new Vector2(-437, 253),
            new Vector2(130, 48), 27, Lime, TextAlignmentOptions.Center, FontStyles.Bold);
        CreateText(root, "PlayerColumn", "PLAYER", new Vector2(-185, 253),
            new Vector2(365, 48), 27, Lime, TextAlignmentOptions.Left, FontStyles.Bold);
        CreateText(root, "ScoreColumn", "SCORE", new Vector2(410, 253),
            new Vector2(240, 48), 27, Lime, TextAlignmentOptions.Right, FontStyles.Bold);

        GameObject boardController = new GameObject("LeaderboardManager", typeof(CirksLeaderboardScreen));
        CirksLeaderboardScreen leaderboardScreen = boardController.GetComponent<CirksLeaderboardScreen>();
        List<CirksLeaderboardRow> entries = new List<CirksLeaderboardRow>();

        for (int i = 0; i < 10; i++)
        {
            float y = 195f - i * 51f;
            GameObject row = new GameObject("RankRow_" + (i + 1).ToString("00"),
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CirksLeaderboardRow));
            row.transform.SetParent(root, false);
            RectTransform rowRect = row.GetComponent<RectTransform>();
            Place(rowRect, new Vector2(0, y), new Vector2(1060, 46));

            Image background = row.GetComponent<Image>();
            background.color = i == 0 ? new Color(0.31f, 0.35f, 0.18f, 0.85f)
                : i % 2 == 0 ? new Color(0.33f, 0.27f, 0.53f, 0.68f)
                : new Color(0.22f, 0.17f, 0.40f, 0.80f);
            background.raycastTarget = false;

            CirksLeaderboardRow view = row.GetComponent<CirksLeaderboardRow>();
            view.rankLabel = CreateText(row.transform, "Rank", (i + 1).ToString("00"),
                new Vector2(-435, 0), new Vector2(150, 42), 26,
                i < 3 ? new Color(1f, 0.85f, 0.41f) : Pale,
                TextAlignmentOptions.Center, FontStyles.Bold);
            view.playerLabel = CreateText(row.transform, "Player", i == 0 ? "BE THE FIRST!" : "—",
                new Vector2(-165, 0), new Vector2(420, 42), 25, Pale,
                TextAlignmentOptions.Left, FontStyles.Normal);
            view.scoreLabel = CreateText(row.transform, "Score", "—",
                new Vector2(406, 0), new Vector2(225, 42), 26, Lime,
                TextAlignmentOptions.Right, FontStyles.Bold);
            entries.Add(view);
        }
        leaderboardScreen.rows = entries.ToArray();
        leaderboardScreen.footerLabel = CreateText(root, "Footer",
            "NO SCORES YET  •  FINISH A MATCH TO GET ON THE BOARD",
            new Vector2(0, -347), new Vector2(1060, 48), 21, Pale,
            TextAlignmentOptions.Center, FontStyles.Normal);

        GameObject back = CreateStyledButton(root, "BackButton", "BACK",
            new Vector2(0, -465), new Vector2(300, 85));
        ConfigureFade(back, "MainMenu");
        back.AddComponent<ButtonFloatAnimation>();

        // Copy the existing cursor configuration into the new scene and wire back button FX.
        EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Additive);
        Scene main = SceneManager.GetSceneByPath(MainMenuPath);
        CursorScript cursorSource = FindComponentInScene<CursorScript>(main);
        if (cursorSource != null)
        {
            GameObject cursorObj = new GameObject("CursorManager");
            SceneManager.MoveGameObjectToScene(cursorObj, leaderboard);
            CursorScript cursorCopy = cursorObj.AddComponent<CursorScript>();
            EditorUtility.CopySerialized(cursorSource, cursorCopy);
            WireCursor(back, cursorCopy);
        }
        AddButtonSound(back);
        EditorSceneManager.CloseScene(main, true);

        // Save scene and register it in Unity's build scene list.
        EditorSceneManager.SetActiveScene(leaderboard);
        EditorSceneManager.SaveScene(leaderboard, ScenePath);
        EnsureInBuildSettings(ScenePath);

        // Add one styled button to MainMenu; don't modify any of the original four buttons.
        Scene mainMenu = EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Single);
        Canvas mainCanvas = FindComponentInScene<Canvas>(mainMenu);
        if (mainCanvas == null)
            throw new InvalidOperationException("MainMenu has no Canvas. Leaderboard scene was created, but menu button cannot be placed.");

        if (mainMenu.GetRootGameObjects()
            .SelectMany(r => r.GetComponentsInChildren<Transform>(true))
            .All(t => t.name != "LeaderboardButton"))
        {
            GameObject top10 = CreateStyledButton(mainCanvas.transform, "LeaderboardButton", "TOP 10",
                new Vector2(550, -275), new Vector2(290, 112));
            ConfigureFade(top10, "Leaderboard");
            top10.AddComponent<ButtonFloatAnimation>();
            CursorScript mainCursor = FindComponentInScene<CursorScript>(mainMenu);
            if (mainCursor != null) WireCursor(top10, mainCursor);
            AddButtonSound(top10);
            Selection.activeGameObject = top10;
        }

        EditorSceneManager.SaveScene(mainMenu);
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        EditorUtility.DisplayDialog("Cirks Leaderboard is ready",
            "Created Assets/Scenes/Leaderboard.unity, added TOP 10 to MainMenu, and registered the new scene in Build Settings.\n\nPlay from MainMenu to test navigation. Scores start empty until your GameScene calls SubmitCurrentPlayerScore(finalScore).",
            "OK");
    }

    private static T FindComponentInScene<T>(Scene scene) where T : Component
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            T result = root.GetComponentInChildren<T>(true);
            if (result != null) return result;
        }
        return null;
    }

    private static void CreateCamera()
    {
        GameObject camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(0, 0, -10);
        Camera cameraComponent = camera.GetComponent<Camera>();
        cameraComponent.orthographic = true;
        cameraComponent.clearFlags = CameraClearFlags.SolidColor;
        cameraComponent.backgroundColor = new Color(0.09f, 0.05f, 0.19f, 1f);
    }

    private static void AddDecoration(Transform root, Vector2 position, float size, Color color)
    {
        Image image = CreateImage(root, "Glow", null, color, position, new Vector2(size, size));
        Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (uiSprite != null) image.sprite = uiSprite;
        image.raycastTarget = false;
    }

    private static void EnsureInBuildSettings(string newScene)
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        if (!scenes.Any(s => s.path == newScene))
        {
            scenes.Add(new EditorBuildSettingsScene(newScene, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }

    private static GameObject CreateStyledButton(Transform parent, string name, string label,
        Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer),
            typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        Place((RectTransform)go.transform, position, size);

        Image image = go.GetComponent<Image>();
        image.sprite = LoadGuiSprite("GUI_11");
        if (image.sprite != null && image.sprite.border.sqrMagnitude > 0)
            image.type = Image.Type.Sliced;
        image.color = image.sprite == null ? new Color(0.49f, 0.80f, 0.20f, 1f) : White;

        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = White;
        colors.highlightedColor = new Color(0.88f, 0.99f, 0.82f, 1f);
        colors.pressedColor = new Color(0.60f, 0.82f, 0.52f, 1f);
        colors.selectedColor = White;
        colors.fadeDuration = 0.12f;
        button.colors = colors;
        button.navigation = new Navigation { mode = Navigation.Mode.None };

        TMP_Text caption = CreateText(go.transform, "Text (TMP)", label,
            new Vector2(0, 0), size - new Vector2(24, 18), 37, White,
            TextAlignmentOptions.Center, FontStyles.Bold);
        caption.outlineWidth = 0.18f;
        caption.outlineColor = DarkText;
        caption.raycastTarget = false;
        return go;
    }

    private static Sprite LoadGuiSprite(string name)
    {
        return AssetDatabase.LoadAllAssetsAtPath("Assets/2D Casual UI/Sprite/GUI.png")
            .OfType<Sprite>().FirstOrDefault(s => s.name == name);
    }

    private static void ConfigureFade(GameObject button, string targetScene)
    {
        SceneFadeTransition fade = button.AddComponent<SceneFadeTransition>();
        SerializedObject serialized = new SerializedObject(fade);
        serialized.FindProperty("targetScene").stringValue = targetScene;
        serialized.FindProperty("fadeDuration").floatValue = 1f;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void WireCursor(GameObject button, CursorScript cursor)
    {
        ButtonCursorEffect effect = button.AddComponent<ButtonCursorEffect>();
        SerializedObject serialized = new SerializedObject(effect);
        serialized.FindProperty("cursorManager").objectReferenceValue = cursor;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AddButtonSound(GameObject button)
    {
        AudioSource source = button.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        AudioMixer mixer = AssetDatabase.FindAssets("t:AudioMixer")
            .Select(guid => AssetDatabase.LoadAssetAtPath<AudioMixer>(AssetDatabase.GUIDToAssetPath(guid)))
            .FirstOrDefault(x => x != null && x.name == "GameAudioMixer");
        if (mixer != null)
        {
            var groups = mixer.FindMatchingGroups("SFX");
            if (groups.Length > 0) source.outputAudioMixerGroup = groups[0];
        }

        JellyButtonSound sound = button.AddComponent<JellyButtonSound>();
        SerializedObject serialized = new SerializedObject(sound);
        serialized.FindProperty("hoverSound").objectReferenceValue = FindAudioClip("pop9");
        serialized.FindProperty("pressSound").objectReferenceValue = FindAudioClip("slime-squish");
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static AudioClip FindAudioClip(string partOfName)
    {
        foreach (string guid in AssetDatabase.FindAssets("t:AudioClip"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.IndexOf(partOfName, StringComparison.OrdinalIgnoreCase) >= 0)
                return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        return null;
    }

    private static Image CreateImage(Transform parent, string name, Sprite sprite, Color color,
        Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        Place((RectTransform)go.transform, position, size);
        return image;
    }

    private static TMP_Text CreateText(Transform parent, string name, string value,
        Vector2 position, Vector2 size, float fontSize, Color color,
        TextAlignmentOptions alignment, FontStyles style)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        Place((RectTransform)go.transform, position, size);
        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) text.font = TMP_Settings.defaultFontAsset;
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = alignment;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        return text;
    }

    private static void Place(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    // Test without finished gameplay: Tools > Cirks > Add Test Leaderboard Score
    [MenuItem("Tools/Cirks/Add Test Leaderboard Score")]
    public static void AddTestScore()
    {
        string name = "Test Player";
        int current = CirksLeaderboardStore.GetTopScores().FirstOrDefault(x => x.playerName == name)?.score ?? 0;
        CirksLeaderboardStore.SubmitScore(name, current + 100);
        Debug.Log("Added a clearly labeled test score. Start Play Mode in Leaderboard to see it.");
    }
}
#endif
