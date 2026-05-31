#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Tools → Plan Bee → Create Rule Tile From Sprites
///
/// Select a texture (sprite sheet) in the Project window, then run this.
/// It creates a RuleTile asset next to the texture with the 16 standard
/// 4-directional neighbor rules pre-configured. Drag sprites from the
/// Inspector into the correct rule slots — or use the auto-assign button
/// if your sprites are named with the _01/_02_T/... convention.
/// </summary>
public static class RuleTileCreator
{
    // Standard 4-direction bitmask rules: T=1 B=2 L=4 R=8 (RuleTile uses this order)
    // Each entry: (label, top, bottom, left, right) where:
    //   1 = must have neighbor, -1 = must NOT have neighbor, 0 = don't care
    static readonly (string label, int top, int bottom, int left, int right)[] Rules =
    {
        ("Isolated",          -1, -1, -1, -1),
        ("Top only",           1, -1, -1, -1),
        ("Bottom only",       -1,  1, -1, -1),
        ("Left only",         -1, -1,  1, -1),
        ("Right only",        -1, -1, -1,  1),
        ("Top+Bottom",         1,  1, -1, -1),
        ("Left+Right",        -1, -1,  1,  1),
        ("Top+Left",           1, -1,  1, -1),
        ("Top+Right",          1, -1, -1,  1),
        ("Bottom+Left",       -1,  1,  1, -1),
        ("Bottom+Right",      -1,  1, -1,  1),
        ("Top+Bottom+Left",    1,  1,  1, -1),
        ("Top+Bottom+Right",   1,  1, -1,  1),
        ("Top+Left+Right",     1, -1,  1,  1),
        ("Bottom+Left+Right", -1,  1,  1,  1),
        ("All neighbors",      1,  1,  1,  1),
    };

    [MenuItem("Tools/Plan Bee/Create Rule Tile From Sprites", validate = false)]
    static void CreateRuleTile()
    {
        // Need a texture selected
        var tex = Selection.activeObject as Texture2D;
        if (tex == null)
        {
            EditorUtility.DisplayDialog(
                "Rule Tile Creator",
                "Select a Texture2D (sprite sheet) in the Project window first, then run this menu item.",
                "OK");
            return;
        }

        string texPath  = AssetDatabase.GetAssetPath(tex);
        string dir      = Path.GetDirectoryName(texPath);
        string baseName = Path.GetFileNameWithoutExtension(texPath);
        string tilePath = $"{dir}/{baseName}_RuleTile.asset";

        // Load all sprites from the sheet
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(texPath);
        var sprites = new System.Collections.Generic.List<Sprite>();
        foreach (var a in allAssets)
            if (a is Sprite s) sprites.Add(s);

        if (sprites.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "Rule Tile Creator",
                "No sprites found in this texture.\nMake sure the texture is imported as Sprite Mode: Multiple and sliced in the Sprite Editor.",
                "OK");
            return;
        }

        // Create or load existing
        var tile = AssetDatabase.LoadAssetAtPath<RuleTile>(tilePath)
                   ?? ScriptableObject.CreateInstance<RuleTile>();

        tile.m_TilingRules = new System.Collections.Generic.List<RuleTile.TilingRule>();

        // Default sprite is the first one (isolated / no neighbors)
        tile.m_DefaultSprite = sprites[0];

        foreach (var (label, top, bottom, left, right) in Rules)
        {
            var rule = new RuleTile.TilingRule();

            // RuleTile neighbor array order: 0=top-left, 1=top, 2=top-right,
            //   3=left, 4=right, 5=bottom-left, 6=bottom, 7=bottom-right
            rule.m_Neighbors = new System.Collections.Generic.List<int>(new int[8]);
            rule.m_NeighborPositions = new System.Collections.Generic.List<Vector3Int>
            {
                new(-1,  1, 0), // top-left    (don't care)
                new( 0,  1, 0), // top
                new( 1,  1, 0), // top-right   (don't care)
                new(-1,  0, 0), // left
                new( 1,  0, 0), // right
                new(-1, -1, 0), // bottom-left (don't care)
                new( 0, -1, 0), // bottom
                new( 1, -1, 0), // bottom-right(don't care)
            };

            int ToNeighbor(int val) => val switch { 1 => RuleTile.TilingRuleOutput.Neighbor.This, -1 => RuleTile.TilingRuleOutput.Neighbor.NotThis, _ => 0 };

            rule.m_Neighbors[1] = ToNeighbor(top);
            rule.m_Neighbors[6] = ToNeighbor(bottom);
            rule.m_Neighbors[3] = ToNeighbor(left);
            rule.m_Neighbors[4] = ToNeighbor(right);

            // Leave sprite unassigned — user drags their sprites in,
            // or run Auto-Assign if using _01/_02_T/... naming convention.
            rule.m_Sprites    = new Sprite[1];
            rule.m_Output     = RuleTile.TilingRuleOutput.OutputSprite.Single;

            tile.m_TilingRules.Add(rule);
        }

        if (!File.Exists(tilePath))
            AssetDatabase.CreateAsset(tile, tilePath);
        else
            EditorUtility.SetDirty(tile);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[RuleTileCreator] Created: {tilePath}  ({sprites.Count} sprites available, {Rules.Length} rules configured)");
        Selection.activeObject = tile;
        EditorGUIUtility.PingObject(tile);
    }

    // Validate: only enable menu item when a Texture2D is selected
    [MenuItem("Tools/Plan Bee/Create Rule Tile From Sprites", validate = true)]
    static bool ValidateCreate() => Selection.activeObject is Texture2D;
}
#endif
