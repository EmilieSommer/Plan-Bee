using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class NurseBeeZone : Zone
{
    private BoxCollider2D box;

    [Header("UI")]
    public GameObject nurseCanvas;

    [Header("Slots (assign per zone in Inspector)")]
    [SerializeField] private QueueSlotUI[] slots;

    protected override void Awake()
    {
        base.Awake();
        zoneType = Bee.BeeType.Nurse;
        isStorageZone = false; // No longer acts as a honey storage drop-off
        depositRadius = 1.0f; // tighter radius for brood
        box = GetComponent<BoxCollider2D>();
    }

    [Header("Costs")]
    public int foragerCost = 5;
    public int nurseCost = 2;
    public int houseCost = 1;
    public int builderCost = 5;
    public int droneCost = 10;

    protected override void Start()
    {
        base.Start();
        foreach (QueueSlotUI slot in slots)
            slot.SetZone(this);

        UpdateUIButtonText();
    }

    private void UpdateUIButtonText()
    {
        if (nurseCanvas == null) return;
        
        // Find all buttons in the canvas and update their text based on their name
        UnityEngine.UI.Button[] buttons = nurseCanvas.GetComponentsInChildren<UnityEngine.UI.Button>(true);
        foreach (var btn in buttons)
        {
            TMPro.TextMeshProUGUI txt = btn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (txt == null) continue;

            string n = btn.name.ToLower();
            if (n.Contains("forager")) txt.text = $"Forager\n({foragerCost} Honey)";
            else if (n.Contains("nurse")) txt.text = $"Nurse\n({nurseCost} Honey)";
            else if (n.Contains("house")) txt.text = $"House Bee\n({houseCost} Honey)";
            else if (n.Contains("builder")) txt.text = $"Builder\n({builderCost} Honey)";
            else if (n.Contains("drone")) txt.text = $"Drone\n({droneCost} Honey)";
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public Vector2 GetRandomPoint()
    {
        Vector2 center = transform.position;
        // Hardcode a tiny radius instead of relying on potentially broken BoxColliders!
        return center + new Vector2(
            Random.Range(-0.3f, 0.3f),
            Random.Range(-0.3f, 0.3f)
        );
    }

    public bool IsInside(Vector2 position) => box.bounds.Contains(position);

    public void Open()
    {
        if (nurseCanvas != null)
            nurseCanvas.SetActive(true);
    }

    public void SpawnEgg(EggType type)
    {
        NurseBeeZone[] broods = FindObjectsOfType<NurseBeeZone>();
        NurseBeeZone targetZone = null;

        foreach (var b in broods)
        {
            if (b.HasFreeSlot())
            {
                targetZone = b;
                break;
            }
        }

        if (targetZone == null)
        {
            if (UIMessagePopup.Instance != null)
                UIMessagePopup.Instance.ShowMessage("All Brood Chambers are full!");
            else
                Debug.LogWarning("All Brood Chambers are full!");
            return;
        }

        int cost = 0;
        switch (type)
        {
            case EggType.Forager: cost = foragerCost; break;
            case EggType.Nurse: cost = nurseCost; break;
            case EggType.House: cost = houseCost; break;
            case EggType.Builder: cost = builderCost; break;
            case EggType.Drone: cost = droneCost; break;
        }

        if (CurrencyManager.Instance != null && !CurrencyManager.Instance.UseHoney(cost))
        {
            if (UIMessagePopup.Instance != null)
                UIMessagePopup.Instance.ShowMessage("Not enough Honey!");
            return; 
        }

        EggSpawner.Instance.SpawnEgg(type, targetZone);
    }

    public bool HasFreeSlot()
    {
        foreach (var slot in slots)
        {
            if (!slot.isFilled) return true;
        }
        return false;
    }
}