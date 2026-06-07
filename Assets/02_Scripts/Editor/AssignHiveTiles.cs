using UnityEngine;
using UnityEditor;
using System.IO;

public class AssignHiveTiles
{
    private static readonly string[] BorderNames = new string[16]
    {
        "Isolated",             // 0: None
        "DeadEnd_Bottom",       // 1: Top
        "DeadEnd_Top",          // 2: Bottom
        "Tunnel_Vertical",      // 3: Top, Bottom
        "DeadEnd_Right",        // 4: Left
        "Corner_BottomRight",   // 5: Top, Left
        "Corner_TopRight",      // 6: Bottom, Left
        "Wall_Right",           // 7: Top, Bottom, Left
        "DeadEnd_Left",         // 8: Right
        "Corner_BottomLeft",    // 9: Top, Right
        "Corner_TopLeft",       // 10: Bottom, Right
        "Wall_Left",            // 11: Top, Bottom, Right
        "Tunnel_Horizontal",    // 12: Left, Right
        "Wall_Bottom",          // 13: Top, Left, Right
        "Wall_Top",             // 14: Bottom, Left, Right
        "Center"                // 15: All
    };

    [MenuItem("Plan Bee/Auto-Assign Hive Tiles")]
    public static void AssignTiles()
    {
        string libraryPath = "Assets/02_Scripts/Hive/HiveTileLibrary.asset";
        HiveTileLibrary lib = AssetDatabase.LoadAssetAtPath<HiveTileLibrary>(libraryPath);
        
        if (lib == null)
        {
            // Try to find it if it's elsewhere
            string[] guids = AssetDatabase.FindAssets("t:HiveTileLibrary");
            if (guids.Length > 0)
                lib = AssetDatabase.LoadAssetAtPath<HiveTileLibrary>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        if (lib == null)
        {
            Debug.LogError("Could not find HiveTileLibrary.asset");
            return;
        }

        Undo.RecordObject(lib, "Auto Assign Hive Tiles");

        for (int i = 0; i < lib.sets.Length; i++)
        {
            var set = lib.sets[i];
            string typeName = set.type.ToString();
            
            // Map InsideHive to Inside
            if (typeName == "InsideHive") typeName = "Inside";
            if (typeName == "Hive") typeName = "Hive"; // Just to be explicit
            
            // Different room folders
            string folderPrefix = typeName == "Inside" ? "InsideHive" : typeName;
            
            // 1. Borders
            string borderDir = $"Assets/05_Tiles/{folderPrefix}/{typeName}_Border"; // e.g. Brood_Border
            if (typeName == "Hive") borderDir = "Assets/05_Tiles/Hive/Hive_Borders"; // Special case for Hive
            if (typeName == "Drone") borderDir = "Assets/05_Tiles/Drone/Drone_Border";
            if (typeName == "Storage") borderDir = "Assets/05_Tiles/Storage/Storage_Border";
            if (typeName == "Brood") borderDir = "Assets/05_Tiles/Brood/Brood_Border";

            for (int mask = 0; mask < 16; mask++)
            {
                string expectedName = BorderNames[mask] + ".png";
                string fullPath = $"{borderDir}/{expectedName}";
                Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
                if (s == null) {
                    // Try without prefix if it's Center, etc.
                    string altPath = $"{borderDir}/{BorderNames[mask]}.png";
                    s = AssetDatabase.LoadAssetAtPath<Sprite>(altPath);
                }
                
                if (s != null)
                {
                    set.border[mask] = s;
                    Debug.Log($"Assigned border [{typeName}] mask {mask}: {s.name}");
                }
                else
                {
                    Debug.LogWarning($"Missing border sprite for {typeName}: {expectedName} (Checked {fullPath})");
                }
            }

            // 2. Overlays
            if (set.type != HiveTileType.Hive)
            {
                string overlayDir = $"Assets/05_Tiles/{folderPrefix}/{folderPrefix}_Overlay";
                
                for (int mask = 0; mask < 16; mask++)
                {
                    string baseName = BorderNames[mask];
                    if (baseName == "Center") baseName = "inside";
                    else baseName = "inside_" + baseName;
                    
                    string expectedName = baseName + ".png";
                    string fullPath = $"{overlayDir}/{expectedName}";
                    Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
                    
                    if (s != null)
                    {
                        set.overlay[mask] = s;
                    }
                    else
                    {
                        Debug.LogWarning($"Missing overlay sprite for {typeName}: {expectedName} (Checked {fullPath})");
                    }
                }
            }
        }

        EditorUtility.SetDirty(lib);
        AssetDatabase.SaveAssets();
        Debug.Log("Successfully auto-assigned all available Hive sprites!");
    }
}
