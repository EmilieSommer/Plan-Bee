using System.Collections.Generic;
using UnityEngine;

public class HiveManager : MonoBehaviour
{
    public static HiveManager Instance;

    private Dictionary<Bee.BeeType, int> beeCounts = new Dictionary<Bee.BeeType, int>();
    private HashSet<Bee> registeredBees = new HashSet<Bee>();
    private HashSet<SleepZone> sleepZones = new HashSet<SleepZone>();

    private int totalBees;

    // NEW: queued eggs reserve capacity
    private int queuedEggs = 0;

    [Header("UI")]
    public TMPro.TextMeshProUGUI totalBeesText;

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

        SleepZone[] zones = FindObjectsOfType<SleepZone>();
        foreach (SleepZone zone in zones)
        {
            RegisterSleepZone(zone);
        }

        // Auto-link UI if it's missing in the inspector
        if (totalBeesText == null)
        {
            TMPro.TextMeshProUGUI[] allTexts = FindObjectsOfType<TMPro.TextMeshProUGUI>();
            foreach(var t in allTexts)
            {
                string n = t.name.ToLower();
                if (n.Contains("total") || n.Contains("capacity") || n.Contains("bee") && n.Contains("text"))
                {
                    totalBeesText = t;
                    break;
                }
            }
        }
    }

    private void Update()
    {
        if (totalBeesText != null)
        {
            totalBeesText.text = totalBees + " / " + GetHiveCapacity();
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
    // QUEUE TRACKING (NEW)
    // ------------------------

    public void RegisterQueuedEgg()
    {
        queuedEggs++;
    }

    public void UnregisterQueuedEgg()
    {
        queuedEggs = Mathf.Max(0, queuedEggs - 1);
    }

    // ------------------------
    // GETTERS
    // ------------------------

    public int GetTotalBees() => totalBees;

    public int GetBeeCount(Bee.BeeType type)
    {
        return beeCounts[type];
    }

    // ------------------------
    // SLEEP ZONES
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
        return (totalBees + queuedEggs) < GetHiveCapacity();
    }
}