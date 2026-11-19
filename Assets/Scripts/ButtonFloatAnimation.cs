
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ButtonFloatAnimation : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveX = 5f;
    [SerializeField] private float moveY = 6f;

    [Header("Rotation")]
    [SerializeField] private float rotationAngle = 2f;

    [Header("Animation Speed")]
    [SerializeField] private float animationSpeed = 1.2f;

    [Header("Individual Animation")]
    [SerializeField] private bool randomStartPhase = true;

    private RectTransform rectTransform;
    private Vector2 startPosition;
    private Quaternion startRotation;
    private float phase;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        startPosition = rectTransform.anchoredPosition;
        startRotation = rectTransform.localRotation;

        if (randomStartPhase)
        {
            phase = Random.Range(0f, Mathf.PI * 2f);
        }
    }

    private void Update()
    {
        float time = Time.unscaledTime * animationSpeed + phase;

        // Smooth horizontal movement
        float x = Mathf.Sin(time) * moveX;

        // Smooth vertical floating
        float y = Mathf.Sin(time * 1.3f + 1f) * moveY;

        // Gentle rotation left and right
        float angle = Mathf.Sin(time * 0.8f) * rotationAngle;

        rectTransform.anchoredPosition =
            startPosition + new Vector2(x, y);

        rectTransform.localRotation =
            startRotation * Quaternion.Euler(0f, 0f, angle);
    }

    private void OnDisable()
    {
        if (rectTransform == null)
            return;

        rectTransform.anchoredPosition = startPosition;
        rectTransform.localRotation = startRotation;
    }
}
