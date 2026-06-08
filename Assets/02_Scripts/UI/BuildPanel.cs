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
    [Header("Main Menu")]
    [SerializeField] private Button mainBuildBtn;
    [SerializeField] private GameObject buildSubmenu;

    [Header("Toolbar Buttons (assign in Inspector)")]
    [SerializeField] private Button broodBtn;
    [SerializeField] private Button storageBtn;
    [SerializeField] private Button insideHiveBtn;
    [SerializeField] private Button droneBtn;
    [SerializeField] private Button cancelBtn;

    [Header("Price Info Section")]
    public GameObject priceSection;
    public TMPro.TextMeshProUGUI priceText;

    [Header("Zone Prefabs")]
    public GameObject broodPrefab;
    public GameObject storagePrefab;
    public GameObject insideHivePrefab;
    public GameObject dronePrefab;

    [Header("Zone Costs")]
    public int broodCost = 4;
    public int storageCost = 6;
    public int insideHiveCost = 1;
    public int droneCost = 8;

    [Header("Zone Build Times")]
    public float broodBuildTime = 10f;
    public float storageBuildTime = 15f;
    public float insideHiveBuildTime = 8f;
    public float droneBuildTime = 20f;

    [Header("Visual feedback")]
    [SerializeField] private Color selectedColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color normalColor   = Color.white;

    private HiveTileType _selectedTile = HiveTileType.None;
    private GameObject _selectedPrefab = null;
    private HiveTileType _selectedVisualForPrefab = HiveTileType.None;
    private int _selectedCost = 0;
    private float _selectedTime = 5f;
    
    private Button _activeBtn;

    void Start()
    {
        if (mainBuildBtn)  mainBuildBtn.onClick.AddListener(ToggleMenu);
        if (buildSubmenu)  buildSubmenu.SetActive(false); // Start hidden

        // Wire up the 4 core buttons to place both the Zone Prefab AND the underlying Tile!
        if (broodBtn)      broodBtn.onClick.AddListener(()      => SelectZone(broodPrefab, HiveTileType.Brood, broodCost, broodBuildTime, broodBtn));
        if (storageBtn)    storageBtn.onClick.AddListener(()    => SelectZone(storagePrefab, HiveTileType.Storage, storageCost, storageBuildTime, storageBtn));
        if (insideHiveBtn) insideHiveBtn.onClick.AddListener(() => SelectZone(insideHivePrefab, HiveTileType.InsideHive, insideHiveCost, insideHiveBuildTime, insideHiveBtn));
        if (droneBtn)      droneBtn.onClick.AddListener(()      => SelectZone(dronePrefab, HiveTileType.InsideHive, droneCost, droneBuildTime, droneBtn));
        
        if (cancelBtn)     cancelBtn.onClick.AddListener(Cancel);

        UpdateUIButtonText();
    }

    private void UpdateUIButtonText()
    {
        if (broodBtn) UpdateBtnText(broodBtn, $"Brood\n({broodCost} Honey)");
        if (storageBtn) UpdateBtnText(storageBtn, $"Storage\n({storageCost} Honey)");
        if (insideHiveBtn) UpdateBtnText(insideHiveBtn, $"Space\n({insideHiveCost} Honey)");
        if (droneBtn) UpdateBtnText(droneBtn, $"Drone Post\n({droneCost} Honey)");
    }

    private void UpdateBtnText(Button btn, string newText)
    {
        TMPro.TextMeshProUGUI txt = btn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (txt != null) txt.text = newText;
    }

    void Update()
    {
        bool isBuildingTile = _selectedTile != HiveTileType.None;
        bool isBuildingZone = _selectedPrefab != null;
        
        if (!isBuildingTile && !isBuildingZone)
        {
            if (Input.GetMouseButtonDown(0) && _activeBtn != null)
            {
                Debug.LogWarning("You clicked the grid, but no Prefab is assigned in the BuildPanel Inspector for this button!");
            }
            return;
        }

        // Cancel
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            Cancel(); return;
        }

        // Place on left-click (hold to paint), skip if over UI
        if (!Input.GetMouseButton(0)) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0f;
        Vector3Int cell = HiveGrid.Instance.WorldToCell(world);
        
        bool success = false;
        
        if (isBuildingZone)
        {
            if (CurrencyManager.Instance != null && CurrencyManager.Instance.honey < _selectedCost)
            {
                Debug.LogWarning("Not enough Honey! Costs " + _selectedCost + " but you have " + CurrencyManager.Instance.honey);
            }
            success = HiveGrid.Instance.TryMarkZone(cell, _selectedPrefab, _selectedVisualForPrefab, _selectedCost, _selectedTime);
        }
        else if (isBuildingTile)
        {
            success = HiveGrid.Instance.TryMark(cell, _selectedTile);
        }

        if (!success)
        {
            Debug.Log("Failed to build at " + cell + ". Either not enough honey, or invalid tile!");
            BuildCursor.Instance?.FlashInvalid(cell);
        }
    }

    // ── API ───────────────────────────────────────────────────────────────────

    public void ToggleMenu()
    {
        bool isActive = buildSubmenu != null && buildSubmenu.activeSelf;
        if (isActive)
        {
            Cancel(); // Close and reset
        }
        else
        {
            if (buildSubmenu) buildSubmenu.SetActive(true);
            if (mainBuildBtn) mainBuildBtn.image.color = selectedColor;
            HiveGrid.Instance?.ShowAllBuildableIndicators();
        }
    }

    public void SelectZone(GameObject prefab, HiveTileType visualType, int cost, float time, Button btn = null)
    {
        ClearSelection();
        _selectedPrefab = prefab;
        _selectedVisualForPrefab = visualType;
        _selectedCost = cost;
        _selectedTime = time;
        _activeBtn = btn;
        if (_activeBtn) _activeBtn.image.color = selectedColor;

        if (priceSection != null) priceSection.SetActive(true);
        if (priceText != null)
        {
            string zoneName = "Zone";
            if (visualType == HiveTileType.Brood) zoneName = "Brood Chamber";
            else if (visualType == HiveTileType.Storage) zoneName = "Storage Area";
            else if (btn == droneBtn) zoneName = "Drone Post";
            else if (visualType == HiveTileType.InsideHive) zoneName = "Space";

            string desc = "";
            switch (zoneName)
            {
                case "Brood Chamber": desc = "Hatch new bees here to expand your colony."; break;
                case "Storage Area": desc = "A safe place for Foragers to drop off precious pollen and honey."; break;
                case "Drone Post": desc = "Drones defend the hive but need a post!"; break;
                case "Space": desc = "Expand your hive! Bees need solid space to walk on."; break;
            }

            priceText.text = $"<size=120%><b>{zoneName.ToUpper()}</b></size>\n{desc}\nPrice: {cost}";
        }
    }

    private void ClearSelection()
    {
        if (_activeBtn) _activeBtn.image.color = normalColor;
        _selectedTile = HiveTileType.None;
        _selectedPrefab = null;
        _selectedVisualForPrefab = HiveTileType.None;

        if (priceSection != null) priceSection.SetActive(false);
    }

    public void Cancel()
    {
        ClearSelection();
        if (mainBuildBtn) mainBuildBtn.image.color = normalColor;
        if (buildSubmenu) buildSubmenu.SetActive(false);
        HiveGrid.Instance?.HideBuildIndicators();
    }

    public bool IsBuilding => _selectedTile != HiveTileType.None || _selectedPrefab != null;
    public HiveTileType Selected => _selectedTile != HiveTileType.None ? _selectedTile : _selectedVisualForPrefab;
}
