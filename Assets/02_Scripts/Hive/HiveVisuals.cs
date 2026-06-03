using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Owns all tilemap visuals for the hive. HiveGrid calls into this; it has no
/// knowledge of sprites or RuleTiles itself.
///
/// Per-cell rendering rule (code-driven, no RuleTiles):
///   For room tiles (Brood/Storage/InsideHive/Solid):
///     side faces same type        → nothing
///     side faces different room   → overlay
///     side faces Hive             → border
///     side faces empty            → nothing
///   For Hive tiles:
///     side faces empty            → border (the outer wall)
///     side faces anything else    → nothing
///
/// Sorting order:
///   builtTilemap   0  — fill + border (the per-cell visual)
///   overlayTilemap 1  — transparent room-to-room blends
///   markedTilemap  2  — construction ghost
///   hoverTilemap   3  — build cursor (owned by BuildCursor, not this script)
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

    public Tilemap BuiltTilemap     => builtTilemap;
    public HiveTileLibrary Library  => library;

    [Header("Build Overlay")]
    [SerializeField] private Sprite markedOverlaySprite;

    // Cardinal direction order — must match the bit layout used by the library
    // bit 0 = top, 1 = bottom, 2 = left, 3 = right
    static readonly Vector3Int[] Dirs =
        { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

    // Cache: avoid allocating a fresh Tile every refresh.
    readonly Dictionary<Sprite, Tile> _tileCache = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        if (library != null) library.Init();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Render a built tile at pos and refresh its neighbors.</summary>
    public void SetBuilt(Vector3Int pos, HiveTileType type)
    {
        RenderCell(pos, type);
        RefreshNeighbors(pos);
    }

    /// <summary>Show the construction ghost (marked state).</summary>
    public void SetMarked(Vector3Int pos, HiveTileType type)
    {
        if (markedOverlaySprite == null || markedTilemap == null) return;
        markedTilemap.SetTile(pos, GetCachedTile(markedOverlaySprite));
    }

    /// <summary>Remove the construction ghost.</summary>
    public void ClearMarked(Vector3Int pos)
    {
        if (markedTilemap != null) markedTilemap.SetTile(pos, null);
    }

    /// <summary>Remove all visual layers at pos and refresh neighbors.</summary>
    public void ClearAll(Vector3Int pos)
    {
        if (builtTilemap != null)   builtTilemap.SetTile(pos, null);
        if (overlayTilemap != null) overlayTilemap.SetTile(pos, null);
        if (markedTilemap != null)  markedTilemap.SetTile(pos, null);
        RefreshNeighbors(pos);
    }

    /// <summary>Recompute a single cell's visual from current grid state.</summary>
    public void RefreshAt(Vector3Int pos)
    {
        var type = HiveGrid.Instance != null
            ? HiveGrid.Instance.GetType(pos)
            : HiveTileType.None;

        if (type == HiveTileType.None)
        {
            if (builtTilemap != null)   builtTilemap.SetTile(pos, null);
            if (overlayTilemap != null) overlayTilemap.SetTile(pos, null);
            return;
        }
        RenderCell(pos, type);
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    void RenderCell(Vector3Int pos, HiveTileType type)
    {
        if (library == null || builtTilemap == null) return;

        int borderMask  = ComputeBorderMask(pos, type);
        int overlayMask = ComputeOverlayMask(pos, type);

        var borderSprite  = library.GetBorderSprite(type, borderMask);
        var overlaySprite = library.GetOverlaySprite(type, overlayMask);

        builtTilemap.SetTile(pos, borderSprite ? GetCachedTile(borderSprite) : null);

        if (overlayTilemap != null)
            overlayTilemap.SetTile(pos, overlaySprite ? GetCachedTile(overlaySprite) : null);
    }

    void RefreshNeighbors(Vector3Int pos)
    {
        foreach (var d in Dirs) RefreshAt(pos + d);
    }

    /// <summary>
    /// Mask bit set if that side is "connected" (no border drawn there).
    ///   Room types : connected = neighbor is NOT Hive
    ///   Hive       : connected = neighbor is NOT empty
    /// </summary>
    int ComputeBorderMask(Vector3Int pos, HiveTileType self)
    {
        int mask = 0;
        for (int i = 0; i < 4; i++)
        {
            var n = HiveGrid.Instance.GetType(pos + Dirs[i]);
            bool counterpart = (self == HiveTileType.Hive)
                ? n == HiveTileType.None
                : n == HiveTileType.Hive;
            if (!counterpart) mask |= (1 << i);
        }
        return mask;
    }

    /// <summary>
    /// Mask bit set if that side faces a different room type.
    /// Hive has no overlay.
    /// </summary>
    int ComputeOverlayMask(Vector3Int pos, HiveTileType self)
    {
        if (self == HiveTileType.Hive) return 0;
        int mask = 0;
        for (int i = 0; i < 4; i++)
        {
            var n = HiveGrid.Instance.GetType(pos + Dirs[i]);
            bool differentRoom =
                n != HiveTileType.None &&
                n != HiveTileType.Hive &&
                n != self;
            if (differentRoom) mask |= (1 << i);
        }
        return mask;
    }

    Tile GetCachedTile(Sprite sprite)
    {
        if (_tileCache.TryGetValue(sprite, out var t)) return t;
        t = ScriptableObject.CreateInstance<Tile>();
        t.sprite = sprite;
        t.colliderType = Tile.ColliderType.None;
        _tileCache[sprite] = t;
        return t;
    }
}
