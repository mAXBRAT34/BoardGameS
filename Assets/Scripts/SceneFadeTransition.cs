
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(Button))]
public class SceneFadeTransition : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string targetScene = "Settings";

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1f;

    private Button button;
    private bool isTransitioning = false;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(StartTransition);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(StartTransition);
    }

    public void StartTransition()
    {
        if (isTransitioning)
            return;

        if (!Application.CanStreamedLevelBeLoaded(targetScene))
        {
            Debug.LogError(
                "Scene not found: " + targetScene
            );
            return;
        }

        StartCoroutine(FadeToScene());
    }

    private IEnumerator FadeToScene()
    {
        isTransitioning = true;

        // Create a Canvas above everything
        GameObject canvasObject = new GameObject(
            "FadeCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasGroup),
            typeof(GraphicRaycaster)
        );

        Canvas canvas = canvasObject.GetComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000;

        CanvasGroup canvasGroup =
            canvasObject.GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true;

        // Create a fullscreen black image
        GameObject blackScreen = new GameObject(
            "BlackScreen",
            typeof(RectTransform),
            typeof(Image)
        );

        blackScreen.transform.SetParent(
            canvasObject.transform,
            false
        );

        RectTransform rect =
            blackScreen.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;

        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = blackScreen.GetComponent<Image>();
        image.color = Color.black;

        // Smoothly darken the screen
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, fadeDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            // Smooth animation
            t = t * t * (3f - 2f * t);

            canvasGroup.alpha = t;

            yield return null;
        }

        canvasGroup.alpha = 1f;

        // Only now load the next scene
        SceneManager.LoadScene(targetScene);
    }
}
