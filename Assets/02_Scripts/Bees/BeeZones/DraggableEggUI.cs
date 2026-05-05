using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableEggUI : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Vector2 startPosition;
    private Transform startParent;

    private bool dropped = false;

    private QueueSlotUI currentSlot;

    public EggType eggType;

    [Header("Cost")]
    public int honeyCost;

    [Header("Hover Popup")]
    public GameObject hoverPopup;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (hoverPopup != null)
            hoverPopup.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = rectTransform.anchoredPosition;
        startParent = transform.parent;

        dropped = false;
        canvasGroup.blocksRaycasts = false;

        if (hoverPopup != null)
            hoverPopup.SetActive(false);
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
            SpawnNewEgg();
        }
    }

    public void SnapToSlot(Transform slot)
    {
        transform.SetParent(slot);
        rectTransform.anchoredPosition = Vector2.zero;

        dropped = true;
        currentSlot = slot.GetComponent<QueueSlotUI>();
    }

    private void SpawnNewEgg()
    {
        GameObject newEgg = Instantiate(gameObject, startParent);
        RectTransform rt = newEgg.GetComponent<RectTransform>();
        rt.anchoredPosition = startPosition;
    }

    public void ResetToStartPosition()
    {
        rectTransform.anchoredPosition = startPosition;
        transform.SetParent(startParent);
        dropped = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverPopup != null)
        {
            hoverPopup.SetActive(true);
            hoverPopup.transform.SetAsLastSibling();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverPopup != null)
            hoverPopup.SetActive(false);
    }
}