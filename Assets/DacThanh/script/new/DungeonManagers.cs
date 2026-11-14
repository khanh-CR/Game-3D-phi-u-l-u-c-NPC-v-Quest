using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonManagers : MonoBehaviour
{
    #region Attibute

    public GameObject player;
    public static DungeonManagers instance;
    [Header("Seed")]
    public int MasterSeed;
    public bool randomSeed = true;
    [Header("Floor")]
    public int currentFloor = 1;
    [SerializeField] float playerSpawnY = 0.5f;   // chỉnh theo chiều cao sàn/pivot player
    [SerializeField] float portalY = 1f;
    private DungeonData runData;     // data cố định của run (null => chọn theo seed từng tầng)
    private int runLevel;
    private bool portalSpawned;
    [Header("Dungeon")]
    [SerializeField] List<DungeonData> listDungeonsData;
    public DungeonData data;
    [SerializeField] int Level = 1;
    [SerializeField] int wallHeight = 5;     // số block tường theo trục Y (mặc định 5)
    private const float tileSize = 1f;
    [SerializeField] bool generateOnStart = false;
    [Header("Light")]
    [SerializeField] float lightHeight = 3f;      // độ cao đèn (tính theo ô, từ mặt sàn)
    [SerializeField] float lightRange = 15f;
    [SerializeField] float lightIntensity = 2f;
    [SerializeField] Color lightColor = Color.white;

    public Dungeon dungeon;
    public List<DungeonRoom> dungeonRooms;
    private RenderDungeon dungeonRenderer;
    #endregion

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (generateOnStart) DungeonGenerator();
    }
    private DungeonRoom activeRoom;

    public void EnterRoom(DungeonRoom room) => activeRoom = room;
    public void ExitRoom(DungeonRoom room) { if (activeRoom == room) activeRoom = null; }
    private void Update()
    {
        if (activeRoom == null) return;
        // CheckRoomWave chỉ trả true khi hết quái và wave < MaxWave => spawn wave kế
        if (activeRoom.CheckRoomWave()) SpawnMonstersInRoom(activeRoom);
        if (!portalSpawned && activeRoom.roomType == RoomType.Boss && activeRoom.IsCleared)
            SpawnPortal(activeRoom);
    }

    [ContextMenu("Generate Dungeon")]
    public void DungeonGenerator()
    {
        if (randomSeed)
            MasterSeed = Random.Range(int.MinValue, int.MaxValue);

        CreateDungeon(data);
    }
    #region BuildDungeon
    public void CreateDungeon(DungeonData data)
    {
        if (data == null)
        {
            if (randomSeed) MasterSeed = Random.Range(int.MinValue, int.MaxValue);
            if (listDungeonsData == null || listDungeonsData.Count == 0)
            {
                Debug.LogError("DungeonData null file:DungeonManagers");
                return;
            }
            // Chọn data theo seed để cùng seed luôn cùng giao diện
            int index = (int)(((long)MasterSeed % listDungeonsData.Count + listDungeonsData.Count) % listDungeonsData.Count);
            data = listDungeonsData[index];

        }
        else
        {
            if (data.randomSeed)
                MasterSeed = Random.Range(int.MinValue, int.MaxValue);
            else
                MasterSeed = data.seed;
            Level = data.level;
        }
        wallHeight = Mathf.Max(1, wallHeight);
        ClearTile();
        dungeon = new Dungeon(MasterSeed, Level, data);
        dungeonRooms = dungeon.rooms;
        SortDungeonRooms();
        // Dữ liệu (Dungeon) và phần dựng ra scene (RenderDungeon) tách riêng
        dungeonRenderer = new RenderDungeon(dungeon, BuildRenderConfig());
        dungeonRenderer.Render();   
        
    }
    public void NextFloor()
    {
        currentFloor++;
        // Muốn tầng sâu hơn thì khó hơn: Level = Mathf.Min(runLevel + (currentFloor - 1) / 2, 7);
        BuildFloor();
    }
    private void BuildFloor()
    {
        int floorSeed = GetFloorSeed(MasterSeed, currentFloor);
        DungeonData floorData = runData != null ? runData : PickDataBySeed(floorSeed);

        ClearTile();              // xoá tầng cũ (kể cả quái, cổng cũ vì đều nằm dưới Root)
        activeRoom = null;
        portalSpawned = false;

        dungeon = new Dungeon(floorSeed, Level, floorData);
        dungeonRooms = dungeon.rooms;
        SortDungeonRooms();

        dungeonRenderer = new RenderDungeon(dungeon, BuildRenderConfig());
        dungeonRenderer.Render();

        PlacePlayerAtStart();
    }
    // Hash xác định (không dùng System.HashCode vì nó khác nhau mỗi lần chạy)
