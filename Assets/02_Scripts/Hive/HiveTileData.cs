using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileState
{
    Empty,       // Outside the hive — not built, not marked
    Marked,      // Player has marked this tile for building
    UnderConstruction,
    Built
}

public enum RoomType
{
    None,
    BroodChamber,
    HoneyStorage,
    PollenStore,
    DronePost
}

[System.Serializable]
public class HiveTileData
{
    public Vector3Int GridPosition;
    public TileState State;
    public RoomType Room;
    public float HP;
    public float MaxHP;

    public HiveTileData(Vector3Int pos, float maxHP = 100f)
    {
        GridPosition = pos;
        State = TileState.Empty;
        Room = RoomType.None;
        MaxHP = maxHP;
        HP = maxHP;
    }

    public bool IsPartOfHive => State == TileState.Built || State == TileState.UnderConstruction;
}
