using UnityEngine;

public class ClickManager : MonoBehaviour
{
    public LayerMask honeyLayer;

    void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, honeyLayer);

        if (hit.collider != null)
        {
            Honey honey = hit.collider.GetComponent<Honey>();

            if (honey != null)
            {
                honey.Collect();
            }
        }
    }
}