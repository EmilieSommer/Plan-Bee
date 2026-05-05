using UnityEngine;
using UnityEngine.EventSystems;

public class ZoneHoverInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject infoPanel;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (infoPanel != null)
            infoPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);
    }
}