#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class InsideHive02TileSetup
{
    const string Folder     = "Assets/05_Tiles/InsideHive/Inside_Hive_02/Variants/";
    const string OutputPath = "Assets/05_Tiles/InsideHive/Inside_Hive_02/InsideHive02_RuleTile.asset";

    // (filename, width, height, pivot.x, pivot.y)
    static readonly (string name, int w, int h, float px, float py)[] SpriteInfo =
    {
        ("IH2_floor.png",      32, 32, 0.5f,       0.5f      ),
        ("IH2_T.png",          32, 36, 0.5f,       16f/36f   ),
        ("IH2_B.png",          32, 36, 0.5f,       20f/36f   ),
        ("IH2_L.png",          36, 32, 20f/36f,    0.5f      ),
        ("IH2_R.png",          36, 32, 16f/36f,    0.5f      ),
        ("IH2_TL.png",         36, 36, 20f/36f,    16f/36f   ),
        ("IH2_TR.png",         36, 36, 16f/36f,    16f/36f   ),
        ("IH2_BL.png",         36, 36, 20f/36f,    20f/36f   ),
        ("IH2_BR.png",         36, 36, 16f/36f,    20f/36f   ),
        ("IH2_TB.png",         36, 36, 16f/36f,    0.5f      ),
        ("IH2_LR.png",         36, 36, 0.5f,       0.5f      ),
        ("IH2_TBL.png",        36, 32, 20f/36f,    0.5f      ),
        ("IH2_TBR.png",        36, 32, 16f/36f,    0.5f      ),
        ("IH2_TLR.png",        32, 36, 0.5f,       16f/36f   ),
        ("IH2_BLR.png",        32, 36, 0.5f,       20f/36f   ),
        ("IH2_TBLR.png",       36, 36, 0.5f,       0.5f      ),
        ("IH2_EndTop_L.png",   36, 36, 20f/36f,    16f/36f   ),
        ("IH2_EndTop_R.png",   36, 36, 16f/36f,    16f/36f   ),
        ("IH2_EndBot_L.png",   36, 36, 20f/36f,    20f/36f   ),
        ("IH2_EndBot_R.png",   36, 36, 16f/36f,    20f/36f   ),
        ("IH2_EndLeft_T.png",  36, 36, 20f/36f,    16f/36f   ),
        ("IH2_EndLeft_B.png",  36, 36, 20f/36f,    20f/36f   ),
        ("IH2_EndRight_T.png", 36, 36, 16f/36f,    16f/36f   ),
        ("IH2_EndRight_B.png", 36, 36, 16f/36f,    20f/36f   ),
    };

    [MenuItem("Tools/Plan Bee/Setup InsideHive02 Rule Tile")]
    static void Setup()
    {
        foreach (var info in SpriteInfo)
            ConfigureSprite(Folder + info.name, info.w, info.h, info.px, info.py);

        AssetDatabase.Refresh();

        var tile = ScriptableObject.CreateInstance<RuleTile>();
        tile.m_TilingRules = new List<RuleTile.TilingRule>();

        int Y = RuleTile.TilingRuleOutput.Neighbor.This;
        int N = RuleTile.TilingRuleOutput.Neighbor.NotThis;

        Sprite S(string name) =>
            AssetDatabase.LoadAssetAtPath<Sprite>(Folder + name);

        void AddRule(Sprite sprite, int top, int bot, int left, int right,
                     Vector3Int diagPos = default, int diagVal = 0)
        {
            var rule = new RuleTile.TilingRule
            {
                m_Sprites           = new[] { sprite },
                m_Output            = RuleTile.TilingRuleOutput.OutputSprite.Single,
                m_NeighborPositions = new List<Vector3Int> { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right },
                m_Neighbors         = new List<int>        { top,           bot,             left,            right            },
            };
            if (diagVal != 0)
            {
                rule.m_NeighborPositions.Add(diagPos);
                rule.m_Neighbors.Add(diagVal);
            }
            tile.m_TilingRules.Add(rule);
        }

        // End-cap rules first (more specific — cardinal + one diagonal condition)
        AddRule(S("IH2_EndTop_L.png"),   N, Y, Y, Y, new Vector3Int(-1,  1, 0), N);
        AddRule(S("IH2_EndTop_R.png"),   N, Y, Y, Y, new Vector3Int( 1,  1, 0), N);
        AddRule(S("IH2_EndBot_L.png"),   Y, N, Y, Y, new Vector3Int(-1, -1, 0), N);
        AddRule(S("IH2_EndBot_R.png"),   Y, N, Y, Y, new Vector3Int( 1, -1, 0), N);
        AddRule(S("IH2_EndLeft_T.png"),  Y, Y, N, Y, new Vector3Int(-1,  1, 0), N);
        AddRule(S("IH2_EndLeft_B.png"),  Y, Y, N, Y, new Vector3Int(-1, -1, 0), N);
        AddRule(S("IH2_EndRight_T.png"), Y, Y, Y, N, new Vector3Int( 1,  1, 0), N);
        AddRule(S("IH2_EndRight_B.png"), Y, Y, Y, N, new Vector3Int( 1, -1, 0), N);

        // Cardinal rules (T/B/L/R = which NEIGHBORS are present, not which borders are visible)
        AddRule(S("IH2_TL.png"),   N, Y, N, Y);
        AddRule(S("IH2_TR.png"),   N, Y, Y, N);
        AddRule(S("IH2_BL.png"),   Y, N, N, Y);
        AddRule(S("IH2_BR.png"),   Y, N, Y, N);
        AddRule(S("IH2_TB.png"),   N, N, Y, Y);
        AddRule(S("IH2_LR.png"),   Y, Y, N, N);
        AddRule(S("IH2_TBL.png"),  N, N, N, Y);
        AddRule(S("IH2_TBR.png"),  N, N, Y, N);
        AddRule(S("IH2_TLR.png"),  N, Y, N, N);
        AddRule(S("IH2_BLR.png"),  Y, N, N, N);
        AddRule(S("IH2_TBLR.png"), N, N, N, N);
        AddRule(S("IH2_T.png"),    N, Y, Y, Y);
        AddRule(S("IH2_B.png"),    Y, N, Y, Y);
        AddRule(S("IH2_L.png"),    Y, Y, N, Y);
        AddRule(S("IH2_R.png"),    Y, Y, Y, N);
        AddRule(S("IH2_floor.png"),Y, Y, Y, Y);

        AssetDatabase.CreateAsset(tile, OutputPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = tile;
        Debug.Log($"[Plan Bee] InsideHive02 RuleTile created at {OutputPath} with {tile.m_TilingRules.Count} rules.");
    }

    static void ConfigureSprite(string path, int w, int h, float pivotX, float pivotY)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) { Debug.LogWarning($"[Plan Bee] Importer not found: {path}"); return; }

        importer.textureType            = TextureImporterType.Sprite;
        importer.spriteImportMode       = SpriteImportMode.Single;
        importer.filterMode             = FilterMode.Point;
        importer.textureCompression     = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled          = false;
        importer.spritePixelsPerUnit    = 32f;
        importer.spritePivot            = new Vector2(pivotX, pivotY);
        importer.spriteAlignment        = (int)SpriteAlignment.Custom;

        importer.SaveAndReimport();
    }
}
#endif
