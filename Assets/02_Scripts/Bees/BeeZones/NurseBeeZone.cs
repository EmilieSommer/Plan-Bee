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
        isStorageZone = true; // Act as a small storage drop-off
        depositRadius = 1.0f; // tighter radius for brood
        box = GetComponent<BoxCollider2D>();
    }

    protected override void Start()
    {
        base.Start();
        foreach (QueueSlotUI slot in slots)
            slot.SetZone(this);
            
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddCapacity(5, 5);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.RemoveCapacity(5, 5);
    }

    public Vector2 GetRandomPoint()
    {
        Bounds bounds = box.bounds;
        return new Vector2(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y)
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
        EggSpawner.Instance.SpawnEgg(type, this);
    }
}