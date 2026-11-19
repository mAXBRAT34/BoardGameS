using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fast, endlessly looping clouds travelling in both directions.
/// Cloud visibility depends ONLY on the Cloud Animation setting,
/// never on fullscreen or screen resolution.
/// Attach to each UI cloud Image under Canvas > CloudLayer.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CloudFloatUI : MonoBehaviour
{
    public enum FlightDirection
    {
        Alternate,
        LeftToRight,
        RightToLeft
    }

    [Header("Fast movement")]
    [SerializeField, Min(1f)] private float moveSpeed = 25f;
    [SerializeField, Min(1f)] private float speedMultiplier = 10f;
    [SerializeField, Range(0f, 0.5f)] private float speedVariation = 0.15f;

    [Header("Directions and looping")]
    [SerializeField] private FlightDirection direction = FlightDirection.Alternate;
    [SerializeField] private bool reverseDirectionOnRepeat = true;
    [SerializeField] private bool randomStartPosition = true;
    [SerializeField] private bool randomizeHeightOnRepeat = true;

    [Header("Smooth floating")]
    [SerializeField, Min(0f)] private float floatHeight = 8f;
    [SerializeField, Min(0f)] private float floatSpeed = 1.6f;
    [SerializeField, Range(0f, 10f)] private float rotationAngle = 2f;

    private const string CloudEffectsKey = "Cirks_CloudEffects";
    private static int alternatingCloudCounter;

    private RectTransform cloud;
    private RectTransform cloudLayer;
    private Graphic cloudGraphic;
    private Quaternion originalRotation;
    private Vector2 previousLayerSize;
    private float baseY;
    private float phase;
    private float elapsed;
    private float actualSpeed;
    private int flightSign;
    private bool initialized;

    private bool CloudsEnabled => PlayerPrefs.GetInt(CloudEffectsKey, 1) == 1;

    private void Awake()
    {
        cloud = GetComponent<RectTransform>();
        cloudLayer = transform.parent as RectTransform;
        cloudGraphic = GetComponent<Graphic>();
        originalRotation = cloud.localRotation;

        if (cloudGraphic != null)
            cloudGraphic.raycastTarget = false;

        RemoveLegacyLayerHiding();
        ApplyVisibility();
    }

    private void OnEnable()
    {
        RemoveLegacyLayerHiding();
        ApplyVisibility();
    }

    private void Start()
    {
        if (cloudLayer == null)
        {
            Debug.LogError("CloudFloatUI: Put clouds under Canvas > CloudLayer.", this);
            enabled = false;
            return;
        }

        Canvas.ForceUpdateCanvases();

        cloud.anchorMin = new Vector2(0.5f, 0.5f);
        cloud.anchorMax = new Vector2(0.5f, 0.5f);
        cloud.pivot = new Vector2(0.5f, 0.5f);

        if (direction == FlightDirection.Alternate)
            flightSign = (alternatingCloudCounter++ % 2 == 0) ? -1 : 1;
        else
            flightSign = direction == FlightDirection.LeftToRight ? 1 : -1;

        ChooseNewSpeed();
        baseY = ChooseRandomY();
        phase = Random.Range(0f, Mathf.PI * 2f);
        elapsed = 0f;

        float halfWidth = cloudLayer.rect.width * 0.5f;
        float outsideEdge = halfWidth + CloudHalfWidth;
        float startX = randomStartPosition
            ? Random.Range(-halfWidth * 0.85f, halfWidth * 0.85f)
            : (flightSign > 0 ? -outsideEdge : outsideEdge);

        cloud.anchoredPosition = new Vector2(startX, baseY);
        previousLayerSize = cloudLayer.rect.size;
        initialized = true;
        ApplyVisibility();
    }

    private void Update()
    {
        if (!initialized)
            return;

        // Resize must not change the cloud toggle setting.
        // Move clouds back into the resized visible area instead of losing them.
        AdaptToCanvasResize();

        if (!CloudsEnabled)
            return;

        float dt = Time.deltaTime;
        elapsed += dt;

        Vector2 position = cloud.anchoredPosition;
        position.x += flightSign * actualSpeed * dt;
        position.y = baseY + Mathf.Sin(elapsed * floatSpeed + phase) * floatHeight;

        float outsideEdge = cloudLayer.rect.width * 0.5f + CloudHalfWidth;
        bool passedEdge = flightSign > 0
            ? position.x > outsideEdge
            : position.x < -outsideEdge;

        if (passedEdge)
        {
            if (reverseDirectionOnRepeat)
                flightSign = -flightSign;

            // Re-enter from the side opposite to the current flight direction.
            position.x = flightSign > 0 ? -outsideEdge : outsideEdge;
            if (randomizeHeightOnRepeat)
                baseY = ChooseRandomY();

            position.y = baseY;
            ChooseNewSpeed();
        }

        cloud.anchoredPosition = position;
        cloud.localRotation = originalRotation * Quaternion.Euler(
            0f, 0f, Mathf.Sin(elapsed * 0.9f + phase) * rotationAngle);
    }

    private void LateUpdate()
    {
        // Reapply the saved cloud setting even after Unity rebuilds the Canvas.
        RemoveLegacyLayerHiding();
        ApplyVisibility();
    }

    private void ApplyVisibility()
    {
        if (cloudGraphic == null)
            return;

        bool shouldBeVisible = CloudsEnabled;
        if (cloudGraphic.enabled != shouldBeVisible)
            cloudGraphic.enabled = shouldBeVisible;
    }

    private void RemoveLegacyLayerHiding()
    {
        if (cloudLayer == null)
            return;

        // Previous script versions could set the whole layer's CanvasGroup
        // alpha to 0. Keep that layer visible; only the cloud Images are hidden.
        CanvasGroup group = cloudLayer.GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.alpha = 1f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }
    }

    private void AdaptToCanvasResize()
    {
        if (cloudLayer == null)
            return;

        Vector2 newSize = cloudLayer.rect.size;
        if (newSize.x < 1f || newSize.y < 1f)
            return;

        if (previousLayerSize.x < 1f || previousLayerSize.y < 1f)
        {
            previousLayerSize = newSize;
            return;
        }

        if (Mathf.Abs(previousLayerSize.x - newSize.x) < 1f &&
            Mathf.Abs(previousLayerSize.y - newSize.y) < 1f)
            return;

        Vector2 position = cloud.anchoredPosition;
        position.x *= newSize.x / previousLayerSize.x;
        position.x = Mathf.Clamp(position.x, -newSize.x * 0.4f, newSize.x * 0.4f);

        baseY *= newSize.y / previousLayerSize.y;
        float maxY = Mathf.Max(0f,
            newSize.y * 0.5f - cloud.rect.height * Mathf.Abs(cloud.localScale.y) * 0.5f - floatHeight);
        baseY = Mathf.Clamp(baseY, -maxY, maxY);
        position.y = baseY;

        cloud.anchoredPosition = position;
        previousLayerSize = newSize;
    }

    private float CloudHalfWidth =>
        cloud.rect.width * Mathf.Abs(cloud.localScale.x) * 0.5f;

    private float ChooseRandomY()
    {
        float range = Mathf.Max(0f,
            cloudLayer.rect.height * 0.5f
            - cloud.rect.height * Mathf.Abs(cloud.localScale.y) * 0.5f
            - floatHeight);
        return Random.Range(-range, range);
    }

    private void ChooseNewSpeed()
    {
        float variation = Random.Range(1f - speedVariation, 1f + speedVariation);
        actualSpeed = Mathf.Max(1f, moveSpeed * speedMultiplier * variation);
    }

    private void OnDisable()
    {
        if (cloud != null)
            cloud.localRotation = originalRotation;
    }
}
