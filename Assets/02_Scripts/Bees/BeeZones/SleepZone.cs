using UnityEngine;
using System.Collections.Generic;

public class SleepZone : MonoBehaviour
{
    public int capacityPerZone = 1;

    private HashSet<Bee> registeredBees = new HashSet<Bee>();

    public bool HasSpace => registeredBees.Count < capacityPerZone;

    private void Start()
    {
        if (HiveManager.Instance != null)
            HiveManager.Instance.RegisterSleepZone(this);
    }

    private void OnDestroy()
    {
        if (HiveManager.Instance != null)
            HiveManager.Instance.UnregisterSleepZone(this);
    }

    public bool IsRegistered(Bee bee) => registeredBees.Contains(bee);

    public bool TryRegister(Bee bee)
    {
        if (registeredBees.Contains(bee)) return true;
        if (!HasSpace) return false;
        registeredBees.Add(bee);
        return true;
    }

    public void Unregister(Bee bee)
    {
        registeredBees.Remove(bee);
    }
}