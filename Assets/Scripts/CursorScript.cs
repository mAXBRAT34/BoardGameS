
using UnityEngine;

public class CursorScript : MonoBehaviour
{
    [Header("Available Cursors")]
    [SerializeField] private Texture2D[] cursors;

    [Header("Button Cursors")]
    [SerializeField] private Texture2D hoverCursor;
    [SerializeField] private Texture2D pressedCursor;

    [Header("Settings")]
    [SerializeField] private int defaultCursorIndex = 0;
    [SerializeField] private Vector2 hotSpot = Vector2.zero;

    private int selectedCursorIndex;

    private enum CursorState
    {
        Normal,
        Hover,
        Pressed
    }

    private CursorState currentState = CursorState.Normal;

    private void Start()
    {
        SetCursor(defaultCursorIndex);
    }

    public void SetCursor(int index)
    {
        if (cursors == null ||
            index < 0 ||
            index >= cursors.Length ||
            cursors[index] == null)
        {
            Debug.LogWarning("Invalid cursor index: " + index);
            return;
        }

        selectedCursorIndex = index;
        UpdateCursor();
    }

    public void ShowHoverCursor()
    {
        currentState = CursorState.Hover;
        UpdateCursor();
    }

    public void ShowPressedCursor()
    {
        currentState = CursorState.Pressed;
        UpdateCursor();
    }

    public void RestoreCursor()
    {
        currentState = CursorState.Normal;
        UpdateCursor();
    }

    private void UpdateCursor()
    {
        Texture2D normalCursor = null;

        if (cursors != null &&
            selectedCursorIndex >= 0 &&
            selectedCursorIndex < cursors.Length)
        {
            normalCursor = cursors[selectedCursorIndex];
        }

        Texture2D targetCursor = normalCursor;

        switch (currentState)
        {
            case CursorState.Hover:
                if (hoverCursor != null)
                    targetCursor = hoverCursor;
                break;

            case CursorState.Pressed:
                if (pressedCursor != null)
                    targetCursor = pressedCursor;
                break;
        }

        if (targetCursor == null)
        {
            Debug.LogWarning("Cursor texture is missing!");
            return;
        }

        Cursor.SetCursor(
            targetCursor,
            hotSpot,
            CursorMode.ForceSoftware
        );
    }
}
