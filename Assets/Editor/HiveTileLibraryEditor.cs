#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Tools → Plan Bee → Populate Tile Library
/// Assigns RuleTile assets to each HiveTileType slot in HiveTileLibrary.
/// Run "Setup All Room Tiles" first to generate the RuleTile assets.
/// </summary>
public static class HiveTileLibraryEditor
{
    // HiveTileType → path to the border RuleTile asset created by RoomTileSetup
    static readonly (HiveTileType type, string ruleTilePath)[] TypeMap =
    {
        (HiveTileType.InsideHive, "Assets/05_Tiles/InsideHive/InsideHive_Border_RuleTile.asset"),
        (HiveTileType.Brood,      "Assets/05_Tiles/Brood/Brood_Border_RuleTile.asset"),
        (HiveTileType.Storage,    "Assets/05_Tiles/Storage/Storage_Border_RuleTile.asset"),
        (HiveTileType.Solid,      null),
    };

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
            var (type, path) = TypeMap[i];
            var set = new HiveTileLibrary.TileSet { type = type };

            if (path != null)
            {
                set.builtTile = AssetDatabase.LoadAssetAtPath<TileBase>(path);
                if (set.builtTile == null)
                    Debug.LogWarning($"[HiveTileLibrary] RuleTile not found: {path} — run Setup All Room Tiles first.");
            }

            lib.sets[i] = set;
        }

        EditorUtility.SetDirty(lib);
        AssetDatabase.SaveAssets();
        Debug.Log($"[HiveTileLibrary] Populated {TypeMap.Length} tile sets → {assetPath}");
        Selection.activeObject = lib;
    }
}
#endif
