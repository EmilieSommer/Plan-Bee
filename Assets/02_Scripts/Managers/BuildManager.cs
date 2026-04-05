using UnityEngine;
using System.Collections.Generic;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    private List<BuildZone> activeZones = new List<BuildZone>();

    private void Awake()
    {
        Instance = this;    
    }

    public void RegisterZone(BuildZone zone)
    {
        if (!activeZones.Contains(zone))
            activeZones.Add(zone);
    }

    public void UnregisterZone(BuildZone zone)
    {
        activeZones.Remove(zone);
    }

    public BuildZone GetNextZone()
    {
        foreach (var zone in activeZones)
        {
            if (zone != null)
                return zone;
        }

        return null;
    }
}