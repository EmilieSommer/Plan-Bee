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
    }

    private void Start()
    {
        foreach (QueueSlotUI slot in slots)
            slot.SetZone(this);
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