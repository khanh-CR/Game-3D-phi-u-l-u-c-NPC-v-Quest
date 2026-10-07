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
    [Header( " thông tin Room")]
    
    public int idRoom;

    private Vector2Int spawnPosition;
    public Vector2Int Min => spawnPosition;  
    public Vector2Int Max => new Vector2Int(spawnPosition.x + size.x - 1,
        spawnPosition.y + size.y - 1);
    public int Width => size.x;
    public int Height => size.y;
    [Header( " thông tin tọa độ sàn")]
    public Vector2Int InnerMin => Min + Vector2Int.one;
    public Vector2Int InnerMax => Max - Vector2Int.one;
    public Vector2Int InnerSize => size - new Vector2Int(2, 2);
    public bool Contains(Vector2Int pos)
    {
        return pos.x >= Min.x && pos.x <= Max.x &&
               pos.y >= Min.y && pos.y <= Max.y;
    }
    public Vector2Int LocalToTile(int localX, int localY)
    {
        return new Vector2Int(Min.x + localX, Min.y + localY);
    }
    public RoomType roomType;

    public Vector2Int size;
    public Vector2Int nodeSize;

    public int roomPadding = 2;

    // Key = vị trí Tile, Value = DungeonTile
    public Dictionary<Vector2Int, DungeonTile> roomTiles = new Dictionary<Vector2Int, DungeonTile>();

    [System.NonSerialized] public List<DungeonRoom> connections = new List<DungeonRoom>();
    public List<Vector2Int> gates = new List<Vector2Int>();

    public RectInt Bounds
    {
        get
        {
            return new RectInt(spawnPosition, size);
        }
    }

    public DungeonRoom(Vector2Int spawnPosition, Vector2Int size, RectInt bounds)
    {
        this.spawnPosition = spawnPosition;

        this.size = new Vector2Int(
            Mathf.Max(size.x, 5),
            Mathf.Max(size.y, 5)
        );

        // ID chưa được tạo ở đây, InitRoomId() trong Dungeon sẽ tạo sau.
        CreateRoomTiles();
    }

    private void CreateRoomTiles()
    {
        roomTiles.Clear();

        for (int x = Bounds.xMin; x < Bounds.xMax; x++)
        {
            for (int y = Bounds.yMin; y < Bounds.yMax; y++)
            {
                DungeonTileType type;

                // Viền Room = Wall
                if (x == Bounds.xMin ||
                    x == Bounds.xMax - 1 ||
                    y == Bounds.yMin ||
                    y == Bounds.yMax - 1)
                {
                    type = DungeonTileType.Wall;
                }
                else
                {
                    type = DungeonTileType.Floor;
                }

                Vector2Int position = new Vector2Int(x, y);
                roomTiles.Add(position, new DungeonTile(type, position));
            }
        }
    }
}
