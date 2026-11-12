using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class DiceRunAwayUI : MonoBehaviour
{
    [Header("Escape Settings")]
    [SerializeField, Min(1f)] private float triggerDistance = 170f;
    [SerializeField, Min(1f)] private float escapeDistance = 270f;
    [SerializeField, Min(1f)] private float moveSpeed = 1000f;
    [SerializeField, Min(0f)] private float edgePadding = 20f;

    [Header("Visual Effect")]
    [SerializeField] private bool spinWhileRunning = true;
    [SerializeField] private float spinSpeed = 360f;

    private RectTransform imageRect;
    private RectTransform parentRect;
    private Canvas canvas;
    private Vector2 destination;
    private bool isRunning;

    private void Awake()
    {
        imageRect = GetComponent<RectTransform>();
        parentRect = imageRect.parent as RectTransform;
        canvas = GetComponentInParent<Canvas>();

        // The mouse proximity is detected in Update, so UI raycasts are unnecessary.
        // Prevents Button clicks even if the player somehow catches the moving image.
        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group == null)
            group = gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
    }

    private void Update()
    {
        if (parentRect == null)
            return;

        Camera uiCamera = canvas != null &&
                          canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, Input.mousePosition, uiCamera, out Vector2 mousePosition))
            return;

        float distanceToMouse = Vector2.Distance(imageRect.anchoredPosition, mousePosition);

        if (distanceToMouse < triggerDistance &&
            (!isRunning || Vector2.Distance(destination, mousePosition) < triggerDistance))
        {
            destination = ChooseEscapePoint(mousePosition);
            isRunning = true;
        }

        if (!isRunning)
            return;

        imageRect.anchoredPosition = Vector2.MoveTowards(
            imageRect.anchoredPosition,
            destination,
            moveSpeed * Time.unscaledDeltaTime
        );

        if (spinWhileRunning)
            imageRect.Rotate(0f, 0f, -spinSpeed * Time.unscaledDeltaTime);

        if (Vector2.Distance(imageRect.anchoredPosition, destination) < 0.1f)
            isRunning = false;
    }

    private Vector2 ChooseEscapePoint(Vector2 mousePosition)
    {
        Vector2 current = imageRect.anchoredPosition;
        Vector2 away = current - mousePosition;
        if (away.sqrMagnitude < 0.01f)
            away = Vector2.right;
        away.Normalize();

        Vector2 best = current;
        float bestScore = float.NegativeInfinity;

        // Try multiple directions, so the die can dodge along screen edges too.
        for (int i = 0; i < 16; i++)
        {
            float angle = i * Mathf.PI * 2f / 16f;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 candidate = ClampInsideParent(current + direction * escapeDistance);
            float movement = Vector2.Distance(candidate, current);
            float score = Vector2.Distance(candidate, mousePosition) + movement * 0.1f;

            if (score > bestScore)
            {
                bestScore = score;
                best = candidate;
            }
        }

        return best;
    }

    private Vector2 ClampInsideParent(Vector2 position)
    {
        // Works with the image's anchors and pivot set to Middle Center (0.5, 0.5).
        float halfWidth = imageRect.rect.width * Mathf.Abs(imageRect.localScale.x) * 0.5f;
        float halfHeight = imageRect.rect.height * Mathf.Abs(imageRect.localScale.y) * 0.5f;

        float minX = parentRect.rect.xMin + halfWidth + edgePadding;
        float maxX = parentRect.rect.xMax - halfWidth - edgePadding;
        float minY = parentRect.rect.yMin + halfHeight + edgePadding;
        float maxY = parentRect.rect.yMax - halfHeight - edgePadding;

        float x = minX <= maxX ? Mathf.Clamp(position.x, minX, maxX) : 0f;
        float y = minY <= maxY ? Mathf.Clamp(position.y, minY, maxY) : 0f;
        return new Vector2(x, y);
    }
}
