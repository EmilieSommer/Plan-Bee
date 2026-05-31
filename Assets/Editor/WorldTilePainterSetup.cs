#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Tools → Plan Bee → Setup World Tile Painter
/// Creates the WorldTilemap and WorldTilePainter objects in the scene
/// and wires everything up automatically.
/// </summary>
public static class WorldTilePainterSetup
{
    const string RuleTilePath = "Assets/05_Tiles/Test_Tiles/KenneyTinyTown/StoneFloor_RuleTile.asset";

    [MenuItem("Tools/Plan Bee/Setup World Tile Painter")]
    static void Setup()
    {
        // ── 1. World Tilemap ─────────────────────────────────────────────────
        Tilemap worldTilemap = null;
        var existingGrid = GameObject.Find("WorldGrid");

        if (existingGrid == null)
        {
            // Grid root
            var gridGO = new GameObject("WorldGrid");
            var grid   = gridGO.AddComponent<Grid>();
            grid.cellSize = Vector3.one;
            Undo.RegisterCreatedObjectUndo(gridGO, "Create WorldGrid");

            // Tilemap child
            var tilemapGO  = new GameObject("WorldTilemap");
            tilemapGO.transform.SetParent(gridGO.transform, false);
            worldTilemap   = tilemapGO.AddComponent<Tilemap>();
            var renderer   = tilemapGO.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = -2; // behind hive, behind background
            Undo.RegisterCreatedObjectUndo(tilemapGO, "Create WorldTilemap");
        }
        else
        {
            worldTilemap = existingGrid.GetComponentInChildren<Tilemap>();
        }

        // ── 2. WorldTilePainter object ───────────────────────────────────────
        var existing = GameObject.Find("WorldTilePainter");
        if (existing != null)
        {
            Debug.Log("[WorldTilePainterSetup] WorldTilePainter already exists in scene.");
            Selection.activeGameObject = existing;
            return;
        }

        var painterGO = new GameObject("WorldTilePainter");
        Undo.RegisterCreatedObjectUndo(painterGO, "Create WorldTilePainter");

        var painter = painterGO.AddComponent<WorldTilePainter>();

        // ── 3. Ghost child ───────────────────────────────────────────────────
        var ghostGO = new GameObject("Ghost");
        ghostGO.transform.SetParent(painterGO.transform, false);
        Undo.RegisterCreatedObjectUndo(ghostGO, "Create Ghost");

        var ghostSR        = ghostGO.AddComponent<SpriteRenderer>();
        ghostSR.sortingOrder = 10;   // always on top
        ghostSR.enabled      = false; // hidden until build mode is active

        // ── 4. Load Rule Tile and assign default ghost sprite ────────────────
        var ruleTile = AssetDatabase.LoadAssetAtPath<RuleTile>(RuleTilePath);
        if (ruleTile == null)
        {
            Debug.LogWarning($"[WorldTilePainterSetup] Rule Tile not found at {RuleTilePath}. " +
                             "Run Tools → Plan Bee → Setup Kenney Stone Floor Rule Tile first, " +
                             "then assign it manually in the WorldTilePainter Inspector.");
        }
        else
        {
            ghostSR.sprite = ruleTile.m_DefaultSprite;
        }

        // ── 5. Wire up serialized fields via SerializedObject ────────────────
        var so = new SerializedObject(painter);
        so.FindProperty("worldTilemap").objectReferenceValue  = worldTilemap;
        so.FindProperty("ruleTile").objectReferenceValue      = ruleTile;
        so.FindProperty("ghostRenderer").objectReferenceValue = ghostSR;
        so.ApplyModifiedProperties();

        // ── 6. Mark scene dirty so Unity saves the changes ───────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Selection.activeGameObject = painterGO;
        Debug.Log("[WorldTilePainterSetup] Done! WorldTilePainter is ready. " +
                  "Hook a UI button to WorldTilePainter.Toggle() to activate it in play mode.");
    }
}
#endif
