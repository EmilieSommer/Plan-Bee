#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// One-click setup for Kenney Tiny Town autotiling.
/// Run: Tools → Plan Bee → Setup Kenney Stone Floor Rule Tile
///
/// What it does:
///   1. Slices tilemap_packed.png into a 12×11 grid of 16×16 sprites.
///   2. Creates a RuleTile asset with the 9-tile stone floor autotile rules:
///
///      [ 96 TL ] [ 97 T  ] [ 98 TR ]   ← row 8 in image (no neighbor above)
///      [108 L  ] [109 C  ] [110 R  ]   ← row 9             (center)
///      [120 BL ] [121 B  ] [122 BR ]   ← row 10           (no neighbor below)
///
/// After running: open Window → 2D → Tile Palette, create a palette,
/// drag in the StoneFloor_RuleTile asset, and paint on any Tilemap.
/// Neighbors auto-update as you paint.
/// </summary>
public static class KenneyTinyTownSetup
{
    const string TexPath  = "Assets/05_Tiles/Test_Tiles/KenneyTinyTown/tilemap_packed.png";
    const string TilePath = "Assets/05_Tiles/Test_Tiles/KenneyTinyTown/StoneFloor_RuleTile.asset";

    const int Cols     = 12;
    const int Rows     = 11;
    const int TileSize = 16;

    [MenuItem("Tools/Plan Bee/Setup Kenney Stone Floor Rule Tile")]
    static void Setup()
    {
        if (!File.Exists(TexPath))
        {
            EditorUtility.DisplayDialog("Kenney Setup",
                $"Texture not found.\nExpected: {TexPath}", "OK");
            return;
        }

        SliceTexture();

        // Refresh so Unity registers the new sprite sub-assets before we load them.
        AssetDatabase.ImportAsset(TexPath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();

        CreateRuleTile();
    }

    // ── Step 1: configure the TextureImporter and slice ─────────────────────

    static void SliceTexture()
    {
#pragma warning disable CS0618 // SpriteMetaData obsolete in newer Unity but still functional
        var importer = AssetImporter.GetAtPath(TexPath) as TextureImporter;
        if (importer == null) return;

        importer.textureType          = TextureImporterType.Sprite;
        importer.spriteImportMode     = SpriteImportMode.Multiple;
        importer.filterMode           = FilterMode.Point;
        importer.textureCompression   = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled        = false;
        importer.spritePixelsPerUnit  = 16f;

        int texH = Rows * TileSize; // 176 px

        var metas = new SpriteMetaData[Cols * Rows];
        int idx = 0;
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Cols; col++)
            {
                // Unity rects have y=0 at the BOTTOM of the texture.
                // Image row 0 (top) maps to Unity y = texH - TileSize.
                float uy = (texH - TileSize) - row * TileSize;
                metas[idx] = new SpriteMetaData
                {
                    name      = $"tilemap_packed_{idx}",
                    rect      = new Rect(col * TileSize, uy, TileSize, TileSize),
                    alignment = 0,
                    pivot     = new Vector2(0.5f, 0.5f),
                };
                idx++;
            }
        }

        importer.spritesheet = metas;
        importer.SaveAndReimport();
#pragma warning restore CS0618
    }

    // ── Step 2: build the RuleTile ───────────────────────────────────────────

    static void CreateRuleTile()
    {
        // Load sliced sprites by name
        var allAssets = AssetDatabase.LoadAllAssetsAtPath(TexPath);
        var byName    = new Dictionary<string, Sprite>();
        foreach (var a in allAssets)
            if (a is Sprite s) byName[s.name] = s;

        Sprite GetSprite(int index)
        {
            string name = $"tilemap_packed_{index}";
            if (byName.TryGetValue(name, out var sp)) return sp;
            Debug.LogWarning($"[KenneySetup] Sprite not found: {name}");
            return null;
        }

        // Stone floor 9-tile layout (image rows 8–10, cols 0–2)
        //   Row 8 = no-top-neighbor edge/corners
        //   Row 9 = middle (center + left/right edges)
        //   Row 10 = no-bottom-neighbor edge/corners
        var tl = GetSprite(96);   // top-left corner
        var tc = GetSprite(97);   // top edge
        var tr = GetSprite(98);   // top-right corner
        var ml = GetSprite(108);  // left edge
        var cc = GetSprite(109);  // center (fully surrounded)
        var mr = GetSprite(110);  // right edge
        var bl = GetSprite(120);  // bottom-left corner
        var bc = GetSprite(121);  // bottom edge
        var br = GetSprite(122);  // bottom-right corner

        var tile = ScriptableObject.CreateInstance<RuleTile>();
        tile.m_DefaultSprite = cc;
        tile.m_TilingRules   = new List<RuleTile.TilingRule>();

        // Helpers ─────────────────────────────────────────────────────────────
        // neighbor values: +1 = This (must match), -1 = NotThis (must differ), 0 = DontCare
        static Vector3Int Up()    => new( 0,  1, 0);
        static Vector3Int Down()  => new( 0, -1, 0);
        static Vector3Int Left()  => new(-1,  0, 0);
        static Vector3Int Right() => new( 1,  0, 0);

        void AddRule(Sprite sprite, int top, int bottom, int left, int right)
        {
            if (sprite == null) return;
            var rule = new RuleTile.TilingRule
            {
                m_Sprites            = new[] { sprite },
                m_Output             = RuleTile.TilingRuleOutput.OutputSprite.Single,
                m_NeighborPositions  = new List<Vector3Int>(),
                m_Neighbors          = new List<int>(),
            };

            void Add(Vector3Int pos, int val)
            {
                if (val == 0) return; // DontCare — omit from list
                rule.m_NeighborPositions.Add(pos);
                rule.m_Neighbors.Add(val > 0
                    ? RuleTile.TilingRuleOutput.Neighbor.This
                    : RuleTile.TilingRuleOutput.Neighbor.NotThis);
            }

            Add(Up(),    top);
            Add(Down(),  bottom);
            Add(Left(),  left);
            Add(Right(), right);

            tile.m_TilingRules.Add(rule);
        }

        // ── Rules (corners first — they are more specific than edges) ─────────
        // Convention: -1 = NotThis (absent), +1 = This (present), 0 = DontCare
        //                              top  bot  left right
        AddRule(tl, -1,  0, -1,  0);  // top-left corner
        AddRule(tr, -1,  0,  0, -1);  // top-right corner
        AddRule(bl,  0, -1, -1,  0);  // bottom-left corner
        AddRule(br,  0, -1,  0, -1);  // bottom-right corner
        AddRule(tc, -1,  0,  0,  0);  // top edge
        AddRule(bc,  0, -1,  0,  0);  // bottom edge
        AddRule(ml,  0,  0, -1,  0);  // left edge
        AddRule(mr,  0,  0,  0, -1);  // right edge
        // Center: no rules — handled by m_DefaultSprite

        // Save ────────────────────────────────────────────────────────────────
        if (File.Exists(TilePath))
            AssetDatabase.DeleteAsset(TilePath);

        AssetDatabase.CreateAsset(tile, TilePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[KenneySetup] Done → {TilePath}");
        Selection.activeObject = tile;
        EditorGUIUtility.PingObject(tile);
    }
}
#endif
