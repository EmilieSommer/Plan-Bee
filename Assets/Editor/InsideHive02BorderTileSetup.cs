#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Creates the InsideHive02 Border RuleTile from the SVG sprites in
/// Assets/05_Tiles/InsideHive/Inside_Hive_02/Borders/.
///
/// Run via:  Tools ▸ Plan Bee ▸ Setup Inside Hive 02 Border Tiles
/// </summary>
public static class InsideHive02BorderTileSetup
{
    // ── Paths ──────────────────────────────────────────────────────────────
    const string BF  = "Assets/05_Tiles/InsideHive/Inside_Hive_02/Borders/";
    const string IBF = "Assets/05_Tiles/InsideHive/Inside_Hive_02/Borders/Inside_Border/";
    const string OBF = "Assets/05_Tiles/InsideHive/Inside_Hive_02/Borders/Outside_Border/";
    const string Out = "Assets/05_Tiles/InsideHive/Inside_Hive_02/Borders/InsideHive02_Border_RuleTile.asset";

    // ── Sprite import config: (assetPath, pivotX, pivotY) ──────────────────
    //
    //  Pivot logic (matches InsideHive02TileSetup convention):
    //    • Extends LEFT  → pivotX = 20/36  (base tile centre shifts right)
    //    • Extends RIGHT → pivotX = 16/36  (base tile centre shifts left)
    //    • Extends UP    → pivotY = 16/36  (base centre is lower in the image)
    //    • Extends DOWN  → pivotY = 20/36
    //    • Both / symmetric → 0.5
    //
    static readonly (string path, float px, float py)[] Sprites =
    {
        // ── 4 cardinal borders ─────────────────────────────────────────────
        (BF  + "Inside_Hive_Border_Top.svg",            0.5f,       16f/36f ),  // 32×36, extends up
        (BF  + "Inside_Hive_Border_Bottom.svg",         0.5f,       20f/36f ),  // 32×36, extends down
        (BF  + "Inside_Hive_Border_Left.svg",           20f/36f,    0.5f    ),  // 36×32, extends left
        (BF  + "Inside_Hive_Border_Right.svg",          16f/36f,    0.5f    ),  // 36×32, extends right

        // ── 7 combination borders ──────────────────────────────────────────
        (BF  + "Inside_Hive_Border_T+B.svg",            0.5f,       0.5f    ),  // 36×36, extends up+down
        (BF  + "Inside_Hive_Border_L+R.svg",            0.5f,       0.5f    ),  // 36×36, extends left+right
        (BF  + "Inside_Hive_Border_T+B+L.svg",          20f/36f,    0.5f    ),  // 36×32, extends left
        (BF  + "Inside_Hive_Border_T+B+R.svg",          16f/36f,    0.5f    ),  // 36×32, extends right
        (BF  + "Inside_Hive_Border_T+L+R.svg",          0.5f,       16f/36f ),  // 32×36, extends up
        (BF  + "Inside_Hive_Border_B+L+R.svg",          0.5f,       20f/36f ),  // 32×36, extends down
        (BF  + "Inside_Hive_Border_T+B+L+R.svg",        0.5f,       0.5f    ),  // 36×36, all sides

        // ── 4 inside concave-corner borders ───────────────────────────────
        (IBF + "Inside_Hive_Border_Top_Left.svg",       20f/36f,    16f/36f ),  // 36×36
        (IBF + "Inside_Hive_Border_Top_Right.svg",      16f/36f,    16f/36f ),  // 36×36
        (IBF + "Inside_Hive_Border_Bot_Left.svg",       20f/36f,    20f/36f ),  // 36×36
        (IBF + "Inside_Hive_Border_Bot_Right.svg",      16f/36f,    20f/36f ),  // 36×36

        // ── 8 outside end-cap borders ──────────────────────────────────────
        (OBF + "Inside_Hive_Border_Out_Top_Left.svg",   20f/36f,    16f/36f ),  // 36×36
        (OBF + "Inside_Hive_Border_Out_Top_Right.svg",  16f/36f,    16f/36f ),  // 36×36
        (OBF + "Inside_Hive_Border_Out_Bot_Left.svg",   20f/36f,    20f/36f ),  // 36×36
        (OBF + "Inside_Hive_Border_Out_Bot_Right.svg",  16f/36f,    20f/36f ),  // 36×36
        (OBF + "Inside_Hive_Border_Out_Left_Top.svg",   20f/36f,    16f/36f ),  // 36×36
        (OBF + "Inside_Hive_Border_Out_Left_Bot.svg",   20f/36f,    20f/36f ),  // 36×36
        (OBF + "Inside_Hive_Border_Out_Right_Top.svg",  16f/36f,    16f/36f ),  // 36×36
        (OBF + "Inside_Hive_Border_Out_Right_Bot.svg",  16f/36f,    20f/36f ),  // 36×36
    };

