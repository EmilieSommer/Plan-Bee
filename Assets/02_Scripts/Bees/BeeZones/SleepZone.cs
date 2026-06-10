using UnityEngine;
using System.Collections.Generic;

public class SleepZone : MonoBehaviour
{
    public int capacityPerZone = 1;

    private HashSet<Bee> registeredBees = new HashSet<Bee>();

    public bool HasSpace => registeredBees.Count < capacityPerZone;

    private void Start()
    {
        ConstructionSite cs = GetComponent<ConstructionSite>();
        
        // If there's no construction site, or if it's already fully built, we register immediately.
        // Otherwise, the ConstructionSite will register us when FinishBuild() is called!
        if (cs == null || cs.GetProgress() >= 1f)
        {
            if (HiveManager.Instance != null)
                HiveManager.Instance.RegisterSleepZone(this);
        }
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