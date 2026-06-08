using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class BeeTypeHoverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    public GameObject hoverPanel;

    [Header("Text Objects (Drag dit GameObject herind!)")]
    public GameObject iconCountTextObj;
    public GameObject hoverCountTextObj;

    [HideInInspector] public TMP_Text iconCountText;   // number on main image
    [HideInInspector] public TMP_Text hoverCountText;  // number inside hover panel

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

        if (iconCountText != null) iconCountText.text = count;
        if (hoverCountText != null) hoverCountText.text = count;

        UpdateText(iconCountTextObj, count);
        UpdateText(hoverCountTextObj, count);
    }

    void UpdateText(GameObject obj, string value)
    {
        if (obj == null) return;
        var tmp = obj.GetComponentInChildren<TMP_Text>();
        if (tmp != null) { tmp.text = value; return; }
        var txt = obj.GetComponentInChildren<UnityEngine.UI.Text>();
        if (txt != null) { txt.text = value; }
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