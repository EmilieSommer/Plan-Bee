using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class NurseBeeZone : MonoBehaviour
{
    private BoxCollider2D box;

    [Header("UI")]
    public GameObject nurseCanvas; // assign in inspector

    private void Awake()
    {
        box = GetComponent<BoxCollider2D>();

        // IMPORTANT: collider must NOT be trigger for OnMouseDown
        box.isTrigger = false;
    }

    private void OnMouseDown()
    {
        if (nurseCanvas != null)
        {
            nurseCanvas.SetActive(true);
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
}