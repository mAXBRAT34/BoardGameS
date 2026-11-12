using UnityEngine;
using UnityEngine.UI;

// Attach to each cloud Image inside Canvas > CloudLayer.
[RequireComponent(typeof(RectTransform))]
public class CloudFloatUI : MonoBehaviour
{
    [Header("Cloud movement")]
    [SerializeField, Min(1f)] private float moveSpeed = 25f;
    [SerializeField, Min(0f)] private float floatHeight = 8f;
    [SerializeField, Min(0f)] private float floatSpeed = 1f;
    [SerializeField] private bool randomStartPosition = true;

    private const string CloudEffectsKey = "Cirks_CloudEffects";

    private RectTransform cloud;
    private RectTransform area;
    private float baseY;
    private float phase;
    private float animationTime;
    private bool initialized;

    private void Awake()
    {
        cloud = GetComponent<RectTransform>();
        area = transform.parent as RectTransform;

        // Clouds should never block UI buttons.
        if (TryGetComponent<Graphic>(out var graphic))
            graphic.raycastTarget = false;
    }

    private void Start()
    {
        if (area == null)
        {
            Debug.LogError("CloudFloatUI: Put the cloud inside CloudLayer (UI RectTransform).", this);
            enabled = false;
            return;
        }

        Canvas.ForceUpdateCanvases();

        cloud.anchorMin = new Vector2(0.5f, 0.5f);
        cloud.anchorMax = new Vector2(0.5f, 0.5f);
        cloud.pivot = new Vector2(0.5f, 0.5f);

        baseY = RandomY();
        phase = Random.Range(0f, Mathf.PI * 2f);

        float startX = randomStartPosition
            ? Random.Range(-area.rect.width * 0.5f, area.rect.width * 0.5f)
            : area.rect.width * 0.5f + CloudHalfWidth;

        cloud.anchoredPosition = new Vector2(startX, baseY);
        initialized = true;
    }

    private float CloudHalfWidth => cloud.rect.width * Mathf.Abs(cloud.localScale.x) * 0.5f;

    private float RandomY()
    {
        float halfAvailableHeight = Mathf.Max(0f,
            area.rect.height * 0.5f - cloud.rect.height * Mathf.Abs(cloud.localScale.y) * 0.5f);

        return Random.Range(-halfAvailableHeight, halfAvailableHeight);
    }

    private void Update()
    {
        if (!initialized) return;

        // The toggle in Settings saves 0 = stop movement, 1 = animate.
        // When OFF, clouds stay visible, but freeze in their current positions.
        if (PlayerPrefs.GetInt(CloudEffectsKey, 1) == 0)
            return;

        float dt = Time.deltaTime;
        animationTime += dt;

        Vector2 position = cloud.anchoredPosition;
        position.x -= moveSpeed * dt;
        position.y = baseY + Mathf.Sin(animationTime * floatSpeed + phase) * floatHeight;

        float edge = area.rect.width * 0.5f + CloudHalfWidth;
        if (position.x < -edge)
        {
            position.x = edge;
            baseY = RandomY();
        }

        cloud.anchoredPosition = position;
    }
}
