using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Toolbar that lets the player choose a tile type and click the grid to place it.
/// Wire up buttons in the Inspector; each calls SelectType() with its HiveTileType.
/// Right-click or pressing Escape cancels placement.
/// </summary>
public class BuildPanel : MonoBehaviour
{
    [Header("Toolbar Buttons (assign in Inspector)")]
    [SerializeField] private Button insideHiveBtn;
    [SerializeField] private Button solidBtn;
    [SerializeField] private Button broodBtn;
    [SerializeField] private Button storageBtn;
    [SerializeField] private Button cancelBtn;

    [Header("Visual feedback")]
    [SerializeField] private Color selectedColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color normalColor   = Color.white;

    private HiveTileType _selected = HiveTileType.None;
    private Button _activeBtn;

    void Start()
    {
        if (insideHiveBtn) insideHiveBtn.onClick.AddListener(() => SelectType(HiveTileType.InsideHive, insideHiveBtn));
        if (solidBtn)      solidBtn.onClick.AddListener(()      => SelectType(HiveTileType.Solid,      solidBtn));
        if (broodBtn)      broodBtn.onClick.AddListener(()      => SelectType(HiveTileType.Brood,      broodBtn));
        if (storageBtn)    storageBtn.onClick.AddListener(()    => SelectType(HiveTileType.Storage,    storageBtn));
        if (cancelBtn)     cancelBtn.onClick.AddListener(Cancel);
    }

    void Update()
    {
        if (_selected == HiveTileType.None) return;

        // Cancel
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            Cancel(); return;
        }

        // Place on left-click, skip if over UI
        if (!Input.GetMouseButtonDown(0)) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0f;
        Vector3Int cell = HiveGrid.Instance.WorldToCell(world);
        if (!HiveGrid.Instance.TryMark(cell, _selected))
            BuildCursor.Instance?.FlashInvalid(cell);
    }

    // ── API ───────────────────────────────────────────────────────────────────

    public void SelectType(HiveTileType type, Button btn = null)
    {
        // Reset previous
        if (_activeBtn) _activeBtn.image.color = normalColor;

        _selected  = type;
        _activeBtn = btn;

        if (_activeBtn) _activeBtn.image.color = selectedColor;
    }

    public void Cancel()
    {
        if (_activeBtn) _activeBtn.image.color = normalColor;
        _selected  = HiveTileType.None;
        _activeBtn = null;
    }

    public HiveTileType Selected => _selected;
}
