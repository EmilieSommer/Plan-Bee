#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Generates Tile assets from every sprite in Assets/05_Tiles/Test_Tiles/.
/// Run via: Tools → Plan Bee → Create Test Tiles
/// Output goes to Assets/05_Tiles/Test_Tiles/Tiles/ (one .tile per sprite).
/// After running, open Window → 2D → Tile Palette and drag the folder in.
/// </summary>
public static class TestTileCreator
{
    const string SourceFolder = "Assets/05_Tiles/Test_Tiles";
    const string OutputFolder  = "Assets/05_Tiles/Test_Tiles/Tiles";

    [MenuItem("Tools/Plan Bee/Create Test Tiles")]
    static void CreateTiles()
    {
        Directory.CreateDirectory(OutputFolder);
        AssetDatabase.Refresh();

        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { SourceFolder });
        int created = 0;
        int skipped = 0;

        foreach (string guid in guids)
        {
            string spritePath = AssetDatabase.GUIDToAssetPath(guid);

            // Skip anything that's already in the Tiles output folder
            if (spritePath.StartsWith(OutputFolder))
                continue;

            // Load all sprites from this texture (handles sprite sheets)
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(spritePath);
            foreach (Object asset in assets)
            {
                if (asset is not Sprite sprite) continue;

                string tilePath = $"{OutputFolder}/{sprite.name}.tile";

                // Don't overwrite existing tiles
                if (File.Exists(tilePath))
                {
                    skipped++;
                    continue;
                }

                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;
                tile.colliderType = Tile.ColliderType.None;

                AssetDatabase.CreateAsset(tile, tilePath);
                created++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[TestTileCreator] Done — {created} tiles created, {skipped} already existed. Output: {OutputFolder}");
        EditorUtility.RevealInFinder(OutputFolder);
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(OutputFolder);
    }
}
#endif
