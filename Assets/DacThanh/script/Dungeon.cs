using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public enum dungeonType
{
    Minor = 10,
    Lesser = 11,
    Medium = 12,
    Huge = 13,
    Large = 14,
    Ultra = 15
}

[System.Serializable]
public class Dungeon
{
    public DungeonData dungeonData;

    public int Level;

    public int roomCount;

    public dungeonType Type;

    public Vector2Int dungeonSize;

    public DungeonRoom startRoom, endRoom;

    public Dictionary<Vector2Int, DungeonTile> dungeonTiles =
        new Dictionary<Vector2Int, DungeonTile>();

    public List<DungeonRoom> rooms = new List<DungeonRoom>();

    public BSPDungeonGenerators bspGenerator;

    public Dungeon(int level, DungeonData dungeonData)
    {
        Init(level, dungeonData);
    }

    public Dungeon(int seed, int level, DungeonData dungeonData)
    {
        Random.InitState(seed);
        Init(level, dungeonData);
    }

    private void Init(int level, DungeonData data)
    {
        if (level <= 0)
        {
            level = 1;
        }

        Level = level;

        Type = Dungeontype(level);

        roomCount = Random.Range(Level, Level * Level + 1);

        dungeonSize = new Vector2Int(
            Level * (int)Type,
            Level * (int)Type
        );

        dungeonData = data;

        CreateDungeonTiles();

        bspGenerator = new BSPDungeonGenerators(this, 12, 7, 1);

        bspGenerator.Generate();

        rooms = bspGenerator.rooms;

        CopyRoomTilesToDungeon();

        InitRoomId();

        GenerateCorridors();

        Debug.Log(
            $"Level: {Level}, " +
            $"RoomCount: {roomCount}, " +
            $"DungeonSize: {dungeonSize}, " +
            $"DungeonType: {Type}, " +
            $"RoomGenerated: {rooms.Count}"
        );
    }

    private void InitRoomId()
    {
        List<int> ids =
            Enumerable
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
        CorridorGenerator gen = new CorridorGenerator(this);
        gen.Generate(bspGenerator.Root);
        startRoom = gen.StartRoom;
        endRoom = gen.EndRoom;
    }

    private dungeonType Dungeontype(int level)
    {
        if (level <= 2) return dungeonType.Minor;
        if (level <= 5) return dungeonType.Lesser;
        if (level <= 9) return dungeonType.Medium;
        if (level <= 13) return dungeonType.Huge;
        if (level <= 17) return dungeonType.Large;
        return dungeonType.Ultra;
    }

    public RectInt Bounds
    {
        get
        {
            return new RectInt(Vector2Int.zero, dungeonSize);
        }
    }

    private void CreateDungeonTiles()
    {
        dungeonTiles.Clear();

        for (int x = Bounds.xMin; x < Bounds.xMax; x++)
        {
            for (int z = Bounds.yMin; z < Bounds.yMax; z++)
            {
                bool isBorder =
                    x == Bounds.xMin ||
                    x == Bounds.xMax - 1 ||
                    z == Bounds.yMin ||
                    z == Bounds.yMax - 1;

                DungeonTileType type = isBorder
                    ? DungeonTileType.Wall
                    : DungeonTileType.Empty;

                Vector2Int position = new Vector2Int(x, z);

                dungeonTiles.Add(position, new DungeonTile(type, position));
            }
        }
    }

    private void CopyRoomTilesToDungeon()
    {
        foreach (DungeonRoom room in rooms)
        {
            foreach (DungeonTile roomTile in room.roomTiles.Values)
            {
                if (!dungeonTiles.ContainsKey(roomTile.Position))
                {
                    continue;
                }

                DungeonTile dungeonTile = dungeonTiles[roomTile.Position];
                dungeonTile.Type = roomTile.Type;
                dungeonTiles[roomTile.Position] = dungeonTile;
            }
        }
    }
}
