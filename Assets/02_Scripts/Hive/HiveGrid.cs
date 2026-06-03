using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Manages the hive tile grid.
/// Handles placement, autotiling (same-type merging), and build-queue handoff.
/// </summary>
public class HiveGrid : MonoBehaviour
{
    public static HiveGrid Instance { get; private set; }

    [Header("Visuals")]
    [SerializeField] private HiveVisuals visuals;

    // ── State ─────────────────────────────────────────────────────────────────

    // tile type for every grid cell that exists
    private readonly Dictionary<Vector3Int, HiveTileType> types  = new();
    // construction status
    private readonly Dictionary<Vector3Int, bool>         marked = new();
    // cells that cannot be demolished or modified (e.g. the hive entrance)
    private readonly HashSet<Vector3Int> lockedCells = new();

    // build queue consumed by builder bees
    private readonly Queue<Vector3Int> buildQueue = new();

    public event System.Action<Vector3Int> OnTileBuilt;
    public event System.Action<Vector3Int> OnTileMarked;

    static readonly Vector3Int[] Dirs =
        { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

    // ── Unity ─────────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (visuals == null) visuals = GetComponent<HiveVisuals>();
    }

    void Start()
    {
        ScanPaintedTiles();
    }

    // ── Starting hive ─────────────────────────────────────────────────────────

    /// <summary>
    /// Reads whatever was painted on the Built tilemap in the scene, identifies
    /// each cell's HiveTileType via the library's sprite reverse-lookup, and
    /// registers them as the starting hive. Then re-renders every cell so the
    /// autotile picks the correct edge variant for each.
    /// </summary>
    void ScanPaintedTiles()
    {
        if (visuals == null || visuals.BuiltTilemap == null || visuals.Library == null) return;

        var tilemap = visuals.BuiltTilemap;
        var library = visuals.Library;
        library.Init();

        // Collect first, render after — masks depend on neighbors being registered.
        var found = new List<(Vector3Int pos, HiveTileType type)>();

        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            var sprite = tilemap.GetSprite(pos);
            if (sprite == null) continue;
            var type = library.GetTypeFromSprite(sprite);
            if (type == HiveTileType.None) continue;

            types[pos]  = type;
            marked[pos] = false;
            found.Add((pos, type));
        }

        // Auto-detect the entrance: any InsideHive cell from the starting layout
        // that touches at least one empty neighbor is the entrance and is locked.
        foreach (var (pos, type) in found)
        {
            if (type != HiveTileType.InsideHive) continue;
            foreach (var d in Dirs)
            {
                if (!types.ContainsKey(pos + d)) { lockedCells.Add(pos); break; }
            }
        }

        // Now redraw each so the correct variant is chosen for its actual neighbors.
        foreach (var (pos, _) in found) visuals.RefreshAt(pos);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by BuildPanel when the player clicks a grid cell.
    /// Returns true if the tile was accepted (adjacent to any existing tile).
    /// </summary>
    public bool TryMark(Vector3Int pos, HiveTileType type)
    {
        if (types.ContainsKey(pos))           return false;   // already placed
        if (!IsAdjacentToAny(pos))            return false;   // not connected

        types[pos]  = type;
        marked[pos] = true;

        visuals?.SetMarked(pos, type);

        buildQueue.Enqueue(pos);
        OnTileMarked?.Invoke(pos);
        return true;
    }

    /// <summary>
    /// Builder bees call this to claim the next job.
    /// </summary>
    public bool TryDequeueBuildJob(out Vector3Int pos)
    {
        while (buildQueue.Count > 0)
        {
            pos = buildQueue.Dequeue();
            if (marked.ContainsKey(pos) && marked[pos])
                return true;
        }
        pos = default;
        return false;
    }

    /// <summary>
    /// Called when a builder bee finishes constructing a tile.
    /// </summary>
    public void CompleteBuild(Vector3Int pos)
    {
        if (!types.ContainsKey(pos)) return;

        marked[pos] = false;
        visuals?.ClearMarked(pos);
        Place(pos, types[pos]);
        OnTileBuilt?.Invoke(pos);
    }

    public bool HasTile(Vector3Int pos)    => types.ContainsKey(pos);
    public bool IsMarked(Vector3Int pos)   => marked.TryGetValue(pos, out var m) && m;
    public bool CanBuildAt(Vector3Int pos) => !types.ContainsKey(pos) && IsAdjacentToAny(pos);
    public bool IsLocked(Vector3Int pos)   => lockedCells.Contains(pos);

    public HiveTileType GetType(Vector3Int pos) =>
        types.TryGetValue(pos, out var t) ? t : HiveTileType.None;

    public Vector3Int WorldToCell(Vector3 world) =>
        GetComponent<Grid>()?.WorldToCell(world) ?? Vector3Int.zero;

    public Vector3 CellToWorld(Vector3Int cell) =>
        GetComponent<Grid>()?.GetCellCenterWorld(cell) ?? Vector3.zero;

    // ── Internals ─────────────────────────────────────────────────────────────

    void Place(Vector3Int pos, HiveTileType type)
    {
        if (!types.ContainsKey(pos)) types[pos] = type;
        if (!marked.ContainsKey(pos)) marked[pos] = false;

        visuals?.SetBuilt(pos, type);
    }

    bool IsAdjacentToAny(Vector3Int pos)
    {
        if (types.Count == 0) return true;
        foreach (var d in Dirs)
            if (types.ContainsKey(pos + d)) return true;
        return false;
    }
}
