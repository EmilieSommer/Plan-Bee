using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class NurseBeeZone : MonoBehaviour
{
    private BoxCollider2D box;

    [Header("UI")]
    public GameObject nurseCanvas;

    private void Awake()
    {
        box = GetComponent<BoxCollider2D>();

        if (box == null)
        {
            Debug.LogError("NurseBeeZone: Missing BoxCollider2D!");
            return;
        }

        box.isTrigger = false;
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