using System.Collections.Generic;
using UnityEngine;

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

        [Tooltip("16 sprites indexed by bitmask (T=8 B=4 L=2 R=1). " +
                 "Index 0 = no neighbours, index 15 = all neighbours.")]
        public Sprite[] variants = new Sprite[16];

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

    public Sprite GetSprite(HiveTileType type, int bitmask)
    {
        if (_lookup == null) Init();
        if (_lookup.TryGetValue(type, out var set) &&
            bitmask >= 0 && bitmask < set.variants.Length)
            return set.variants[bitmask];
        return null;
    }

    public Sprite GetMarkedSprite(HiveTileType type)
    {
        if (_lookup == null) Init();
        return _lookup.TryGetValue(type, out var set) ? set.markedSprite : null;
    }
}
