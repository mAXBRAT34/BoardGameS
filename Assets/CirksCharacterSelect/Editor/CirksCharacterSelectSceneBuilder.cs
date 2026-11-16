#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Builds a complete ready-to-play CharacterSelect.unity scene from the included
/// user-supplied character animations and 2D block sprites.
/// Click Tools > Cirks > Create Character Selection Scene (7 Characters).
/// No changes are made to the user's existing MainMenu or GameScene assets.
/// </summary>
public static class CirksCharacterSelectSceneBuilder
{
    private const string Root = "Assets/CirksCharacterSelect/";
    private const string ScenePath = Root + "Scenes/CharacterSelect.unity";

    private static readonly string[] FolderNames =
    {
        "Reaper", "FallenAngel_A", "FallenAngel_B", "Golem", "Orc", "Warrior_A", "Warrior_B"
    };

    private static readonly string[] DisplayNames =
    {
        "REAPER", "FALLEN ANGEL", "DARK ANGEL", "GOLEM", "ORC", "WARRIOR", "ARMORED WARRIOR"
    };

    [MenuItem("Tools/Cirks/Create Character Selection Scene (7 Characters)")]
    public static void CreateScene()
    {
        if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog(
            "CharacterSelect already exists",
            "Replace the existing generated CharacterSelect scene? Your MainMenu and GameScene will remain untouched.",
            "Rebuild", "Cancel"))
        {
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EnsureFolder(Root + "Scenes");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera();
        CreateEventSystem();

        GameObject canvasObject = new GameObject(
            "CharacterSelectCanvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        CanvasScaler canvasScaler = canvasObject.GetComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;

        RectTransform root = canvasObject.GetComponent<RectTransform>();

        GameObject backdrop = CreateImage("SunnyBackground", root,
            GetSprite("UI/SunnyBackground.png"), 0f, 0f, 1920f, 1080f, false);
        Stretch(backdrop.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);

        CreateLabel("Select Your Character", root, "CHOOSE YOUR HERO", 0, 430,
            930, 100, 75, new Color(0.29f, 0.26f, 0.14f), true);

        // Player name entry: fully functional TMPro input field.
        TMP_InputField nameInput = CreateNameInput(root);

        Button closeButton = CreateButton("CloseButton", root, "UI/CloseButtonX.png",
            845f, 426f, 100f, 100f);

        // Central isometric platform: block images from 2D_Blocks_DevilsWorkShop_v04.
        CreateImage("GrassStonePlatform", root,
            GetSprite("Stage/GrassStoneBlock_Original.png"), -15f, -185f, 520f, 600f, false);
        CreateImage("WoodenCrate", root,
            GetSprite("Stage/WoodenCrate_Original.png"), 263f, -244f, 250f, 300f, false);

        CreateImage("CharacterShadow", root,
            GetSprite("UI/CharacterShadow.png"), -10f, -230f, 280f, 47f, false);

        Image characterImage = CreateImage("AnimatedCharacter", root,
            GetSprite("CharacterAnimations/Reaper/Idle_000.png"),
            -5f, -15f, 345f, 450f, false).GetComponent<Image>();
        characterImage.preserveAspect = true;
        characterImage.raycastTarget = false;

        Button leftArrow = CreateButton("PreviousCharacterButton", root,
            "UI/ArrowLeft.png", -610f, -38f, 205f, 130f);
        Button rightArrow = CreateButton("NextCharacterButton", root,
            "UI/ArrowRight.png", 610f, -38f, 205f, 130f);

        TMP_Text characterName = CreateLabel("CharacterName", root, "REAPER", 0f, -394f,
            500f, 55f, 39f, new Color(.21f, .25f, .15f), true);
        TMP_Text characterCounter = CreateLabel("CharacterCounter", root, "01 / 07", 0f, -450f,
            260f, 52f, 30f, new Color(.28f, .33f, .15f), true);

        Button confirmButton = CreateButton("ConfirmButton", root,
            "UI/ConfirmButtonCheck.png", 730f, -335f, 172f, 172f);
        CreateLabel("ConfirmCaption", root, "PLAY", 730f, -452f,
            200f, 50f, 32f, new Color(.20f, .34f, .08f), true);
        TMP_Text status = CreateLabel("HelpLabel", root,
            "Choose a character and enter your name", -570f, -470f,
            760f, 50f, 26f, new Color(.30f, .29f, .22f), false);
        status.alignment = TextAlignmentOptions.Left;

        GameObject manager = new GameObject("CharacterSelectionManager", typeof(CirksCharacterSelection));
        CirksCharacterSelection selection = manager.GetComponent<CirksCharacterSelection>();
        selection.playerNameInput = nameInput;
        selection.characterImage = characterImage;
        selection.characterTransform = characterImage.rectTransform;
        selection.characterNameLabel = characterName;
        selection.characterIndexLabel = characterCounter;
        selection.statusLabel = status;
        selection.previousButton = leftArrow;
        selection.nextButton = rightArrow;
        selection.confirmButton = confirmButton;
        selection.closeButton = closeButton;
        selection.gameSceneName = "GameScene";
        selection.mainMenuSceneName = "MainMenu";
        selection.idleFrameRate = 12f;
        selection.bobAmount = 7f;
        selection.swayDegrees = 2.5f;
        selection.breathingAmount = .018f;

        selection.characters = new CirksCharacterSelection.CharacterDefinition[FolderNames.Length];
        int frameCount = 0;
        for (int i = 0; i < FolderNames.Length; i++)
        {
            string folder = Root + "CharacterAnimations/" + FolderNames[i];
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folder });
            List<string> filePaths = new List<string>();
            foreach (string guid in guids)
            {
                string filePath = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileName(filePath).StartsWith("Idle_", StringComparison.Ordinal))
                    filePaths.Add(filePath);
            }
            filePaths.Sort(StringComparer.Ordinal);
            Sprite[] frames = new Sprite[filePaths.Count];
            for (int f = 0; f < filePaths.Count; f++)
                frames[f] = AssetDatabase.LoadAssetAtPath<Sprite>(filePaths[f]);
            frameCount += frames.Length;

