#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Tools → Plan Bee → Populate Tile Library
///
/// Scans the per-type Border and Overlay PNG folders, parses filenames to
/// derive a 4-bit cardinal mask (T=1 B=2 L=4 R=8), and stores the resulting
/// sprite references into HiveTileLibrary. The PNG files themselves are not
/// modified — this only indexes them.
///
/// Filename conventions:
///   Border PNG : letters in the name = sides connected to same-type
///                (the floor / fully-connected tile is "r-t-l-b.png")
///   Overlay PNG: prefix "inside_" is stripped, then letters = sides that
///                face a different room type (overlay visible there)
///   Suffixes "-1", "-2", … (alternates) and "_hive" are stripped.
///   Any extra files in a slot are ignored (no random output).
/// </summary>
public static class RoomTileSetup
{
    const string LibraryPath = "Assets/05_Tiles/Settup/HiveTileLibrary.asset";

    static readonly (HiveTileType type, string borderFolder, string overlayFolder)[] Map =
    {
        (HiveTileType.InsideHive,
            "Assets/05_Tiles/InsideHive/Inside_Border",
            "Assets/05_Tiles/InsideHive/InsideHive_Overlay"),
        (HiveTileType.Brood,
            "Assets/05_Tiles/Brood/Brood_Border",
            "Assets/05_Tiles/Brood/Brood_Overlay"),
        (HiveTileType.Storage,
            "Assets/05_Tiles/Storage/Storage_Border",
            "Assets/05_Tiles/Storage/Storage_Overlay"),
        (HiveTileType.Hive,
            "Assets/05_Tiles/Hive/Hive_Borders",
            null),
    };

    [MenuItem("Tools/Plan Bee/Populate Tile Library")]
    static void PopulateLibrary()
    {
        var library = AssetDatabase.LoadAssetAtPath<HiveTileLibrary>(LibraryPath);
        if (library == null)
        {
            Debug.LogError($"[Plan Bee] HiveTileLibrary not found at {LibraryPath}");
            return;
        }

        var sets = new List<HiveTileLibrary.TileSet>();
        foreach (var (type, borderFolder, overlayFolder) in Map)
        {
            var borderSprites = new Sprite[16];
            foreach (var bf in borderFolder.Split('|'))
            {
                var loaded = LoadByMask(bf, isOverlay: false);
                for (int i = 0; i < 16; i++) if (loaded[i] != null) borderSprites[i] = loaded[i];
            }

            var overlaySprites = new Sprite[16];
            if (overlayFolder != null)
            {
                foreach (var of in overlayFolder.Split('|'))
                {
                    var loaded = LoadByMask(of, isOverlay: true);
                    for (int i = 0; i < 16; i++) if (loaded[i] != null) overlaySprites[i] = loaded[i];
                }
            }

            sets.Add(new HiveTileLibrary.TileSet
            {
                type    = type,
                border  = borderSprites,
                overlay = overlaySprites,
            });
        }
        library.sets = sets.ToArray();

        EditorUtility.SetDirty(library);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        int borderCount = 0, overlayCount = 0;
        foreach (var s in library.sets)
        {
            foreach (var sp in s.border)  if (sp != null) borderCount++;
            foreach (var sp in s.overlay) if (sp != null) overlayCount++;
        }
        Debug.Log($"[Plan Bee] HiveTileLibrary populated — " +
                  $"{library.sets.Length} types, {borderCount} borders, {overlayCount} overlays.");

        Selection.activeObject = library;
    }

    // -----------------------------------------------------------------------

    static Sprite[] LoadByMask(string folder, bool isOverlay)
    {
        var result = new Sprite[16];
        if (!Directory.Exists(folder))
        {
            Debug.LogWarning($"[Plan Bee] Folder missing: {folder}");
            return result;
        }

        foreach (var fullPath in Directory.GetFiles(folder, "*.png"))
        {
            string assetPath = fullPath.Replace('\\', '/');
            int idx = assetPath.IndexOf("Assets/");
            if (idx < 0) continue;
            assetPath = assetPath.Substring(idx);

            string name = Path.GetFileNameWithoutExtension(assetPath);

            // Strip alternate suffix (-1, -2, …)
            int dash = name.LastIndexOf('-');
            if (dash >= 0 && int.TryParse(name.Substring(dash + 1), out _))
                name = name.Substring(0, dash);

            // Strip _hive suffix on Hive border PNGs
            if (name.EndsWith("_hive"))
                name = name.Substring(0, name.Length - 5);

            if (isOverlay)
            {
                if (name == "inside")               name = "";
                else if (name.StartsWith("inside_")) name = name.Substring(7);
            }

            int mask = 0;
            
            // Check if name uses the exact string names (e.g. "Corner_BottomLeft")
            string[] borderNames = {
                "Isolated", "DeadEnd_Top", "DeadEnd_Bottom", "Tunnel_Vertical",
                "DeadEnd_Left", "Corner_BottomRight", "Corner_TopRight", "Wall_Right",
                "DeadEnd_Right", "Corner_BottomLeft", "Corner_TopLeft", "Wall_Left",
                "Tunnel_Horizontal", "Wall_Bottom", "Wall_Top", "Center"
            };
            
            int foundIndex = System.Array.IndexOf(borderNames, name);
            if (foundIndex >= 0)
            {
                mask = foundIndex;
            }
            else
            {
                // Fallback to t-b-l-r parsing for Hive_Outside_Border
                if (name.Contains("t")) mask |= 1;
                if (name.Contains("b")) mask |= 2;
                if (name.Contains("l")) mask |= 4;
                if (name.Contains("r")) mask |= 8;
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null) continue;

            // First match wins — explicit, no randomness.
            if (result[mask] == null) result[mask] = sprite;
        }
        return result;
    }
}
#endif
