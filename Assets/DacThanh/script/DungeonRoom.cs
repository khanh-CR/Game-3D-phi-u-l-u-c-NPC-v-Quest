using System.Collections.Generic;
using UnityEngine;

public enum RoomType
{
    Empty,
    Normal,
    Boss,
    Start,
    Other
}

[System.Serializable]
public class DungeonRoom
{
    public int idRoom;

    public Vector2Int spawnPosition;

    public RoomType roomType;

    public Vector2Int size;
    public Vector2Int nodeSize;

    public int roomPadding = 2;

    public Dictionary<Vector2Int, DungeonTile> roomTiles =
        new Dictionary<Vector2Int, DungeonTile>();

    [System.NonSerialized] public List<DungeonRoom> connections = new List<DungeonRoom>();
    public List<Vector2Int> gates = new List<Vector2Int>();

    public RectInt Bounds
    {
        get
        {
            return new RectInt(
                spawnPosition,
                size
            );
        }
    }

    public DungeonRoom(
        Vector2Int spawnPosition,
        Vector2Int size,
        RectInt bounds)
    {
        this.spawnPosition = spawnPosition;

        this.size = new Vector2Int(
            Mathf.Max(size.x, 5),
            Mathf.Max(size.y, 5)
        );

        CreateRoomTiles();
    }

    private void CreateRoomTiles()
    {
        roomTiles.Clear();

        for (int x = Bounds.xMin; x < Bounds.xMax; x++)
        {
            for (int z = Bounds.yMin; z < Bounds.yMax; z++)
            {
                DungeonTileType type;

                if (x == Bounds.xMin ||
                    x == Bounds.xMax - 1 ||
                    z == Bounds.yMin ||
                    z == Bounds.yMax - 1)
                {
                    type = DungeonTileType.Wall;
                }
                else
                {
                    type = DungeonTileType.Floor;
                }

                Vector2Int position = new Vector2Int(x, z);

                roomTiles.Add(position, new DungeonTile(type, position));
            }
        }
    }
}
