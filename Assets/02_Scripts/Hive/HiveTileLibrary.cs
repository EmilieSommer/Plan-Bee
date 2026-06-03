using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Holds the 16 autotile sprite variants for each HiveTileType.
/// Sprites are indexed 0-15 by connection bitmask: T=8  B=4  L=2  R=1.
/// Populate once via Tools → Plan Bee → Populate Tile Library.
/// </summary>
[CreateAssetMenu(fileName = "HiveTileLibrary", menuName = "Plan Bee/Hive Tile Library")]
public class HiveTileLibrary : ScriptableObject
{
    [System.Serializable]
    public class TileSet
    {
        public HiveTileType type;

        [Tooltip("RuleTile for the built zone — autotiles against same-type neighbors.")]
        public TileBase builtTile;

        [Tooltip("RuleTile for the overlay layer — transparent edges where zones meet.")]
        public TileBase overlayTile;

        [Tooltip("Shown while tile is marked / under construction.")]
        public Sprite markedSprite;
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

    public TileBase GetOverlayTile(HiveTileType type)
    {
        if (_lookup == null) Init();
        return _lookup.TryGetValue(type, out var set) ? set.overlayTile : null;
    }

    public Sprite GetMarkedSprite(HiveTileType type)
    {
        if (_lookup == null) Init();
        return _lookup.TryGetValue(type, out var set) ? set.markedSprite : null;
    }
}
