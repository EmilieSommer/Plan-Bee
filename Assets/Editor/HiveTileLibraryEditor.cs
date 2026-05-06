#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// One-click tool: Tools → Plan Bee → Populate Tile Library
/// Scans Assets/05_Tiles/ and assigns every sprite to the correct slot
/// in a HiveTileLibrary ScriptableObject based on filename.
/// </summary>
public static class HiveTileLibraryEditor
{
    // Map from filename suffix → bitmask index  (T=8  B=4  L=2  R=1)
    static readonly (string suffix, int mask)[] VariantMap =
    {
        ("_01",      0),   // no connections
        ("_02_T",    8),   // top
        ("_03_B",    4),   // bottom
        ("_04_L",    2),   // left
        ("_05_R",    1),   // right
        ("_06_TB",  12),   // top + bottom
        ("_07_LR",   3),   // left + right
        ("_08_TL",  10),   // top + left
        ("_09_TR",   9),   // top + right
        ("_10_BL",   6),   // bottom + left
        ("_11_BR",   5),   // bottom + right
        ("_12_TBL", 14),   // top + bottom + left
        ("_13_TBR", 13),   // top + bottom + right
        ("_14_TLR", 11),   // top + left + right
        ("_15_BLR",  7),   // bottom + left + right
        ("_16_TBLR",15),   // all
    };

    // Map from HiveTileType → sprite name prefix in Assets/05_Tiles/
    static readonly (HiveTileType type, string prefix, string markedPrefix)[] TypeMap =
    {
        (HiveTileType.InsideHive, "Inside_Hive", "MarkedForRoom"),
        (HiveTileType.Solid,      "Solid",        "MarkedForSolid"),
        (HiveTileType.Brood,      "Brood",        "MarkedForRoom"),
        (HiveTileType.Storage,    "Storage",      "MarkedForRoom"),
    };

    [MenuItem("Tools/Plan Bee/Populate Tile Library")]
    static void Populate()
    {
        // Find or create the library asset
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
            var (type, prefix, markedPrefix) = TypeMap[i];
            var set = new HiveTileLibrary.TileSet { type = type };
            set.variants = new Sprite[16];

            foreach (var (suffix, mask) in VariantMap)
            {
                string name = $"Assets/05_Tiles/{prefix}{suffix}.png";
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(name);
                if (sp != null) set.variants[mask] = sp;
                else Debug.LogWarning($"[HiveTileLibrary] Sprite not found: {name}");
            }

            // Marked sprite — use bitmask 0 (all-walls closed) of the marked set
            string markedPath = $"Assets/05_Tiles/{markedPrefix}_01.png";
            set.markedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(markedPath);

            lib.sets[i] = set;
        }

        EditorUtility.SetDirty(lib);
        AssetDatabase.SaveAssets();
        Debug.Log($"[HiveTileLibrary] Populated {TypeMap.Length} tile sets. Asset: {assetPath}");
        Selection.activeObject = lib;
    }
}
#endif
