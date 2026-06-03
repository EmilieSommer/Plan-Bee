using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Owns all tilemap visuals for the hive.
/// HiveGrid calls into this — it has no knowledge of sprites or RuleTiles itself.
///
/// Setup in the scene:
///   Attach to the same GameObject as HiveGrid (or a child).
///   Assign three Tilemap references and the tile library in the Inspector.
///
/// Layer order (Tilemap Renderer Sorting Order):
///   builtTilemap   0  — zone fill, autotiles via RuleTile
///   overlayTilemap 1  — transparent edge blend where zones meet
///   markedTilemap  2  — construction ghost
/// </summary>
public class HiveVisuals : MonoBehaviour
{
    public static HiveVisuals Instance { get; private set; }

    [Header("Tilemaps")]
    [SerializeField] private Tilemap builtTilemap;
    [SerializeField] private Tilemap overlayTilemap;
    [SerializeField] private Tilemap markedTilemap;

    [Header("Library")]
    [SerializeField] private HiveTileLibrary library;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        library.Init();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Place the built RuleTile and its overlay at pos.</summary>
    public void SetBuilt(Vector3Int pos, HiveTileType type)
    {
        builtTilemap.SetTile(pos, library.GetBuiltTile(type));

        if (overlayTilemap != null)
            overlayTilemap.SetTile(pos, library.GetOverlayTile(type));
    }

    /// <summary>Show the construction ghost (marked state).</summary>
    public void SetMarked(Vector3Int pos, HiveTileType type)
    {
        var sprite = library.GetMarkedSprite(type);
        if (sprite == null) return;

        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.colliderType = Tile.ColliderType.None;
        markedTilemap.SetTile(pos, tile);
    }

    /// <summary>Remove the construction ghost.</summary>
    public void ClearMarked(Vector3Int pos)
    {
        markedTilemap.SetTile(pos, null);
    }

    /// <summary>Remove all visual layers at pos.</summary>
    public void ClearAll(Vector3Int pos)
    {
        builtTilemap.SetTile(pos, null);
        overlayTilemap?.SetTile(pos, null);
        markedTilemap.SetTile(pos, null);
    }
}
