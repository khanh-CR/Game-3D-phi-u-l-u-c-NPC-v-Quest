using UnityEngine;

public enum DungeonTileType
{
    Empty,
    Floor,
    Wall,
    Corridor,
    Gate
}

// Position.x = x ngoài thế giới 3D, Position.y = z ngoài thế giới 3D
public struct DungeonTile
{
    public DungeonTileType Type;
    public Vector2Int Position;

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
}