private static int GetFloorSeed(int masterSeed, int floor)
{
    unchecked
    {
        uint h = (uint)masterSeed;
        h ^= (uint)floor * 0x9E3779B9u;
        h ^= h >> 16; h *= 0x85EBCA6Bu;
        h ^= h >> 13; h *= 0xC2B2AE35u;
        h ^= h >> 16;
        return (int)h;
    }
}

private DungeonData PickDataBySeed(int seed)
{
    int n = listDungeonsData.Count;
    return listDungeonsData[(int)(((long)seed % n + n) % n)];
}

private void PlacePlayerAtStart()
{
    if (player == null) { Debug.LogWarning("Chưa gán player"); return; }

    DungeonRoom start = dungeon.startRoom;
    if (start == null && dungeon.rooms.Count > 0) start = dungeon.rooms[0];
    if (start == null) return;

    Vector2Int c = (start.InnerMin + start.InnerMax) / 2;
    Vector3 pos = new Vector3(c.x * tileSize, playerSpawnY, c.y * tileSize);

    var ui = player.GetComponent<PlayerUI>();
    if (ui != null) ui.currentGate = null;   // bỏ tham chiếu gate của tầng cũ

    var pt = player.GetComponent<PlayerTest>();
    if (pt != null) pt.Teleport(pos);
    else player.transform.position = pos;
}

