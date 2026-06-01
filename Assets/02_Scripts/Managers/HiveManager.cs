using System.Collections.Generic;
using UnityEngine;

public class HiveManager : MonoBehaviour
{
    public static HiveManager Instance;

    private Dictionary<Bee.BeeType, int> beeCounts = new Dictionary<Bee.BeeType, int>();
    private HashSet<Bee> registeredBees = new HashSet<Bee>();

    private HashSet<SleepZone> sleepZones = new HashSet<SleepZone>();

    private int totalBees;

    private void Awake()
    {
        Instance = this;

        foreach (Bee.BeeType type in System.Enum.GetValues(typeof(Bee.BeeType)))
        {
            beeCounts[type] = 0;
        }
    }

    private void Start()
    {
        Bee[] bees = FindObjectsOfType<Bee>();
        foreach (Bee bee in bees)
        {
            RegisterBee(bee);
        }
    }

    // ------------------------
    // BEE TRACKING
    // ------------------------

    public void RegisterBee(Bee bee)
    {
        if (bee == null) return;
        if (registeredBees.Contains(bee)) return;

        registeredBees.Add(bee);

        totalBees++;
        beeCounts[bee.beeType]++;
    }

    public void UnregisterBee(Bee bee)
    {
        if (bee == null) return;
        if (!registeredBees.Contains(bee)) return;

        registeredBees.Remove(bee);

        totalBees--;
        beeCounts[bee.beeType]--;
    }

    // ------------------------
    // GETTERS
    // ------------------------

    public int GetTotalBees()
    {
        return totalBees;
    }

    public int GetBeeCount(Bee.BeeType type)
    {
        return beeCounts[type];
    }

    // ------------------------
    // SLEEP ZONES (CAPACITY)
    // ------------------------

    public void RegisterSleepZone(SleepZone zone)
    {
        if (zone == null) return;
        sleepZones.Add(zone);
    }

    public void UnregisterSleepZone(SleepZone zone)
    {
        if (zone == null) return;
        sleepZones.Remove(zone);
    }

    public int GetHiveCapacity()
    {
        int total = 0;

        foreach (var zone in sleepZones)
        {
            if (zone == null) continue;
            total += zone.capacityPerZone;
        }

        return total;
    }

    // ------------------------
    // SPAWN RULE (FIXED)
    // ------------------------

    public bool CanSpawnBee()
    {
        return totalBees < GetHiveCapacity();
    }
}