    // ── Menu entry ──────────────────────────────────────────────────────────
    [MenuItem("Tools/Plan Bee/Setup Inside Hive 02 Border Tiles")]
    static void Setup()
    {
        // 1. Configure every SVG's import settings
        int configured = 0;
        foreach (var (path, px, py) in Sprites)
        {
            if (ConfigureSvgSprite(path, px, py))
                configured++;
        }
        Debug.Log($"[Plan Bee] Configured {configured}/{Sprites.Length} SVG importers.");

        AssetDatabase.Refresh();

        // 2. Build the RuleTile
        var tile = ScriptableObject.CreateInstance<RuleTile>();
        tile.m_TilingRules = new List<RuleTile.TilingRule>();

        int Y = RuleTile.TilingRuleOutput.Neighbor.This;     // neighbour IS same tile
        int N = RuleTile.TilingRuleOutput.Neighbor.NotThis;  // neighbour is NOT same tile

        Sprite S(string p) => AssetDatabase.LoadAssetAtPath<Sprite>(p);

        // Simple 4-directional rule (no diagonal condition)
        void Rule(Sprite sprite, int top, int bot, int left, int right)
        {
            if (sprite == null) { Debug.LogWarning($"[Plan Bee] Sprite not found – skipping rule for null sprite."); return; }
            tile.m_TilingRules.Add(new RuleTile.TilingRule
            {
                m_Sprites           = new[] { sprite },
                m_Output            = RuleTile.TilingRuleOutput.OutputSprite.Single,
                m_NeighborPositions = new List<Vector3Int> { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right },
                m_Neighbors         = new List<int>        { top,           bot,             left,            right            },
            });
        }

        // Rule with one additional diagonal condition (for outside end-caps)
        void RuleDiag(Sprite sprite, int top, int bot, int left, int right,
                      Vector3Int diagPos, int diagVal)
        {
            if (sprite == null) { Debug.LogWarning($"[Plan Bee] Sprite not found – skipping diagonal rule."); return; }
            tile.m_TilingRules.Add(new RuleTile.TilingRule
            {
                m_Sprites           = new[] { sprite },
                m_Output            = RuleTile.TilingRuleOutput.OutputSprite.Single,
                m_NeighborPositions = new List<Vector3Int> { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right, diagPos },
                m_Neighbors         = new List<int>        { top,           bot,             left,            right,            diagVal },
            });
        }

        // ── Rule ordering: most specific first ─────────────────────────────
        //
        //  Convention matches InsideHive02TileSetup:
        //    top / bot / left / right = Y  → that neighbour IS present (no border on that edge)
        //                             = N  → that neighbour is ABSENT  (border shows on that edge)
        //
        //  SVG file name = which borders are VISIBLE = which neighbours are ABSENT.

        // Outside end-caps (cardinal + one diagonal absent) — most specific
        RuleDiag(S(OBF+"Inside_Hive_Border_Out_Top_Left.svg"),   N,Y,Y,Y, new Vector3Int(-1, 1,0), N);
        RuleDiag(S(OBF+"Inside_Hive_Border_Out_Top_Right.svg"),  N,Y,Y,Y, new Vector3Int( 1, 1,0), N);
        RuleDiag(S(OBF+"Inside_Hive_Border_Out_Bot_Left.svg"),   Y,N,Y,Y, new Vector3Int(-1,-1,0), N);
        RuleDiag(S(OBF+"Inside_Hive_Border_Out_Bot_Right.svg"),  Y,N,Y,Y, new Vector3Int( 1,-1,0), N);
        RuleDiag(S(OBF+"Inside_Hive_Border_Out_Left_Top.svg"),   Y,Y,N,Y, new Vector3Int(-1, 1,0), N);
        RuleDiag(S(OBF+"Inside_Hive_Border_Out_Left_Bot.svg"),   Y,Y,N,Y, new Vector3Int(-1,-1,0), N);
        RuleDiag(S(OBF+"Inside_Hive_Border_Out_Right_Top.svg"),  Y,Y,Y,N, new Vector3Int( 1, 1,0), N);
        RuleDiag(S(OBF+"Inside_Hive_Border_Out_Right_Bot.svg"),  Y,Y,Y,N, new Vector3Int( 1,-1,0), N);

        // 4-absent: isolated tile
        Rule(S(BF +"Inside_Hive_Border_T+B+L+R.svg"),  N,N,N,N);

        // 3-absent
        Rule(S(BF +"Inside_Hive_Border_T+B+L.svg"),    N,N,N,Y);  // only right present
        Rule(S(BF +"Inside_Hive_Border_T+B+R.svg"),    N,N,Y,N);  // only left present
        Rule(S(BF +"Inside_Hive_Border_T+L+R.svg"),    N,Y,N,N);  // only bottom present
        Rule(S(BF +"Inside_Hive_Border_B+L+R.svg"),    Y,N,N,N);  // only top present

        // 2-absent (axis pairs)
        Rule(S(BF +"Inside_Hive_Border_T+B.svg"),      N,N,Y,Y);  // top+bottom absent
        Rule(S(BF +"Inside_Hive_Border_L+R.svg"),      Y,Y,N,N);  // left+right absent

        // 2-absent (diagonal pairs = inside concave corners)
        Rule(S(IBF+"Inside_Hive_Border_Top_Left.svg"), N,Y,N,Y);  // top+left absent
        Rule(S(IBF+"Inside_Hive_Border_Top_Right.svg"),N,Y,Y,N);  // top+right absent
        Rule(S(IBF+"Inside_Hive_Border_Bot_Left.svg"), Y,N,N,Y);  // bottom+left absent
        Rule(S(IBF+"Inside_Hive_Border_Bot_Right.svg"),Y,N,Y,N);  // bottom+right absent

        // 1-absent (cardinal borders)
        Rule(S(BF +"Inside_Hive_Border_Top.svg"),      N,Y,Y,Y);
        Rule(S(BF +"Inside_Hive_Border_Bottom.svg"),   Y,N,Y,Y);
        Rule(S(BF +"Inside_Hive_Border_Left.svg"),     Y,Y,N,Y);
        Rule(S(BF +"Inside_Hive_Border_Right.svg"),    Y,Y,Y,N);

        // 0-absent (fully surrounded, no border) — no rule needed;
        // RuleTile falls back to its default sprite (leave it null = invisible)

        // 3. Save the asset
        AssetDatabase.DeleteAsset(Out); // remove stale asset if re-running
        AssetDatabase.CreateAsset(tile, Out);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = tile;
        EditorUtility.FocusProjectWindow();
        Debug.Log($"[Plan Bee] Border RuleTile saved at {Out}  ({tile.m_TilingRules.Count} rules).");
    }

