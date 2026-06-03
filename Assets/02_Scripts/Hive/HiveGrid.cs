using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Manages the hive tile grid.
/// Handles placement, tile logic, and builder assignments.
/// </summary>
public class HiveGrid : MonoBehaviour
{
    public static HiveGrid Instance { get; private set; }

    [Header("Tilemaps")]
    [SerializeField] private Tilemap builtTilemap;
    [SerializeField] private Tilemap markedTilemap;

    [Header("Library")]
    [SerializeField] private HiveTileLibrary library;

    [Header("Starting Hive")]
    [SerializeField] private Vector3Int startCenter = Vector3Int.zero;
    [SerializeField] private int startRadius = 1;
    [SerializeField] private HiveTileType startType = HiveTileType.InsideHive;

    [Header("Build Settings")]
    [SerializeField] private float buildTimePerTile = 5f;

    // ── State ─────────────────────────────────────────────────────────────────

    // tile type for every grid cell that exists
    private readonly Dictionary<Vector3Int, HiveTileType> types  = new();
    
    // construction status (true = marked for building, false = fully built)
    private readonly Dictionary<Vector3Int, bool>         marked = new();

    // tracks build progress (0 to buildTimePerTile)
    private readonly Dictionary<Vector3Int, float> buildProgress = new();

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
        library.Init();
    }

    void Start()
    {
        BuildStartingHive();
    }

    // ── Starting hive ─────────────────────────────────────────────────────────

    void BuildStartingHive()
    {
        for (int x = -startRadius; x <= startRadius; x++)
        for (int y = -startRadius; y <= startRadius; y++)
            Place(startCenter + new Vector3Int(x, y, 0), startType);
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
        buildProgress[pos] = 0f;

        var tile = library.GetMarkedTile(type);
        markedTilemap.SetTile(pos, tile);

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
    /// Called by builder bees every frame they are at the tile working.
    /// </summary>
    public void AddBuildProgress(Vector3Int pos, float amount)
    {
        if (!marked.TryGetValue(pos, out bool isMarked) || !isMarked) return;

        if (!buildProgress.ContainsKey(pos)) buildProgress[pos] = 0f;
        buildProgress[pos] += amount;

        // Optional: you can change markedTilemap alpha here if you want visual fade
        
        if (buildProgress[pos] >= buildTimePerTile)
        {
            CompleteBuild(pos);
        }
    }

    /// <summary>
    /// Called when a tile has received enough progress.
    /// </summary>
    private void CompleteBuild(Vector3Int pos)
    {
        if (!types.ContainsKey(pos)) return;

        marked[pos] = false;
        markedTilemap.SetTile(pos, null);
        
        Place(pos, types[pos]);
        OnTileBuilt?.Invoke(pos);
    }

    public bool HasTile(Vector3Int pos)  => types.ContainsKey(pos);
    public bool IsMarked(Vector3Int pos) => marked.TryGetValue(pos, out var m) && m;

    public HiveTileType GetType(Vector3Int pos) =>
        types.TryGetValue(pos, out var t) ? t : HiveTileType.None;

    public Vector3Int WorldToCell(Vector3 world) =>
        builtTilemap.WorldToCell(world);

    public Vector3 CellToWorld(Vector3Int cell) =>
        builtTilemap.GetCellCenterWorld(cell);

    // ── Internals ─────────────────────────────────────────────────────────────

    void Place(Vector3Int pos, HiveTileType type)
    {
        if (!types.ContainsKey(pos)) types[pos] = type;
        if (!marked.ContainsKey(pos)) marked[pos] = false;

        // Set the RuleTile on the tilemap, Unity handles the adjacency visuals!
        builtTilemap.SetTile(pos, library.GetBuiltTile(type));
    }

    bool IsAdjacentToAny(Vector3Int pos)
    {
        if (types.Count == 0) return true;   // first tile always allowed
        foreach (var d in Dirs)
            if (types.ContainsKey(pos + d)) return true;
        return false;
    }
}
