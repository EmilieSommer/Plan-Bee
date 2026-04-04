using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableEggUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Vector2 startPosition;
    private Transform startParent;

    private bool dropped = false;

    private QueueSlotUI currentSlot;

    public EggType eggType;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = rectTransform.anchoredPosition;
        startParent = transform.parent;

        dropped = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (!dropped)
        {
            rectTransform.anchoredPosition = startPosition;
        }
        else
        {
            // spawn new egg in UI
            SpawnNewEgg();
        }
    }
    public void SnapToSlot(Transform slot)
    {
        transform.SetParent(slot);
        rectTransform.anchoredPosition = Vector2.zero;

        dropped = true;

        // ✅ Save which slot this egg belongs to
        currentSlot = slot.GetComponent<QueueSlotUI>();
    }

    private void SpawnNewEgg()
    {
        GameObject newEgg = Instantiate(gameObject, startParent);
        RectTransform rt = newEgg.GetComponent<RectTransform>();
        rt.anchoredPosition = startPosition;
    }
}