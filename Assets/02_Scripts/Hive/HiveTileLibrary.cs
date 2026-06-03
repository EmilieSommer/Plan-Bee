using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Holds the Rule Tiles and Marked Tiles for each HiveTileType.
/// </summary>
[CreateAssetMenu(fileName = "HiveTileLibrary", menuName = "Plan Bee/Hive Tile Library")]
public class HiveTileLibrary : ScriptableObject
{
    [System.Serializable]
    public class TileSet
    {
        public HiveTileType type;

        [Tooltip("The built tile (usually a Rule Tile for autoconnecting).")]
        public TileBase builtTile;

        [Tooltip("Shown while tile is marked / under construction.")]
        public TileBase markedTile;
    }

    public TileSet[] sets;

    // ── Runtime lookup ────────────────────────────────────────────────────────

    private Dictionary<HiveTileType, TileSet> _lookup;

    public void Init()
    {
        _lookup = new Dictionary<HiveTileType, TileSet>();
        if (sets == null) return;
        foreach (var s in sets)
            _lookup[s.type] = s;
    }

    public TileBase GetBuiltTile(HiveTileType type)
    {
        if (_lookup == null) Init();
        return _lookup.TryGetValue(type, out var set) ? set.builtTile : null;
    }

    public TileBase GetMarkedTile(HiveTileType type)
    {
        if (_lookup == null) Init();
        return _lookup.TryGetValue(type, out var set) ? set.markedTile : null;
    }
}
