using UnityEngine;
using System.Collections.Generic;

public class DroneZone : MonoBehaviour
{
    public static List<DroneZone> allZones = new List<DroneZone>();

    public bool isBuilt = false;

    private void OnEnable()
    {
        // Only register if already built
        if (isBuilt && !allZones.Contains(this))
            allZones.Add(this);
    }

    private void OnDisable()
    {
        allZones.Remove(this);
    }

    // Call this when construction finishes
    public void SetBuilt()
    {
        isBuilt = true;

        if (!allZones.Contains(this))
            allZones.Add(this);
    }

    // Optional helper (safer checks elsewhere)
    public bool IsAvailable()
    {
        return isBuilt;
    }
}