using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fast, looping UI clouds in both directions.
/// Attach to each cloud Image inside Canvas > CloudLayer.
/// Compatible with CloudEffectsToggle (PlayerPrefs key Cirks_CloudEffects).
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CloudFloatUI : MonoBehaviour
{
    public enum FlightDirection
    {
        Alternate,       // Cloud1 left, Cloud2 right, Cloud3 left, etc.
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
    private Quaternion initialRotation;
    private float baseY;
    private float phase;
    private float elapsed;
    private float actualSpeed;
    private int flightSign; // +1: from left to right; -1: from right to left
    private bool initialized;

    private void Awake()
    {
        cloud = GetComponent<RectTransform>();
        cloudLayer = transform.parent as RectTransform;
        initialRotation = cloud.localRotation;

        // Hide the UI cloud when Cloud Animation is OFF,
        // without disabling the GameObject or this script.
        cloudGraphic = GetComponent<Graphic>();
        if (cloudGraphic != null)
        {
            cloudGraphic.raycastTarget = false;
            cloudGraphic.enabled = PlayerPrefs.GetInt(CloudEffectsKey, 1) == 1;
        }
    }

    private void Start()
    {
        if (cloudLayer == null)
        {
            Debug.LogError("CloudFloatUI: Place cloud under CloudLayer (RectTransform).", this);
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
            flightSign = (direction == FlightDirection.LeftToRight) ? 1 : -1;

        ChooseNewSpeed();
        baseY = ChooseRandomY();
        phase = Random.Range(0f, Mathf.PI * 2f);
        elapsed = 0f;

        float halfWidth = cloudLayer.rect.width * 0.5f;
        float offscreenX = halfWidth + CloudHalfWidth;
        float startX = randomStartPosition
            ? Random.Range(-halfWidth, halfWidth)
            : (flightSign > 0 ? -offscreenX : offscreenX);

        cloud.anchoredPosition = new Vector2(startX, baseY);
        initialized = true;
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
        float factor = Random.Range(1f - speedVariation, 1f + speedVariation);
        actualSpeed = Mathf.Max(1f, moveSpeed * speedMultiplier * factor);
    }

    private void Update()
    {
        if (!initialized)
            return;

        bool cloudsEnabled = PlayerPrefs.GetInt(CloudEffectsKey, 1) == 1;

        // Switching OFF hides the Image completely, not just the motion.
        // The script still runs so switching ON makes it visible again.
        if (cloudGraphic != null && cloudGraphic.enabled != cloudsEnabled)
            cloudGraphic.enabled = cloudsEnabled;

        if (!cloudsEnabled)
            return;

        float delta = Time.deltaTime;
        elapsed += delta;

        Vector2 position = cloud.anchoredPosition;
        position.x += flightSign * actualSpeed * delta;
        position.y = baseY + Mathf.Sin(elapsed * floatSpeed + phase) * floatHeight;

        // Only respawn once the entire cloud is outside the screen.
        float limit = cloudLayer.rect.width * 0.5f + CloudHalfWidth;
        bool fullyOffscreen = flightSign > 0
            ? position.x > limit
            : position.x < -limit;

        if (fullyOffscreen)
        {
            if (reverseDirectionOnRepeat)
                flightSign *= -1;

            // Bring it back offscreen from the side it will fly INTO the screen.
            position.x = flightSign > 0 ? -limit : limit;
            if (randomizeHeightOnRepeat)
                baseY = ChooseRandomY();

            position.y = baseY;
            ChooseNewSpeed();
        }

        cloud.anchoredPosition = position;
        cloud.localRotation = initialRotation * Quaternion.Euler(
            0f, 0f, Mathf.Sin(elapsed * 0.9f + phase) * rotationAngle);
    }

    private void OnDisable()
    {
        if (cloud != null)
            cloud.localRotation = initialRotation;
    }
}
