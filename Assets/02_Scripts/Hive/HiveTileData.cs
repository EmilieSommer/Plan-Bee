using UnityEngine;

public enum TileState
{
    Outside,           // Not part of the hive
    Solid,             // Unexcavated hive wall — bees cannot pass
    Passage,           // Dug out corridor — bees can walk through
    Room,              // Designated functional room
    MarkedForSolid,    // Player marked outside tile to extend hive wall
    MarkedForPassage,  // Player marked solid tile to dig a corridor
    MarkedForRoom      // Player marked passage tile to build a room
}

public enum RoomType
{
    None,
    Brood,
    Storage
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
        State        = TileState.Outside;
        Room         = RoomType.None;
        MaxHP        = maxHP;
        HP           = maxHP;
    }

    public bool IsMarked =>
        State == TileState.MarkedForSolid ||
        State == TileState.MarkedForPassage ||
        State == TileState.MarkedForRoom;

    public bool IsWalkable =>
        State == TileState.Passage || State == TileState.Room;

    public bool IsPartOfHive =>
        State == TileState.Solid   ||
        State == TileState.Passage ||
        State == TileState.Room    ||
        IsMarked;
}
