using UnityEngine;
using UnityEngine.EventSystems;

// Attach to a UI Image (Canvas) or a SpriteRenderer with Collider2D.
[RequireComponent(typeof(AudioSource))]
public class DiceHoverSpin : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float returnSpeed = 360f;
    [SerializeField] private bool resetOnExit = true;

    [Header("Shake while hovered")]
    [SerializeField] private bool enableShake = true;
    [SerializeField, Min(0f)] private float shakeAmount = 4f;
    [SerializeField, Min(0f)] private float shakeSpeed = 18f;

    [Header("Pulse while hovered")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField, Range(0f, 0.5f)] private float pulseAmount = 0.12f;
    [SerializeField, Min(0f)] private float pulseSpeed = 3f;
    [SerializeField, Min(0f)] private float smoothReturnSpeed = 10f;

    [Header("Dice Sound")]
    [SerializeField] private AudioClip rollingSound;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 0.35f;

    private AudioSource audioSource;
    private Vector3 initialPosition;
    private Vector3 initialScale;
    private Quaternion initialRotation;
    private bool isHovered;
    private float hoverTime;

    private void Awake()
    {
        initialPosition = transform.localPosition;
        initialScale = transform.localScale;
        initialRotation = transform.localRotation;

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
        audioSource.volume = soundVolume;
        audioSource.clip = rollingSound;
    }

    private void Update()
    {
        if (isHovered)
        {
            hoverTime += Time.deltaTime;

            // Spin the flat sprite around its center.
            transform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime, Space.Self);

            // Smooth irregular wobble: offset is always relative to the original position.
            if (enableShake)
            {
                float x = (Mathf.PerlinNoise(hoverTime * shakeSpeed, 0.37f) - 0.5f) * 2f * shakeAmount;
                float y = (Mathf.PerlinNoise(0.83f, hoverTime * shakeSpeed + 8f) - 0.5f) * 2f * shakeAmount;
                transform.localPosition = initialPosition + new Vector3(x, y, 0f);
            }
            else
            {
                transform.localPosition = initialPosition;
            }

            // Gently grow and shrink between original size and +pulseAmount.
            if (enablePulse)
            {
                float wave = (Mathf.Sin(hoverTime * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
                transform.localScale = initialScale * (1f + pulseAmount * wave);
            }
            else
            {
                transform.localScale = initialScale;
            }
        }
        else if (resetOnExit)
        {
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation, initialRotation, returnSpeed * Time.deltaTime);

            float blend = 1f - Mathf.Exp(-smoothReturnSpeed * Time.deltaTime);
            transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition, blend);
            transform.localScale = Vector3.Lerp(transform.localScale, initialScale, blend);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) => StartRolling();
    public void OnPointerExit(PointerEventData eventData) => StopRolling();

    // For SpriteRenderer + Collider2D (not needed for UI Image).
    private void OnMouseEnter() => StartRolling();
    private void OnMouseExit() => StopRolling();

    private void StartRolling()
    {
        if (isHovered) return;
        isHovered = true;
        hoverTime = 0f;

        if (rollingSound != null)
        {
            audioSource.clip = rollingSound;
            audioSource.volume = soundVolume;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void StopRolling()
    {
        isHovered = false;
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    private void OnDisable()
    {
        StopRolling();
        transform.localPosition = initialPosition;
        transform.localRotation = initialRotation;
        transform.localScale = initialScale;
    }
}
