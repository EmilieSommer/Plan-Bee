using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class NurseBeeZone : Zone
{
    private BoxCollider2D box;

    [Header("UI")]
    public GameObject nurseCanvas;

    private void Awake()
    {
        zoneType = Bee.BeeType.Nurse;
        box = GetComponent<BoxCollider2D>();
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
        {
            nurseCanvas.SetActive(true);
        }
    }
}