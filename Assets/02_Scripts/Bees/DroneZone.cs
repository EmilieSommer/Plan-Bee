using UnityEngine;
using System.Collections.Generic;

public class DroneZone : MonoBehaviour
{
    public static List<DroneZone> allZones = new List<DroneZone>();

    private void OnEnable()
    {
        if (!allZones.Contains(this))
            allZones.Add(this);
    }

    private void OnDisable()
    {
        allZones.Remove(this);
    }
}