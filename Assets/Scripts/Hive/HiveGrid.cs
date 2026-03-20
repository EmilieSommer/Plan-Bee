using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Manages the hive tile grid. Handles marking tiles for construction
/// and tracks the state of every hive cell.
/// </summary>
public class HiveGrid : MonoBehaviour
{
    public static HiveGrid Instance { get; private set; }

    [Header("Tilemaps")]
    [SerializeField] private Tilemap hiveTilemap;       // Visually shows built hive tiles
    [SerializeField] private Tilemap markedTilemap;     // Overlay showing tiles marked for building

    [Header("Tiles")]
    [SerializeField] private TileBase builtTile;
    [SerializeField] private TileBase markedTile;
    [SerializeField] private TileBase underConstructionTile;

    [Header("Starting Hive")]
    [SerializeField] private Vector3Int startingCenter = Vector3Int.zero;
    [SerializeField] private int startingRadius = 1; // 1 = 3x3 grid

    // All known tile data, keyed by grid position
    private Dictionary<Vector3Int, HiveTileData> tiles = new();

    // Tiles waiting to be built (ordered queue for builders)
    private Queue<Vector3Int> buildQueue = new();

    public event System.Action<Vector3Int> OnTileBuilt;
    public event System.Action<Vector3Int> OnTileMarked;

    private static readonly Vector3Int[] Neighbours =
    {
        Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
    };

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        BuildStartingHive();
    }

    // ── Starting hive ──────────────────────────────────────────────

    private void BuildStartingHive()
    {
        for (int x = -startingRadius; x <= startingRadius; x++)
        for (int y = -startingRadius; y <= startingRadius; y++)
        {
            Vector3Int pos = startingCenter + new Vector3Int(x, y, 0);
            var data = new HiveTileData(pos);
            data.State = TileState.Built;
            data.Room = RoomType.BroodChamber;
            tiles[pos] = data;
            hiveTilemap.SetTile(pos, builtTile);
        }
    }

    // ── Marking tiles ──────────────────────────────────────────────

    /// <summary>
    /// Marks a tile for building. Must be adjacent to an existing hive tile.
    /// Returns false if the tile is invalid or already part of the hive.
    /// </summary>
    public bool MarkTile(Vector3Int pos)
    {
        if (tiles.TryGetValue(pos, out var existing) && existing.IsPartOfHive)
            return false; // already hive

        if (!IsAdjacentToHive(pos))
            return false; // not connected

        var data = tiles.ContainsKey(pos) ? tiles[pos] : new HiveTileData(pos);
        data.State = TileState.Marked;
        tiles[pos] = data;

        markedTilemap.SetTile(pos, markedTile);
        buildQueue.Enqueue(pos);
        OnTileMarked?.Invoke(pos);
        return true;
    }

    /// <summary>
    /// Called by a Builder bee when it finishes constructing a tile.
    /// </summary>
    public void CompleteTile(Vector3Int pos, RoomType room = RoomType.BroodChamber)
    {
        if (!tiles.TryGetValue(pos, out var data)) return;

        data.State = TileState.Built;
        data.Room = room;
        tiles[pos] = data;

        markedTilemap.SetTile(pos, null);
        hiveTilemap.SetTile(pos, builtTile);
        OnTileBuilt?.Invoke(pos);
    }

    /// <summary>
    /// Called by a Builder to claim the next tile in the build queue.
    /// Returns false if nothing is queued.
    /// </summary>
    public bool TryDequeueBuildJob(out Vector3Int pos)
    {
        while (buildQueue.Count > 0)
        {
            pos = buildQueue.Dequeue();
            if (tiles.TryGetValue(pos, out var d) && d.State == TileState.Marked)
            {
                d.State = TileState.UnderConstruction;
                tiles[pos] = d;
                markedTilemap.SetTile(pos, underConstructionTile);
                return true;
            }
        }
        pos = default;
        return false;
    }

    // ── Queries ────────────────────────────────────────────────────

    public bool IsHiveTile(Vector3Int pos) =>
        tiles.TryGetValue(pos, out var d) && d.State == TileState.Built;

    public bool TryGetTile(Vector3Int pos, out HiveTileData data) =>
        tiles.TryGetValue(pos, out data);

    public RoomType GetRoomAt(Vector3Int pos) =>
        tiles.TryGetValue(pos, out var d) ? d.Room : RoomType.None;

    /// <summary>Returns world-space center of a grid cell.</summary>
    public Vector3 GetWorldPosition(Vector3Int pos) =>
        hiveTilemap.GetCellCenterWorld(pos);

    /// <summary>Returns all built tiles of a given room type.</summary>
    public List<Vector3Int> GetTilesOfType(RoomType room)
    {
        var result = new List<Vector3Int>();
        foreach (var kv in tiles)
            if (kv.Value.State == TileState.Built && kv.Value.Room == room)
                result.Add(kv.Key);
        return result;
    }

    // ── Helpers ────────────────────────────────────────────────────

    private bool IsAdjacentToHive(Vector3Int pos)
    {
        foreach (var offset in Neighbours)
            if (IsHiveTile(pos + offset))
                return true;
        return false;
    }
}
