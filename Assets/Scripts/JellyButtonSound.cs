
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(AudioSource))]
public class JellyButtonSound : MonoBehaviour,
    IPointerEnterHandler, IPointerDownHandler
{
    [Header("Jelly UI Sounds")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip pressSound;

    [Header("Volumes")]
    [Range(0f, 1f)]
    [SerializeField] private float hoverVolume = 0.12f;

    [Range(0f, 1f)]
    [SerializeField] private float pressVolume = 0.45f;

    private AudioSource audioSource;
    private Button button;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        button = GetComponent<Button>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.IsInteractable() && hoverSound != null)
            audioSource.PlayOneShot(hoverSound, hoverVolume);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left &&
            button.IsInteractable() && pressSound != null)
        {
            audioSource.PlayOneShot(pressSound, pressVolume);
        }
    }
}