    // ── SVG import helper ───────────────────────────────────────────────────
    static bool ConfigureSvgSprite(string path, float pivotX, float pivotY)
    {
        var importer = AssetImporter.GetAtPath(path);
        if (importer == null)
        {
            Debug.LogWarning($"[Plan Bee] SVG importer not found: {path}  (run again after Unity has imported the file)");
            return false;
        }

        var so = new SerializedObject(importer);

        // Core quality settings
        TrySet(so, "svgPixelsPerUnit",  32f);
        TrySet(so, "filterMode",        0);    // FilterMode.Point

        // Custom pivot
        TrySet(so, "alignment",         9);    // SpriteAlignment.Custom
        TrySet(so, "customPivot",       new Vector2(pivotX, pivotY));

        // Some Unity versions expose a nested spriteData block
        var spriteData = so.FindProperty("spriteData");
        if (spriteData != null)
        {
            var sp = spriteData.FindPropertyRelative("SpritePivot");
            if (sp != null) sp.vector2Value = new Vector2(pivotX, pivotY);
            var sa = spriteData.FindPropertyRelative("SpriteAlignment");
            if (sa != null) sa.intValue = 9;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        importer.SaveAndReimport();
        return true;
    }

    static void TrySet(SerializedObject so, string name, float value)
    {
        var prop = so.FindProperty(name);
        if (prop != null) prop.floatValue = value;
    }

    static void TrySet(SerializedObject so, string name, int value)
    {
        var prop = so.FindProperty(name);
        if (prop != null) prop.intValue = value;
    }

    static void TrySet(SerializedObject so, string name, Vector2 value)
    {
        var prop = so.FindProperty(name);
        if (prop != null) prop.vector2Value = value;
    }
}
#endif
