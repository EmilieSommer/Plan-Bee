using UnityEngine;

public class SleepZone : MonoBehaviour
{
    public int capacityPerZone = 2;

    private void OnEnable()
    {
        if (HiveManager.Instance != null)
            HiveManager.Instance.RegisterSleepZone(this);
    }

    private void OnDisable()
    {
        if (HiveManager.Instance != null)
            HiveManager.Instance.UnregisterSleepZone(this);
    }
}