using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sprite tables for each HiveTileType.
/// Two 16-slot arrays per type, indexed by a 4-bit cardinal mask.
///
/// Bit layout (used for both border and overlay arrays):
///   bit 0 = top, bit 1 = bottom, bit 2 = left, bit 3 = right
///
/// Border array:
///   indexed by "connected mask" — bit set if that side is connected
///   (i.e., NOT facing the tile's border counterpart).
///   For rooms: counterpart = Hive       → border shows on Hive-facing sides.
///   For Hive : counterpart = empty cell → border shows on empty-facing sides.
///
/// Overlay array (rooms only — Hive is unused):
///   indexed by "overlay mask" — bit set if that side faces a different room type.
///   The overlay PNG is transparent on non-overlay sides, so it stacks cleanly
///   on top of the border layer.
/// </summary>
[CreateAssetMenu(fileName = "HiveTileLibrary", menuName = "Plan Bee/Hive Tile Library")]
public class HiveTileLibrary : ScriptableObject
{
    [System.Serializable]
    public class TileSet
    {
        public HiveTileType type;

        [Tooltip("16 sprites indexed by connected-side bitmask (T=1 B=2 L=4 R=8).")]
        public Sprite[] border = new Sprite[16];

        [Tooltip("16 sprites indexed by overlay-side bitmask (sides facing a different room).")]
        public Sprite[] overlay = new Sprite[16];
    }

    public TileSet[] sets;

    private Dictionary<HiveTileType, TileSet> _lookup;
    private Dictionary<Sprite, HiveTileType>  _spriteToType;

    public void Init()
    {
        _lookup = new Dictionary<HiveTileType, TileSet>();
        _spriteToType = new Dictionary<Sprite, HiveTileType>();
        if (sets == null) return;
        foreach (var s in sets)
        {
            _lookup[s.type] = s;
            if (s.border != null)
                foreach (var sp in s.border)
                    if (sp != null && !_spriteToType.ContainsKey(sp)) _spriteToType[sp] = s.type;
            if (s.overlay != null)
                foreach (var sp in s.overlay)
                    if (sp != null && !_spriteToType.ContainsKey(sp)) _spriteToType[sp] = s.type;
        }
    }

    /// <summary>Reverse lookup — used to identify painted starting tiles.</summary>
    public HiveTileType GetTypeFromSprite(Sprite sprite)
    {
        if (_spriteToType == null) Init();
        if (sprite == null) return HiveTileType.None;
        return _spriteToType.TryGetValue(sprite, out var t) ? t : HiveTileType.None;
    }

    public Sprite GetBorderSprite(HiveTileType type, int mask)
    {
        if (_lookup == null) Init();
        if (!_lookup.TryGetValue(type, out var set) || set.border == null) return null;
        return mask >= 0 && mask < set.border.Length ? set.border[mask] : null;
    }

    public Sprite GetOverlaySprite(HiveTileType type, int mask)
    {
        if (_lookup == null) Init();
        if (!_lookup.TryGetValue(type, out var set) || set.overlay == null) return null;
        return mask >= 0 && mask < set.overlay.Length ? set.overlay[mask] : null;
    }
}
