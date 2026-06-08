using UnityEditor;
using UnityEngine;

public static class DebugLibrary
{
    public static void Execute()
    {
        var grid = Object.FindAnyObjectByType<HiveGrid>();
        if (grid != null)
        {
            var lib = grid.GetComponent<HiveVisuals>()?.Library;
            if (lib != null)
            {
                foreach(var set in lib.sets)
                {
                    if (set.type == HiveTileType.InsideHive)
                    {
                        Debug.Log("InsideHive Overlay 0: " + (set.overlay[0] != null ? set.overlay[0].name : "NULL"));
                        Debug.Log("InsideHive Overlay 1: " + (set.overlay[1] != null ? set.overlay[1].name : "NULL"));
                    }
                }
            }
        }
    }
}
