using UnityEngine;

public enum DungeonTileType
{
    Empty,
    Floor,
    Wall,
    Corridor,
    Gate
}

public struct DungeonTile
{
    public DungeonTileType Type;
    public Vector2Int Position;

    public int X => Position.x;
    public int Z => Position.y;

    public DungeonTile(DungeonTileType type, Vector2Int position)
    {
        Type = type;
        Position = position;
    }

    public bool IsWalkable
    {
        get
        {
            return Type == DungeonTileType.Floor ||
                   Type == DungeonTileType.Corridor;
        }
    }

    public Vector3Int GetBlockPosition(int y)
    {
        return new Vector3Int(Position.x, y, Position.y);
    }
}
