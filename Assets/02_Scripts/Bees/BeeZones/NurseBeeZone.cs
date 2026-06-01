using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class NurseBeeZone : Zone
{
    private BoxCollider2D box;

    [Header("UI")]
    public GameObject nurseCanvas;

    [Header("Slots (assign per zone in Inspector)")]
    [SerializeField] private QueueSlotUI[] slots;

    private void Awake()
    {
        zoneType = Bee.BeeType.Nurse;
        box = GetComponent<BoxCollider2D>();

        SetupCapacity();
    }

    private void Start()
    {
        // ✅ bind ONCE → permanent ownership
        foreach (QueueSlotUI slot in slots)
        {
            slot.SetZone(this);
        }
    }

    void SetupCapacity()
    {
        if (limits.Count == 0)
        {
            limits.Add(new BeeTypeLimit
            {
                type = Bee.BeeType.Nurse,
                capacity = 5,
                current = 0
            });
        }
    }

    public Vector2 GetRandomPoint()
    {
        Bounds bounds = box.bounds;

        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);

        return new Vector2(x, y);
    }

    public bool IsInside(Vector2 position)
    {
        return box.bounds.Contains(position);
    }

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