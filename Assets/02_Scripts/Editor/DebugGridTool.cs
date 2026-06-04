#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DebugGridTool : MonoBehaviour
{
    [MenuItem("Plan Bee/9. Debug Grid")]
    public static void DebugGrid()
    {
        GameObject grid = GameObject.Find("Grid");
        if (grid == null)
        {
            Debug.LogError("No GameObject named 'Grid' found!");
            return;
        }

        string report = "=== GRID DEBUG REPORT ===\n";
        
        HiveVisuals visuals = grid.GetComponent<HiveVisuals>();
        if (visuals != null)
        {
            report += "HiveVisuals is attached.\n";
            report += "BuiltTilemap: " + (visuals.BuiltTilemap != null ? visuals.BuiltTilemap.name : "NULL") + "\n";
        }
        else
        {
            report += "NO HiveVisuals script attached to Grid!\n";
        }

        Tilemap[] allMaps = grid.GetComponentsInChildren<Tilemap>();
        report += "\nFound " + allMaps.Length + " Tilemaps under Grid:\n";
        foreach (var map in allMaps)
        {
            int tileCount = 0;
            foreach (var pos in map.cellBounds.allPositionsWithin)
            {
                if (map.HasTile(pos)) tileCount++;
            }
            report += "- " + map.name + " (" + tileCount + " tiles)\n";
        }

        Debug.Log(report);
        
        // Let's also check if any GameObject is named "Tilemap" and is disabled
        Transform tMap = grid.transform.Find("Tilemap");
        if (tMap != null)
        {
            report += "\nTilemap GameObject is active: " + tMap.gameObject.activeSelf;
            TilemapRenderer tr = tMap.GetComponent<TilemapRenderer>();
            report += "\nTilemapRenderer is enabled: " + (tr != null && tr.enabled);
        }

        // Save report to file so AI can read it
        System.IO.File.WriteAllText("DebugReport.txt", report);
        Debug.Log("Saved report to DebugReport.txt");
    }
}
#endif
