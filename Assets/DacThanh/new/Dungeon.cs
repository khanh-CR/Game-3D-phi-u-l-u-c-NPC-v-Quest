using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public enum dungeonType
{
    Minor = 15,
    Lesser = 25,
    Medium = 30,
    Huge = 40,
    Large = 55,
    Ultra = 60
}

[System.Serializable]
public class Dungeon
{
    // Attribute
    public int Seed;
    public DungeonData dungeonData;
    public int Level;
    public int roomCount;
    public dungeonType Type;
    public Vector2Int dungeonSize;
    public DungeonRoom startRoom, endRoom;

    // List
    // Key = (x, z) trong thế giới 3D
    public Dictionary<Vector2Int, DungeonTile> dungeonTiles = new Dictionary<Vector2Int, DungeonTile>();
    public List<DungeonRoom> rooms = new List<DungeonRoom>();

    public BSPDungeonGenerators bspGenerator;

    // =========================================================
    // CONSTRUCTOR - CÓ SEED
    // =========================================================
    public Dungeon(int seed, int level, DungeonData dungeonData)
    {
        Seed = seed;

        // Mọi Random.* trong BSP / Corridor / InitRoomId dùng chung state này,
        // nên cùng seed => cùng dungeon. Xong thì trả state cũ lại.
        Random.State previous = Random.state;
        Random.InitState(seed);

        Build(level, dungeonData);

        Random.state = previous;
    }

    private void Build(int level, DungeonData data)
    {
        if (level <= 0) level = 1;

        Level = level;
        Type = Dungeontype(level);
        roomCount = Random.Range(Level, Level * Level + 1);

        dungeonSize = new Vector2Int(Level * (int)Type, Level * (int)Type);
        dungeonData = data;

        // 1. Tạo tile
        CreateDungeonTiles();

        // 2. BSP tạo room (minLeafSize, minRoomSize, roomPadding)
        bspGenerator = new BSPDungeonGenerators(this, 20, 15, 1);
        bspGenerator.Generate();

        // 3. Lấy list room
        rooms = bspGenerator.rooms;

        // 4. Copy tile của room vào dungeon
        CopyRoomTilesToDungeon();

        // 5. Tạo ID sau khi room đã xong
        InitRoomId();

        // 6. Corridor + tường quanh corridor
        GenerateCorridors();

        Debug.Log($"Seed: {Seed}, Level: {Level}, Size: {dungeonSize}, Rooms: {rooms.Count}");
    }

    // =========================================================
    // INIT ROOM ID
    // =========================================================
    private void InitRoomId()
    {
        List<int> ids = Enumerable
            .Range(0, rooms.Count)
            .OrderBy(x => Random.value)
            .ToList();

        for (int i = 0; i < rooms.Count; i++)
        {
            rooms[i].idRoom = ids[i];
        }
    }

    private void GenerateCorridors()
    {
        var gen = new CorridorGenerator(this);   // (this, loopChance, maxLoopDistance, corridorHalfWidth)
        gen.Generate(bspGenerator.Root);
        startRoom = gen.StartRoom;
        endRoom = gen.EndRoom;
    }

    // =========================================================
    // DUNGEON TYPE
    // =========================================================
    private dungeonType Dungeontype(int level)
    {
        if (level <= 1) return dungeonType.Minor;
        if (level <= 4) return dungeonType.Lesser;
        if (level <= 6) return dungeonType.Medium;
        if (level <= 8) return dungeonType.Huge;
        if (level <= 9) return dungeonType.Large;
        return dungeonType.Ultra;
    }

    // =========================================================
    // DUNGEON BOUNDS
    // =========================================================
    public RectInt Bounds
    {
        get
        {
            return new RectInt(Vector2Int.zero, dungeonSize);
        }
    }

    // =========================================================
    // CREATE DUNGEON TILES
    // =========================================================
    private void CreateDungeonTiles()
    {
        dungeonTiles.Clear();

        for (int x = Bounds.xMin; x < Bounds.xMax; x++)
        {
            for (int y = Bounds.yMin; y < Bounds.yMax; y++)
            {
                bool isBorder =
                    x == Bounds.xMin ||
                    x == Bounds.xMax - 1 ||
                    y == Bounds.yMin ||
                    y == Bounds.yMax - 1;

                DungeonTileType type = isBorder
                    ? DungeonTileType.Wall
                    : DungeonTileType.Empty;

                Vector2Int position = new Vector2Int(x, y);
                dungeonTiles.Add(position, new DungeonTile(type, position));
            }
        }
    }

    // =========================================================
    // COPY ROOM TILES -> DUNGEON
    // =========================================================
    private void CopyRoomTilesToDungeon()
    {
        foreach (DungeonRoom room in rooms)
        {
            foreach (DungeonTile roomTile in room.roomTiles.Values)
            {
                if (!dungeonTiles.ContainsKey(roomTile.Position))
                    continue;

                DungeonTile dungeonTile = dungeonTiles[roomTile.Position];
                dungeonTile.Type = roomTile.Type;
                dungeonTiles[roomTile.Position] = dungeonTile;
            }
        }
    }
}
