#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class InsideHiveTileSetup
{
    const string Folder     = "Assets/05_Tiles/InsideHive/";
    const string OutputPath = "Assets/05_Tiles/InsideHive/InsideHive_RuleTile.asset";

    [MenuItem("Tools/Plan Bee/Setup Inside Hive Rule Tile")]
    static void Setup()
    {
        string[] allFiles =
        {
            "Inside_Hive_01.png", "Inside_Hive_02_T.png", "Inside_Hive_03_B.png",
            "Inside_Hive_04_L.png", "Inside_Hive_05_R.png", "Inside_Hive_06_TB.png",
            "Inside_Hive_07_LR.png", "Inside_Hive_08_TL.png", "Inside_Hive_09_TR.png",
            "Inside_Hive_10_BL.png", "Inside_Hive_11_BR.png", "Inside_Hive_12_TBL.png",
            "Inside_Hive_13_TBR.png", "Inside_Hive_14_TLR.png", "Inside_Hive_15_BLR.png",
            "Inside_Hive_16_TBLR.png",
            "Inside_Hive_Inner_TL.png", "Inside_Hive_Inner_TR.png",
            "Inside_Hive_Inner_BL.png", "Inside_Hive_Inner_BR.png",
        };
        foreach (var f in allFiles) EnsureSprite(Folder + f);
        AssetDatabase.Refresh();

        Sprite Load(string name)
        {
            var sp = AssetDatabase.LoadAssetAtPath<Sprite>(Folder + name);
            if (sp == null) Debug.LogWarning("[InsideHiveSetup] Not found: " + Folder + name);
            return sp;
        }

        var s01      = Load("Inside_Hive_01.png");
        var s02T     = Load("Inside_Hive_02_T.png");
        var s03B     = Load("Inside_Hive_03_B.png");
        var s04L     = Load("Inside_Hive_04_L.png");
        var s05R     = Load("Inside_Hive_05_R.png");
        var s06TB    = Load("Inside_Hive_06_TB.png");
        var s07LR    = Load("Inside_Hive_07_LR.png");
        var s08TL    = Load("Inside_Hive_08_TL.png");
        var s09TR    = Load("Inside_Hive_09_TR.png");
        var s10BL    = Load("Inside_Hive_10_BL.png");
        var s11BR    = Load("Inside_Hive_11_BR.png");
        var s12TBL   = Load("Inside_Hive_12_TBL.png");
        var s13TBR   = Load("Inside_Hive_13_TBR.png");
        var s14TLR   = Load("Inside_Hive_14_TLR.png");
        var s15BLR   = Load("Inside_Hive_15_BLR.png");
        var s16TBLR  = Load("Inside_Hive_16_TBLR.png");
        var sInnerTL = Load("Inside_Hive_Inner_TL.png");
        var sInnerTR = Load("Inside_Hive_Inner_TR.png");
        var sInnerBL = Load("Inside_Hive_Inner_BL.png");
        var sInnerBR = Load("Inside_Hive_Inner_BR.png");

        var tile = ScriptableObject.CreateInstance<RuleTile>();
        tile.m_DefaultSprite = s16TBLR;
        tile.m_TilingRules   = new List<RuleTile.TilingRule>();

        int Y = RuleTile.TilingRuleOutput.Neighbor.This;
        int N = RuleTile.TilingRuleOutput.Neighbor.NotThis;

        static Vector3Int Up()        => new( 0,  1, 0);
        static Vector3Int Down()      => new( 0, -1, 0);
        static Vector3Int Left()      => new(-1,  0, 0);
        static Vector3Int Right()     => new( 1,  0, 0);
        static Vector3Int UpLeft()    => new(-1,  1, 0);
        static Vector3Int UpRight()   => new( 1,  1, 0);
        static Vector3Int DownLeft()  => new(-1, -1, 0);
        static Vector3Int DownRight() => new( 1, -1, 0);

        void AddRule(Sprite sprite, int top, int bot, int left, int right,
                     Vector3Int diagPos = default, int diagVal = 0)
        {
            if (sprite == null) return;
            var rule = new RuleTile.TilingRule
            {
                m_Sprites           = new[] { sprite },
                m_Output            = RuleTile.TilingRuleOutput.OutputSprite.Single,
                m_NeighborPositions = new List<Vector3Int> { Up(), Down(), Left(), Right() },
                m_Neighbors         = new List<int>        { top,  bot,   left,   right   },
            };
            if (diagVal != 0)
            {
                rule.m_NeighborPositions.Add(diagPos);
                rule.m_Neighbors.Add(diagVal);
            }
            tile.m_TilingRules.Add(rule);
        }

        // Inner corners — before plain centre so they take priority
        AddRule(sInnerTL, Y, Y, Y, Y, UpLeft(),    N);
        AddRule(sInnerTR, Y, Y, Y, Y, UpRight(),   N);
        AddRule(sInnerBL, Y, Y, Y, Y, DownLeft(),  N);
        AddRule(sInnerBR, Y, Y, Y, Y, DownRight(), N);

        // Outer shape rules — top / bot / left / right
        AddRule(s01,     N, N, N, N);
        AddRule(s16TBLR, Y, Y, Y, Y);
        AddRule(s12TBL,  Y, Y, Y, N);
        AddRule(s13TBR,  Y, Y, N, Y);
        AddRule(s14TLR,  Y, N, Y, Y);
        AddRule(s15BLR,  N, Y, Y, Y);
        AddRule(s08TL,   Y, N, Y, N);
        AddRule(s09TR,   Y, N, N, Y);
        AddRule(s10BL,   N, Y, Y, N);
        AddRule(s11BR,   N, Y, N, Y);
        AddRule(s06TB,   Y, Y, N, N);
        AddRule(s07LR,   N, N, Y, Y);
        AddRule(s02T,    Y, N, N, N);
        AddRule(s03B,    N, Y, N, N);
        AddRule(s04L,    N, N, Y, N);
        AddRule(s05R,    N, N, N, Y);

        if (File.Exists(OutputPath))
            AssetDatabase.DeleteAsset(OutputPath);

        AssetDatabase.CreateAsset(tile, OutputPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[InsideHiveSetup] Done: " + OutputPath);
        Selection.activeObject = tile;
        EditorGUIUtility.PingObject(tile);
    }

    static void EnsureSprite(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;

        importer.textureType         = TextureImporterType.Sprite;
        importer.spriteImportMode    = SpriteImportMode.Single;
        importer.filterMode          = FilterMode.Point;
        importer.textureCompression  = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled       = false;
        importer.spritePixelsPerUnit = 32f;
        importer.SaveAndReimport();
    }
}
#endif
