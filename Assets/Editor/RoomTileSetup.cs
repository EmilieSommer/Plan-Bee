#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class RoomTileSetup
{
    static readonly int THIS = RuleTile.TilingRuleOutput.Neighbor.This;
    static readonly int NOT  = RuleTile.TilingRuleOutput.Neighbor.NotThis;

    [MenuItem("Tools/Plan Bee/Setup All Room Tiles")]
    static void SetupAll()
    {
        // Border tiles — outer edge where zone meets empty space
        SetupBorder("Assets/05_Tiles/InsideHive/Inside_Border",
                    "Assets/05_Tiles/InsideHive/InsideHive_Border_RuleTile.asset");
        SetupBorder("Assets/05_Tiles/Brood/Brood_Border",
                    "Assets/05_Tiles/Brood/Brood_Border_RuleTile.asset");
        SetupBorder("Assets/05_Tiles/Drone/Drone_Border",
                    "Assets/05_Tiles/Drone/Drone_Border_RuleTile.asset");
        SetupBorder("Assets/05_Tiles/Storage/Storage_Border",
                    "Assets/05_Tiles/Storage/Storage_Border_RuleTile.asset");
        SetupBorder("Assets/05_Tiles/Hive/Hive_Borders",
                    "Assets/05_Tiles/Hive/Hive_Border_RuleTile.asset");

        // Overlay tiles — transparent edges where two zone types meet
        SetupOverlay("Assets/05_Tiles/InsideHive/InsideHive_Overlay",
                     "Assets/05_Tiles/InsideHive/InsideHive_Overlay_RuleTile.asset");
        SetupOverlay("Assets/05_Tiles/Brood/Brood_Overlay",
                     "Assets/05_Tiles/Brood/Brood_Overlay_RuleTile.asset");
        SetupOverlay("Assets/05_Tiles/Drone/Drone_Overlay",
                     "Assets/05_Tiles/Drone/Drone_Overlay_RuleTile.asset");
        SetupOverlay("Assets/05_Tiles/Storage/Storage_Overlay",
                     "Assets/05_Tiles/Storage/Storage_Overlay_RuleTile.asset");
        SetupOverlay("Assets/05_Tiles/Storage/Storage_Inside",
                     "Assets/05_Tiles/Storage/Storage_InsideEdge_RuleTile.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Plan Bee] All room RuleTiles created.");
    }

    // -----------------------------------------------------------------------

    static void SetupBorder(string folder, string output)
    {
        if (!Directory.Exists(folder)) { Debug.LogWarning($"[Plan Bee] Missing: {folder}"); return; }
        ConfigureSprites(folder);
        AssetDatabase.Refresh();

        var tile = ScriptableObject.CreateInstance<RuleTile>();
        tile.m_TilingRules = new List<RuleTile.TilingRule>();

        var groups = ScanFolder(folder, isOverlay: false);

        var floorKey = (true, true, true, true);
        if (groups.TryGetValue(floorKey, out var floor))
        {
            tile.m_DefaultSprite = floor[0];
            groups.Remove(floorKey);
        }

        foreach (var kv in groups)
        {
            var (t, b, l, r) = kv.Key;
            tile.m_TilingRules.Add(MakeRule(kv.Value,
                t ? THIS : NOT, b ? THIS : NOT, l ? THIS : NOT, r ? THIS : NOT));
        }

        Save(tile, output);
        Debug.Log($"[Plan Bee] Border {Path.GetFileName(folder)}: {tile.m_TilingRules.Count} rules");
    }

    static void SetupOverlay(string folder, string output)
    {
        if (!Directory.Exists(folder)) { Debug.LogWarning($"[Plan Bee] Missing: {folder}"); return; }
        ConfigureSprites(folder);
        AssetDatabase.Refresh();

        var tile = ScriptableObject.CreateInstance<RuleTile>();
        tile.m_TilingRules = new List<RuleTile.TilingRule>();

        var groups = ScanFolder(folder, isOverlay: true);

        var transparentKey = (false, false, false, false);
        if (groups.TryGetValue(transparentKey, out var transparent))
        {
            tile.m_DefaultSprite = transparent[0];
            groups.Remove(transparentKey);
        }

        foreach (var kv in groups)
        {
            var (t, b, l, r) = kv.Key;
            tile.m_TilingRules.Add(MakeRule(kv.Value,
                t ? NOT : THIS, b ? NOT : THIS, l ? NOT : THIS, r ? NOT : THIS));
        }

        Save(tile, output);
        Debug.Log($"[Plan Bee] Overlay {Path.GetFileName(folder)}: {tile.m_TilingRules.Count} rules");
    }

    // -----------------------------------------------------------------------

    static Dictionary<(bool t, bool b, bool l, bool r), List<Sprite>> ScanFolder(
        string folder, bool isOverlay)
    {
        var groups = new Dictionary<(bool, bool, bool, bool), List<Sprite>>();

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

            if (isOverlay)
            {
                if (name == "inside")               name = "";
                else if (name.StartsWith("inside_")) name = name.Substring(7);
            }

            bool t = name.Contains('t');
            bool b = name.Contains('b');
            bool l = name.Contains('l');
            bool r = name.Contains('r');

            var key = (t, b, l, r);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null) continue;

            if (!groups.TryGetValue(key, out var list))
                groups[key] = list = new List<Sprite>();
            list.Add(sprite);
        }

        return groups;
    }

    static RuleTile.TilingRule MakeRule(List<Sprite> sprites, int top, int bot, int left, int right)
    {
        return new RuleTile.TilingRule
        {
            m_Sprites           = new Sprite[] { sprites[0] },
            m_Output            = RuleTile.TilingRuleOutput.OutputSprite.Single,
            m_NeighborPositions = new List<Vector3Int>
                                    { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right },
            m_Neighbors         = new List<int> { top, bot, left, right },
        };
    }

    static void ConfigureSprites(string folder)
    {
        foreach (var fullPath in Directory.GetFiles(folder, "*.png"))
        {
            string assetPath = fullPath.Replace('\\', '/');
            int idx = assetPath.IndexOf("Assets/");
            if (idx < 0) continue;
            assetPath = assetPath.Substring(idx);

            var imp = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (imp == null) continue;

            imp.textureType         = TextureImporterType.Sprite;
            imp.spriteImportMode    = SpriteImportMode.Single;
            imp.filterMode          = FilterMode.Point;
            imp.textureCompression  = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled       = false;
            imp.spritePixelsPerUnit = 32f;
            imp.SaveAndReimport();
        }
    }

    static void Save(RuleTile tile, string path)
    {
        if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(tile, path);
    }
}
#endif
