using UnityEngine;

public class Honey : MonoBehaviour
{
    public int value = 1;

    private bool isCarried = false;

    private void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 1000;
    }

    public void SetCarried(bool state)
    {
        isCarried = state;
    }

    public bool IsCarried()
    {
        return isCarried;
    }

    public void Collect()
    {
        if (isCarried) return; // ❌ block collection while stolen

        CurrencyManager.Instance.AddHoney(value);
        Destroy(gameObject);
    }
}