private void SpawnPortal(DungeonRoom room)
{
    portalSpawned = true;

    Vector2Int c = (room.InnerMin + room.InnerMax) / 2;
    Vector3 pos = new Vector3(c.x * tileSize, portalY, c.y * tileSize);

    GameObject prefab = dungeon.dungeonData.portal;
    
    GameObject go;
    if (prefab != null)
    {
        go = Instantiate(prefab, pos, Quaternion.identity, dungeonRenderer.Root);
    }
    else
    {
        Debug.LogWarning("DungeonData.portal chưa gán => dùng portal tạm");
        go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.transform.SetParent(dungeonRenderer.Root, false);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(2f, 1f, 2f);
        go.GetComponent<Collider>().isTrigger = true;
        go.AddComponent<Portal>();
    }
    go.name = "Portal_NextFloor";
}
    
    
    
    
    
    
    
    
    
    
    
    private DungeonRenderConfig BuildRenderConfig()
    {
        return new DungeonRenderConfig
        {
            wallHeight = wallHeight,
            tileSize = tileSize,
            lightHeight = lightHeight,
            lightRange = lightRange,
            lightIntensity = lightIntensity,
            lightColor = lightColor
        };
    }

    public void ClearTile()
    {
        if (dungeonRenderer == null) return;

        dungeonRenderer.Clear();
        dungeonRenderer = null;
    }
    #endregion

    #region Get,Set_dungeon 
    public DungeonRoom GetRoomById(int id)
    {
        if (!dungeon.TryGetRoom(id, out DungeonRoom room)) return null;
        return room;
    }

    public Vector3? GetPosOfRoomId(int roomId, int localX, int localY)
    {
        if (GetRoomById(roomId) == null)
        {
            Debug.LogWarning($"Không có room id {roomId}");
            return null;
        }
        Vector2Int pos = GetRoomById(roomId).LocalToTile(localX, localY);
        if (!dungeon.TryGetTile(pos, out DungeonTile tile) ||
            tile.Type != DungeonTileType.Floor)
        {
            Debug.LogWarning($"Tile {pos} không phải Floor / không có tile nhập vào");
            return null;
        }
        Vector3 world = new Vector3(pos.x * tileSize, 0f, pos.y * tileSize);
        return world;
    }
    #endregion
    [Header("Monster")]
    [SerializeField] int minMonster = 7;
    [SerializeField] int maxMonster = 10;   // bao gồm cả 10
    [SerializeField] int monsterEdgeMargin = 1; // chừa N ô sát tường, tránh quái dính tường/gate
    public void SpawnMonstersInRoom(DungeonRoom room)
    {
        // DungeonRoom room = GetRoomById(roomId);
        List<MonsterPrefab> list = dungeon.dungeonData.monster;
        if (room == null || list == null || list.Count == 0) return;

        // Các ô sàn trong room, xáo trộn => lấy từ đầu là không trùng vị trí
        List<Vector2Int> tiles = new List<Vector2Int>();
        foreach (DungeonTile t in room.roomTiles.Values)
            if (t.Type == DungeonTileType.Floor) tiles.Add(t.Position);

        for (int i = tiles.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (tiles[i], tiles[j]) = (tiles[j], tiles[i]);
        }
        if (room.roomType == RoomType.Boss)
        {
            GameObject boss = dungeon.dungeonData.Boss;p
            if (boss == null) return;
            Vector2 c = (Vector2)(room.InnerMin + room.InnerMax) * 0.5f;
            var b = Instantiate(boss, new Vector3(c.x * tileSize, 0f, c.y * tileSize),
                Quaternion.identity, dungeonRenderer.Root);
            room.Monster.Add(b);
            return;
        }
        int count = Mathf.Min(Random.Range(minMonster, maxMonster + 1), tiles.Count);
        // int count = Mathf.Min(Random.Range(1, 2), tiles.Count);   // 7-10 con

        // Tổng rate
        int total = 0;
        foreach (MonsterPrefab m in list)
            if (m.prefab != null && m.Rate > 0) total += m.Rate;
        if (total <= 0) return;

        for (int i = 0; i < count; i++)
        {
            // Chọn prefab theo rate
            int roll = Random.Range(0, total);
            GameObject prefab = null;
            foreach (MonsterPrefab m in list)
            {
                if (m.prefab == null || m.Rate <= 0) continue;
                if (roll < m.Rate) { prefab = m.prefab; break; }
                roll -= m.Rate;
            }

            Vector3 pos = new Vector3(tiles[i].x * tileSize, 0f, tiles[i].y * tileSize);
            var enemyObj = Instantiate(prefab, pos, Quaternion.identity, dungeonRenderer.Root);
            room.Monster.Add(enemyObj);
        }
    }

    public void SpawnObjectInRoomById(int roomId, GameObject gameObject, int localX, int localY)
    {
        DungeonRoom room = GetRoomById(roomId);
        for (int i = 0; i < room.Width; i++)
        {
            for (int j = 0; j < room.Height; j++)
            {
                if (i == room.Width / 2 && j == room.Height / 2)
                {
                    Vector3? pos = GetPosOfRoomId(roomId, i, j);
                    if(!pos.HasValue) return;
                    var obj = Instantiate(gameObject,pos.Value + new Vector3(0,0.5f,0),Quaternion.identity);
                }
            }
        }
    }
    public void SortDungeonRooms()
    {
        if (dungeonRooms == null || dungeonRooms.Count == 0)
            return;

        dungeonRooms.Sort((a, b) => a.idRoom.CompareTo(b.idRoom));
    }
}