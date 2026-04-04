using UnityEngine;
using UnityEngine.EventSystems;

public class Honey : MonoBehaviour
{
    public int value = 1;

    private void Update()
    {
        // 🚫 Ignore if clicking UI
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            if (hit != null && hit.gameObject == gameObject)
            {
                Collect();
            }
        }
    }

    void Collect()
    {
        CurrencyManager.Instance.AddHoney(value);
        Destroy(gameObject);
    }
}