#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// One-click tool: Tools → Plan Bee → Populate Tile Library
/// Loads autotile spritesheets from Assets/05_Tiles/Tilemap_02/
/// Each sheet has 16 sprites named autotiles_0..autotiles_15 (bitmask order).
/// </summary>
public static class HiveTileLibraryEditor
{
    const string SheetDir = "Assets/05_Tiles/Tilemap_02";

    // Map from HiveTileType → autotile sheet filename (without extension)
    static readonly (HiveTileType type, string sheet)[] TypeMap =
    {
        (HiveTileType.InsideHive, "autotiles_InsideHive"),
        (HiveTileType.Solid,      "autotiles_Solid"),
        (HiveTileType.Brood,      "autotiles_Brood"),
        (HiveTileType.Storage,    "autotiles_Storage"),
    };

    static Sprite LoadSheetSprite(string sheetBaseName, int mask)
    {
        string path = $"{SheetDir}/{sheetBaseName}.png";
        string spriteName = $"autotiles_{mask}";
        var all = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (var obj in all)
            if (obj is Sprite sp && sp.name == spriteName)
                return sp;
        return null;
    }

    [MenuItem("Tools/Plan Bee/Populate Tile Library")]
    static void Populate()
    {
        const string assetPath = "Assets/09_ScriptableObjects/HiveTileLibrary.asset";
        var lib = AssetDatabase.LoadAssetAtPath<HiveTileLibrary>(assetPath);
        if (lib == null)
        {
            lib = ScriptableObject.CreateInstance<HiveTileLibrary>();
            System.IO.Directory.CreateDirectory("Assets/09_ScriptableObjects");
            AssetDatabase.CreateAsset(lib, assetPath);
        }

        lib.sets = new HiveTileLibrary.TileSet[TypeMap.Length];

        for (int i = 0; i < TypeMap.Length; i++)
        {
            var (type, sheet) = TypeMap[i];
            var set = new HiveTileLibrary.TileSet { type = type };
            set.variants = new Sprite[16];

            int loaded = 0;
            for (int mask = 0; mask < 16; mask++)
            {
                var sp = LoadSheetSprite(sheet, mask);
                if (sp != null) { set.variants[mask] = sp; loaded++; }
                else Debug.LogWarning($"[HiveTileLibrary] Sprite not found: {sheet}.png / autotiles_{mask}");
            }

            // mask 0 = no connections = fully bordered = best standalone marker sprite
            set.markedSprite = set.variants[0];

            lib.sets[i] = set;
            Debug.Log($"[HiveTileLibrary] {type}: {loaded}/16 sprites loaded from {sheet}.png");
        }

        EditorUtility.SetDirty(lib);
        AssetDatabase.SaveAssets();
        Debug.Log($"[HiveTileLibrary] Done. Asset: {assetPath}");
        Selection.activeObject = lib;
    }
}
#endif
