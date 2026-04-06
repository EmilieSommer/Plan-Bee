using System.Collections.Generic;
using UnityEngine;

public class ZoneManager : MonoBehaviour
{
    public static ZoneManager Instance;

    private Dictionary<Bee.BeeType, List<Zone>> zonesByType = new();
    private Dictionary<Bee.BeeType, int> beeCounts = new();

    private void Awake()
    {
        Instance = this;

        foreach (Bee.BeeType type in System.Enum.GetValues(typeof(Bee.BeeType)))
        {
            zonesByType[type] = new List<Zone>();
            beeCounts[type] = 0;
        }
    }

    // ------------------------
    // ZONE REGISTRATION
    // ------------------------

    public void RegisterZone(Zone zone)
    {
        zonesByType[zone.zoneType].Add(zone);
    }

    public void UnregisterZone(Zone zone)
    {
        zonesByType[zone.zoneType].Remove(zone);
    }

    // ------------------------
    // BEE TRACKING
    // ------------------------

    public void RegisterBee(Bee bee)
    {
        beeCounts[bee.beeType]++;
    }

    public void UnregisterBee(Bee bee)
    {
        beeCounts[bee.beeType]--;
    }

    public int GetBeeCount(Bee.BeeType type)
    {
        return beeCounts[type];
    }

    // ------------------------
    // LIMIT CHECK
    // ------------------------

    public bool CanSpawnBee(Bee.BeeType type)
    {
        int max = GetZoneCapacity(type);
        return beeCounts[type] < max;
    }

    public int GetZoneCapacity(Bee.BeeType type)
    {
        int capacity = 0;

        foreach (var zone in zonesByType[type])
        {
            capacity += zone.capacity;
        }

        return capacity;
    }

    public Zone GetClosestZone(Bee.BeeType type, Vector2 position)
    {
        if (!zonesByType.ContainsKey(type))
            return null;

        Zone bestZone = null;
        float bestScore = Mathf.Infinity;

        foreach (Zone zone in zonesByType[type])
        {
            if (zone == null) continue;

            // ❌ Skip full zones
            if (zone.IsFull()) continue;

            float distance = Vector2.Distance(position, zone.transform.position);

            // 🔥 IMPORTANT: include how full the zone is
            float fillPenalty = zone.GetFillRatio() * 10f;

            // 🔥 FINAL SCORE
            float score = distance + fillPenalty;

            if (score < bestScore)
            {
                bestScore = score;
                bestZone = zone;
            }
        }

        return bestZone;
    }
}