            selection.characters[i] = new CirksCharacterSelection.CharacterDefinition
            {
                displayName = DisplayNames[i],
                idleFrames = frames
            };

            if (frames.Length == 0)
                Debug.LogWarning("Cirks: No idle sprites imported for " + FolderNames[i]);
        }

        EditorUtility.SetDirty(selection);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene, ScenePath))
        {
            Debug.LogError("Cirks: Unable to save generated scene at " + ScenePath);
            return;
        }

        // Preserve existing build scenes. Add our scene and automatically include
        // previously existing MainMenu/GameScene files when Unity can locate them.
        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        TryAddExistingScene(buildScenes, "MainMenu");
        AddBuildScene(buildScenes, ScenePath);
        TryAddExistingScene(buildScenes, "GameScene");
        EditorBuildSettings.scenes = buildScenes.ToArray();

        Selection.activeGameObject = manager;
        SceneView.RepaintAll();
        Debug.Log("Cirks: CharacterSelect.unity was created with 7 ORIGINAL characters and " + frameCount + " idle frames. Saved to " + ScenePath);
        EditorUtility.DisplayDialog("Character Selection created",
            "Ready! Scene: " + ScenePath + "\n\n7 original characters, blinking animations, name entry, arrow controls, confirmation and Back button.\n\nFor navigation, also enable MainMenu and GameScene in Build Profiles > Scene List.",
            "Open scene");
    }

    private static void AddBuildScene(List<EditorBuildSettingsScene> buildScenes, string path)
    {
        foreach (EditorBuildSettingsScene entry in buildScenes)
            if (entry.path == path) return;
        buildScenes.Add(new EditorBuildSettingsScene(path, true));
    }

    private static void TryAddExistingScene(List<EditorBuildSettingsScene> buildScenes, string sceneName)
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Scene"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path).Equals(sceneName, StringComparison.OrdinalIgnoreCase))
            {
                AddBuildScene(buildScenes, path);
                return;
            }
        }
    }

    private static void EnsureFolder(string fullPath)
    {
        string[] parts = fullPath.Trim('/').Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        Camera camera = cameraObject.GetComponent<Camera>();
        cameraObject.tag = "MainCamera";
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.83f, .66f, .19f);
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
    }

    private static void CreateEventSystem()
    {
        GameObject objectWithEventSystem = new GameObject("EventSystem", typeof(EventSystem));
        // Use the new Input System module if installed. Otherwise use built-in input.
        Type newInputModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (newInputModuleType != null && typeof(BaseInputModule).IsAssignableFrom(newInputModuleType))
            objectWithEventSystem.AddComponent(newInputModuleType);
        else
            objectWithEventSystem.AddComponent<StandaloneInputModule>();
    }

    private static Sprite GetSprite(string relativePath)
    {
        string path = Root + relativePath;
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null) Debug.LogWarning("Cirks missing Sprite: " + path);
        return sprite;
    }

    private static RectTransform MakeRect(string name, Transform parent,
        float x, float y, float width, float height)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(.5f, .5f);
        rt.anchorMax = new Vector2(.5f, .5f);
        rt.pivot = new Vector2(.5f, .5f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(x, y);
        return rt;
    }

    private static void Stretch(RectTransform rt, float left, float bottom, float right, float top)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(.5f, .5f);
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    private static GameObject CreateImage(string name, RectTransform parent, Sprite sprite,
        float x, float y, float width, float height, bool raycastTarget)
    {
        RectTransform rt = MakeRect(name, parent, x, y, width, height);
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Simple;
        img.preserveAspect = false;
        img.raycastTarget = raycastTarget;
        img.color = Color.white;
        return rt.gameObject;
    }

    private static Button CreateButton(string name, RectTransform parent,
        string spritePath, float x, float y, float width, float height)
    {
        GameObject go = CreateImage(name, parent, GetSprite(spritePath), x, y, width, height, true);
        Image image = go.GetComponent<Image>();
        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = ColorBlock.defaultColorBlock;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(.89f, .96f, .80f, 1f);
        colors.pressedColor = new Color(.68f, .83f, .61f, 1f);
        colors.selectedColor = Color.white;
        colors.fadeDuration = .1f;
        button.colors = colors;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        return button;
    }

    private static TMP_Text CreateLabel(string name, RectTransform parent,
        string value, float x, float y, float width, float height,
        float fontSize, Color color, bool bold)
    {
        RectTransform rect = MakeRect(name, parent, x, y, width, height);
        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        TMP_FontAsset fallbackFont = TMP_Settings.defaultFontAsset;
        if (fallbackFont != null) label.font = fallbackFont;
        label.text = value;
        label.fontSize = fontSize;
        label.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        label.alignment = TextAlignmentOptions.Center;
        label.color = color;
        label.raycastTarget = false;
        label.enableAutoSizing = true;
        label.fontSizeMin = Mathf.Max(12, fontSize * .6f);
        label.fontSizeMax = fontSize;
        label.overflowMode = TextOverflowModes.Ellipsis;
        return label;
    }

    private static TMP_InputField CreateNameInput(RectTransform root)
    {
        GameObject field = CreateImage("PlayerNameInputField", root,
            GetSprite("UI/InputField.png"), -615f, 421f, 540f, 110f, true);
        TMP_InputField input = field.AddComponent<TMP_InputField>();
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.characterLimit = 18;
        input.contentType = TMP_InputField.ContentType.Standard;
        input.caretColor = new Color(.18f, .29f, .08f);
        input.customCaretColor = true;
        input.selectionColor = new Color(.41f, .67f, .21f, .5f);

        RectTransform viewport = MakeRect("Text Area", field.transform, 0, 0, 100, 50);
        Stretch(viewport, 36, 16, 36, 14);

        TMP_Text placeholder = CreateLabel("Placeholder", viewport,
            "ENTER NAME...", 0, 0, 200, 60, 37,
            new Color(.25f, .37f, .14f, .63f), true);
        placeholder.fontStyle = FontStyles.Italic;
        placeholder.alignment = TextAlignmentOptions.Left;
        Stretch(placeholder.rectTransform, 0, 0, 0, 0);

        TMP_Text typedText = CreateLabel("Text", viewport,
            "", 0, 0, 200, 60, 39,
            new Color(.15f, .28f, .05f), true);
        typedText.alignment = TextAlignmentOptions.Left;
        Stretch(typedText.rectTransform, 0, 0, 0, 0);

        input.textViewport = viewport;
        input.textComponent = typedText;
        input.placeholder = placeholder;
        return input;
    }
}
#endif
