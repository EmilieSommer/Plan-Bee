using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class BeeTypeHoverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    public GameObject hoverPanel;

    public TMP_Text iconCountText;   // number on main image
    public TMP_Text hoverCountText;  // number inside hover panel

    [Header("Data")]
    public Bee.BeeType beeType;

    private void Start()
    {
        UpdateCounts();

        if (hoverPanel != null)
            hoverPanel.SetActive(false);
    }

    private void Update()
    {
        UpdateCounts();
    }

    void UpdateCounts()
    {
        if (HiveManager.Instance == null) return;

        string count = HiveManager.Instance.GetBeeCount(beeType).ToString();

        if (iconCountText != null)
            iconCountText.text = count;

        if (hoverCountText != null)
            hoverCountText.text = count;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverPanel != null)
            hoverPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverPanel != null)
            hoverPanel.SetActive(false);
    }
}