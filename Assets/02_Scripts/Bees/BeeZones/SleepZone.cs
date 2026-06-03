using UnityEngine;
using System.Collections.Generic;

public class SleepZone : MonoBehaviour
{
    public int capacityPerZone = 2;

    private HashSet<Bee> registeredBees = new HashSet<Bee>();

    public bool HasSpace => registeredBees.Count < capacityPerZone;

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