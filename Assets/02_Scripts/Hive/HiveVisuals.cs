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
    [SerializeField] private Tilemap[] overlayTilemaps = new Tilemap[4];
    [SerializeField] private Tilemap markedTilemap;
    [SerializeField] private Tilemap indicatorTilemap;

    [Header("Library")]
    [SerializeField] private HiveTileLibrary library;

    public Tilemap BuiltTilemap     => builtTilemap;
    public Tilemap[] OverlayTilemaps => overlayTilemaps;
    public HiveTileLibrary Library  => library;

    [Header("Build Overlay")]
    [SerializeField] private Sprite markedOverlaySprite;
    [SerializeField] private Sprite indicatorSprite;

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

    /// <summary>Shows valid build locations for the player.</summary>
    public void ShowIndicators(IEnumerable<Vector3Int> validCells)
    {
        if (indicatorTilemap == null || indicatorSprite == null) return;
        indicatorTilemap.ClearAllTiles();
        var tile = GetCachedTile(indicatorSprite);
        foreach (var pos in validCells)
        {
            indicatorTilemap.SetTile(pos, tile);
        }
    }

    /// <summary>Clears the valid build indicators.</summary>
    public void ClearIndicators()
    {
        if (indicatorTilemap != null) indicatorTilemap.ClearAllTiles();
    }

    /// <summary>Remove all visual layers at pos and refresh neighbors.</summary>
    public void ClearAll(Vector3Int pos)
    {
        if (builtTilemap != null)   builtTilemap.SetTile(pos, null);
        if (overlayTilemaps != null) {
            for (int i = 0; i < overlayTilemaps.Length; i++) {
                if (overlayTilemaps[i] != null) overlayTilemaps[i].SetTile(pos, null);
            }
        }
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
            if (overlayTilemaps != null) {
                for (int i = 0; i < overlayTilemaps.Length; i++) {
                    if (overlayTilemaps[i] != null) overlayTilemaps[i].SetTile(pos, null);
                }
            }
            return;
        }
        RenderCell(pos, type);
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    void RenderCell(Vector3Int pos, HiveTileType type)
    {
        if (library == null || builtTilemap == null) return;

        int borderMask  = ComputeBorderMask(pos, type);
        var borderSprite  = library.GetBorderSprite(type, borderMask);

        if (borderSprite != null)
        {
            builtTilemap.SetTile(pos, GetCachedTile(borderSprite));
        }
        else if (type == HiveTileType.None)
        {
            builtTilemap.SetTile(pos, null);
        }
        // If type != None but borderSprite is null, we safely do NOTHING! 
        // This preserves Kristoffer's original hand-painted tiles instead of deleting them.

        // Clear existing overlays
        if (overlayTilemaps != null) {
            for (int i = 0; i < overlayTilemaps.Length; i++) {
                if (overlayTilemaps[i] != null) overlayTilemaps[i].SetTile(pos, null);
            }
        }

        if (type == HiveTileType.Hive || type == HiveTileType.None) return;

        // Group neighbors by their room type
        Dictionary<HiveTileType, int> neighborMasks = new Dictionary<HiveTileType, int>();
        for (int i = 0; i < 4; i++)
        {
            var n = HiveGrid.Instance.GetType(pos + Dirs[i]);
            if (n != HiveTileType.None && n != HiveTileType.Hive && n != type)
            {
                if (!neighborMasks.ContainsKey(n)) neighborMasks[n] = 0;
                neighborMasks[n] |= (1 << i);
            }
        }

        int layerIndex = 0;
        foreach (var kvp in neighborMasks)
        {
            var nType = kvp.Key;
            var mask = kvp.Value;
            var overlaySprite = library.GetOverlaySprite(nType, mask); // Fetch from neighbor's library!

            if (overlaySprite != null && overlayTilemaps != null && layerIndex < overlayTilemaps.Length)
            {
                if (overlayTilemaps[layerIndex] != null)
                {
                    overlayTilemaps[layerIndex].SetTile(pos, GetCachedTile(overlaySprite));
                }
                layerIndex++;
            }
        }
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
            bool counterpart;
            if (self == HiveTileType.Hive)
            {
                // Hive dirt draws a wall against ANYTHING that isn't Hive dirt
                counterpart = (n != HiveTileType.Hive);
            }
            else
            {
                // Rooms draw a wall against Hive dirt and Empty space
                counterpart = (n == HiveTileType.Hive || n == HiveTileType.None);
            }
            if (!counterpart) mask |= (1 << i);
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
