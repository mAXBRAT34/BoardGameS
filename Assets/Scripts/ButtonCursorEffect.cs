
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonCursorEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [SerializeField] private CursorScript cursorManager;

    private Button button;
    private bool isHovered;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;

        isHovered = true;

        if (cursorManager != null)
            cursorManager.ShowHoverCursor();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;

        if (cursorManager != null)
            cursorManager.RestoreCursor();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;

        if (cursorManager != null)
            cursorManager.ShowPressedCursor();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (cursorManager == null) return;

        if (isHovered && button.interactable)
            cursorManager.ShowHoverCursor();
        else
            cursorManager.RestoreCursor();
    }

    private void OnDisable()
    {
        if (cursorManager != null)
            cursorManager.RestoreCursor();
    }
}
