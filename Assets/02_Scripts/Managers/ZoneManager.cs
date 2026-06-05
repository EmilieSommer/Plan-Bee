using System.Collections.Generic;
using UnityEngine;

public class ZoneManager : MonoBehaviour
{
    public static ZoneManager Instance;

    private Dictionary<Bee.BeeType, List<Zone>> zonesByType = new();
    private Dictionary<Bee.BeeType, int> beeCounts = new();
    private Dictionary<Bee.BeeType, Dictionary<Zone, int>> zoneAssignments = new();

    private void Awake()
    {
        Instance = this;

        foreach (Bee.BeeType type in System.Enum.GetValues(typeof(Bee.BeeType)))
        {
            zonesByType[type] = new List<Zone>();
            beeCounts[type] = 0;
            zoneAssignments[type] = new Dictionary<Zone, int>();
        }
    }

    // ------------------------
    // ZONE REGISTRATION
    // ------------------------

    public void RegisterZone(Zone zone)
    {
        zonesByType[zone.zoneType].Add(zone);

        if (!zoneAssignments[zone.zoneType].ContainsKey(zone))
            zoneAssignments[zone.zoneType][zone] = 0;
    }

    public void UnregisterZone(Zone zone)
    {
        zonesByType[zone.zoneType].Remove(zone);

        if (zoneAssignments[zone.zoneType].ContainsKey(zone))
            zoneAssignments[zone.zoneType].Remove(zone);
    }

    public bool HasZone(Bee.BeeType type)
    {
        return zonesByType.ContainsKey(type) && zonesByType[type].Count > 0;
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
        beeCounts[bee.beeType] = Mathf.Max(0, beeCounts[bee.beeType] - 1);
    }

    public int GetBeeCount(Bee.BeeType type)
    {
        return beeCounts[type];
    }

    // ------------------------
    // ASSIGNMENT TRACKING
    // ------------------------

    public void RegisterZoneAssignment(Bee.BeeType type, Zone zone)
    {
        if (!zoneAssignments[type].ContainsKey(zone))
            zoneAssignments[type][zone] = 0;

        zoneAssignments[type][zone]++;
    }

    // ------------------------
    // CAPACITY
    // ------------------------

    public int GetZoneCapacity(Bee.BeeType type)
    {
        int capacity = 0;

        foreach (var zone in zonesByType[type])
        {
            if (zone == null) continue;

            foreach (var limit in zone.limits)
            {
                if (limit.type == type)
                    capacity += limit.capacity;
            }
        }

        return capacity;
    }

    public bool CanSpawnBee(Bee.BeeType type)
    {
        return beeCounts[type] < GetZoneCapacity(type);
    }

    // ------------------------
    // SMART ZONE SELECTION
    // ------------------------

    public Zone GetClosestZone(Bee.BeeType type, Vector2 position)
    {
        if (!zonesByType.ContainsKey(type))
            return null;

        Zone bestZone = null;
        float bestScore = Mathf.Infinity;

        foreach (Zone zone in zonesByType[type])
        {
            if (zone == null) continue;
            if (!zone.CanAccept(type)) continue;

            float distance = Vector2.Distance(position, zone.transform.position);
            float fillPenalty = zone.GetFillRatio(type) * 15f;

            int assigned = zoneAssignments[type].ContainsKey(zone)
                ? zoneAssignments[type][zone]
                : 0;

            float spreadPenalty = assigned * 2f;

            float score = distance + fillPenalty + spreadPenalty;

            if (score < bestScore)
            {
                bestScore = score;
                bestZone = zone;
            }
        }

        if (bestZone != null)
            RegisterZoneAssignment(type, bestZone);

        return bestZone;
    }

    public Zone GetAlternativeZone(Bee.BeeType type, Vector2 position, Zone excludeZone)
    {
        Zone bestZone = null;
        float bestScore = Mathf.Infinity;

        foreach (Zone zone in zonesByType[type])
        {
            if (zone == null) continue;
            if (zone == excludeZone) continue;
            if (!zone.CanAccept(type)) continue;

            float distance = Vector2.Distance(position, zone.transform.position);
            float fillPenalty = zone.GetFillRatio(type) * 15f;

            if (distance + fillPenalty < bestScore)
            {
                bestScore = distance + fillPenalty;
                bestZone = zone;
            }
        }

        return bestZone;
    }
}