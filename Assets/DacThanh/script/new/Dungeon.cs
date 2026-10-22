using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public enum dungeonSize
{
    Minor = 15,
    Lesser = 25,
    Medium = 30,
    Huge = 40,
    Large = 55,
    Ultra = 60
}
public class CorridorLink
{
    public readonly DungeonRoom A;
    public readonly DungeonRoom B;

    public CorridorLink(DungeonRoom a, DungeonRoom b)
    {
        A = a;
        B = b;
    }
}

[System.Serializable]
public class Dungeon
{
    #region Attributes and constructor 
    // Attribute
    public int Seed;
    public int Level;
    public DungeonData dungeonData;

    public dungeonSize dungeonSize;
    public Vector2Int size;
    public DungeonRoom startRoom, endRoom;
    public Dictionary<int, DungeonRoom> roomById = new Dictionary<int, DungeonRoom>();
    public List<DungeonRoom> rooms = new List<DungeonRoom>();
    // Tile Corridor / Wall bao quanh corridor -> corridor sở hữu nó
    public Dictionary<Vector2Int, DungeonTile> dungeonTiles = new Dictionary<Vector2Int, DungeonTile>();
    public Dictionary<Vector2Int, CorridorLink> corridorOwner = new Dictionary<Vector2Int, CorridorLink>();
    public BSPDungeonGenerators bspGenerator;
    
    #endregion
    
    #region Constructors
    public Dungeon(int seed, int level, DungeonData dungeonData)
    {
        Seed = seed;
        Random.State previous = Random.state;
        Random.InitState(seed);
        Build(level, dungeonData);
        Random.state = previous;
    }
    public RectInt Bounds
    {
        get
        {
            return new RectInt(Vector2Int.zero, size);
        }
    }
    #endregion
    
    #region Methods
    private void Build(int level, DungeonData data)
    {
        if (level <= 0) level = 1;
        else if (level > 8)
        {
            level = 7;
        }
        Level = level;
        dungeonSize = Dungeonsize(level);
        size = new Vector2Int(Level * (int)dungeonSize, Level * (int)dungeonSize);
        dungeonData = data;
        CreateDungeonTiles();
        GenerateRooms();
        InitRoomId();
        CopyRoomTilesToDungeon();
        GenerateCorridors();
        Debug.Log($"Seed: {Seed}, Level: {Level}, Size: {size}, Rooms: {rooms.Count}");
    }
    private dungeonSize Dungeonsize(int level)
        {
            if (level == 1) return dungeonSize.Minor;
            if (level == 2) return dungeonSize.Lesser;
            if (level == 3) return dungeonSize.Medium;
            if (level <= 5) return dungeonSize.Huge;
            if (level <= 7) return dungeonSize.Large;
            return dungeonSize.Ultra;
        } //size dungeon
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
    } // tạo khung dungeon
    private void GenerateRooms()
        {
            //BSP tạo room (minLeafSize, minRoomSize, roomPadding)
            bspGenerator = new BSPDungeonGenerators(this, 20, 15, 1);
            bspGenerator.Generate();
        } // phân lô sau đó tạo rooms cho dungeon
    
    private void InitRoomId()
    {
        rooms = bspGenerator.rooms;
        List<int> ids = Enumerable.Range(0, rooms.Count).OrderBy(x => Random.value).ToList();
        for (int i = 0; i < rooms.Count; i++)
        {
            rooms[i].idRoom = ids[i];
        }
        roomById.Clear();
        for (int i = 0; i < rooms.Count; i++)
        {
            rooms[i].idRoom = ids[i];
            roomById[ids[i]] = rooms[i];
        }
    } // gán id cho từng room

    private void GenerateCorridors()
    {
        // (this, loopChance, maxLoopDistance, corridorHalfWidth)
        var gen = new CorridorGenerator(this);   
        gen.Generate(bspGenerator.Root);
        startRoom = gen.StartRoom;
        endRoom = gen.EndRoom;
    } // tạo lối đi cho dungeon

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
    #endregion
    
    #region Tra cứu

    public bool TryGetRoom(int id, out DungeonRoom room)
    {
        return roomById.TryGetValue(id, out room);
    }

    public bool TryGetTile(Vector2Int pos, out DungeonTile tile)
    {
        return dungeonTiles.TryGetValue(pos, out tile);
    }
    #endregion
}