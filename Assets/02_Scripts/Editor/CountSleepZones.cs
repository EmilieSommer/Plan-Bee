using UnityEditor;
using UnityEngine;

public static class CountSleepZones
{
    public static void Execute()
    {
        SleepZone[] zones = Object.FindObjectsOfType<SleepZone>(true);
        int total = 0;
        foreach(var z in zones) total += z.capacityPerZone;
        Debug.Log($"Found {zones.Length} SleepZones. Total capacity = {total}");
        EditorApplication.Exit(0);
    }
}
