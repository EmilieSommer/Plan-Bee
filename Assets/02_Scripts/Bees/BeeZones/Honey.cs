using UnityEngine;

public class Honey : MonoBehaviour
{
    public int value = 1;

    public void Collect()
    {
        CurrencyManager.Instance.AddHoney(value);
        Destroy(gameObject);
    }
}