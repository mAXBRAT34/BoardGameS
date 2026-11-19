using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Character selection with animated hero preview and required player name.
/// The existing PLAY button saves the chosen hero/name and only then fades to GameScene.
/// </summary>
public class CirksCharacterSelection : MonoBehaviour
{
    [Serializable]
    public class CharacterDefinition
    {
        public string displayName;
        public Sprite[] idleFrames;
    }

    [Header("Original character animations (7 variants)")]
    public CharacterDefinition[] characters;

    [Header("UI references")]
    public TMP_InputField playerNameInput;
    public Image characterImage;
    public RectTransform characterTransform;
    public TMP_Text characterNameLabel;
    public TMP_Text characterIndexLabel;
    public TMP_Text statusLabel;
    public Button previousButton;
    public Button nextButton;
    public Button confirmButton;
    public Button closeButton;

    [Header("Animation")]
    [Range(2f, 30f)] public float idleFrameRate = 12f;
    [Range(0f, 16f)] public float bobAmount = 7f;
    [Range(0f, 12f)] public float swayDegrees = 2.5f;
    [Range(0f, 0.1f)] public float breathingAmount = 0.018f;

    [Header("Scenes")]
    public string gameSceneName = "GameScene";
    public string mainMenuSceneName = "MainMenu";

    public const string PlayerNameKey = "Cirks_PlayerName";
    public const string SelectedCharacterKey = "Cirks_CharacterId";
    public const string SelectedCharacterNameKey = "Cirks_CharacterName";

    private int selectedIndex;
    private float idleTimer;
    private Vector2 basePosition;
    private Vector3 baseScale;
    private Quaternion baseRotation;
    private SceneFadeTransition playFade;

    private void Awake()
    {
        if (characterTransform == null && characterImage != null)
            characterTransform = characterImage.rectTransform;

        if (characterTransform != null)
        {
            basePosition = characterTransform.anchoredPosition;
            baseScale = characterTransform.localScale;
            baseRotation = characterTransform.localRotation;
        }

        if (confirmButton != null)
            playFade = confirmButton.GetComponent<SceneFadeTransition>();

        if (previousButton != null) previousButton.onClick.AddListener(PreviousCharacter);
        if (nextButton != null) nextButton.onClick.AddListener(NextCharacter);
        if (confirmButton != null) confirmButton.onClick.AddListener(ConfirmSelection);
        if (closeButton != null) closeButton.onClick.AddListener(BackToMainMenu);
    }

    private void Start()
    {
        // SceneFadeTransition registers its own button listener in Awake.
        // All Awake calls finish before any Start: remove the automatic fade listener
        // so that a click can transition ONLY after ConfirmSelection validates the name.
        if (confirmButton != null && playFade != null)
            confirmButton.onClick.RemoveListener(playFade.StartTransition);

        if (playerNameInput != null)
        {
            playerNameInput.characterLimit = 18;
            // The name field must be empty on every entry to CharacterSelect.
            // Previously saved names are still available to GameScene -> LOAD DATA.
            playerNameInput.SetTextWithoutNotify(string.Empty);
        }

        selectedIndex = Mathf.Clamp(
            PlayerPrefs.GetInt(SelectedCharacterKey, 0),
            0,
            Mathf.Max(0, characters == null ? 0 : characters.Length - 1));

        ShowCharacter();
        ShowStatus("Choose a character and enter your name");
    }

    private void Update()
    {
        if (characters == null || characters.Length == 0 || characterImage == null)
            return;

        CharacterDefinition character = characters[selectedIndex];
        if (character == null || character.idleFrames == null || character.idleFrames.Length == 0)
            return;

        idleTimer += Time.unscaledDeltaTime;
        int frame = Mathf.FloorToInt(idleTimer * idleFrameRate) % character.idleFrames.Length;
        if (character.idleFrames[frame] != null)
            characterImage.sprite = character.idleFrames[frame];

        if (characterTransform != null)
        {
            float t = Time.unscaledTime;
            float bob = Mathf.Sin(t * 2f + 0.6f) * bobAmount;
            float tilt = Mathf.Sin(t * 1.2f) * swayDegrees;
            float breath = 1f + Mathf.Sin(t * 1.6f) * breathingAmount;
            characterTransform.anchoredPosition = basePosition + new Vector2(0f, bob);
            characterTransform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, tilt);
            characterTransform.localScale = new Vector3(baseScale.x * breath, baseScale.y * breath, baseScale.z);
        }
    }

    public void NextCharacter() { ChangeCharacter(1); }
    public void PreviousCharacter() { ChangeCharacter(-1); }

    private void ChangeCharacter(int direction)
    {
        if (characters == null || characters.Length == 0) return;
        selectedIndex = (selectedIndex + direction + characters.Length) % characters.Length;
        ShowCharacter();
        ShowStatus("Choose a character and enter your name");
    }

    private void ShowCharacter()
    {
        idleTimer = 0f;
        if (characters == null || characters.Length == 0) return;

        CharacterDefinition current = characters[selectedIndex];
        if (characterImage != null && current != null && current.idleFrames != null && current.idleFrames.Length > 0)
            characterImage.sprite = current.idleFrames[0];

        if (characterNameLabel != null)
            characterNameLabel.text = current != null ? current.displayName : "Hero";

        if (characterIndexLabel != null)
            characterIndexLabel.text = (selectedIndex + 1).ToString("00") + " / " + characters.Length.ToString("00");

        if (characterTransform != null)
        {
            characterTransform.anchoredPosition = basePosition;
            characterTransform.localScale = baseScale;
            characterTransform.localRotation = baseRotation;
        }
    }

    public void ConfirmSelection()
    {
        string playerName = playerNameInput != null ? playerNameInput.text.Trim() : "";

        // Blank or whitespace-only input cannot start the game.
        if (playerName.Length < 3)
        {
            ShowStatus("Name must contain at least 3 characters!");
            if (playerNameInput != null)
            {
                playerNameInput.Select();
                playerNameInput.ActivateInputField();
            }
            return;
        }

        if (characters == null || characters.Length == 0) return;

        if (!Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            ShowStatus("Add GameScene to Build Profiles > Scene List.");
            Debug.LogWarning("Cirks: GameScene is not in the build scene list.");
            return;
        }

        // Preserve the existing GameScene LOAD DATA integration.
        PlayerPrefs.SetString(PlayerNameKey, playerName);
        PlayerPrefs.SetInt(SelectedCharacterKey, selectedIndex);
        PlayerPrefs.SetString(SelectedCharacterNameKey, characters[selectedIndex].displayName);
        PlayerPrefs.Save();

        if (playFade != null)
            playFade.StartTransition();
        else
            SceneManager.LoadScene(gameSceneName);
    }

    public void BackToMainMenu()
    {
        if (Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
        else
        {
            ShowStatus("Add MainMenu to Build Profiles > Scene List.");
            Debug.LogWarning("Cirks: MainMenu is not in the build scene list.");
        }
    }

    private void ShowStatus(string message)
    {
        if (statusLabel != null) statusLabel.text = message;
    }

    private void OnDestroy()
    {
        if (previousButton != null) previousButton.onClick.RemoveListener(PreviousCharacter);
        if (nextButton != null) nextButton.onClick.RemoveListener(NextCharacter);
        if (confirmButton != null) confirmButton.onClick.RemoveListener(ConfirmSelection);
        if (closeButton != null) closeButton.onClick.RemoveListener(BackToMainMenu);
    }
}
