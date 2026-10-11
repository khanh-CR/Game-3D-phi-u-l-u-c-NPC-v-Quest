using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonManagers : MonoBehaviour
{
    #region Attibute
    public static DungeonManagers instance;
    [Header("Seed")]
    public int MasterSeed;
    public bool randomSeed = true;

    [Header("Dungeon")]
    [SerializeField] List<DungeonData> listDungeonsData;
    [SerializeField] int Level = 1;
    [SerializeField] int wallHeight = 5;     // số block tường theo trục Y (mặc định 5)
    [SerializeField] float tileSize = 1f;
    [SerializeField] bool generateOnStart = false;
    private const int GateHeight = 2;        // gate cao 2 ô
    [Header("Light")]
    [SerializeField] float lightHeight = 3f;      // độ cao đèn (tính theo ô, từ mặt sàn)
    [SerializeField] float lightRange = 15f;
    [SerializeField] float lightIntensity = 2f;
    [SerializeField] Color lightColor = Color.white;
    
    

    private Dungeon dungeon;
    public List<DungeonRoom> dungeonRooms;
    private Transform dungeonRoot;
    private System.Random renderRandom;
    #endregion
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (generateOnStart) DungeonGenerator();
    }
    [ContextMenu("Generate Dungeon")]
    public void DungeonGenerator()
    {
        if (randomSeed)
            MasterSeed = Random.Range(int.MinValue, int.MaxValue);

        CreateDungeon(MasterSeed);
    }
    public void CreateDungeon(int seed)
    {
        CreateDungeon(seed, wallHeight);
    }
    public void CreateDungeon(int seed, int height)
    {
        if (listDungeonsData == null || listDungeonsData.Count == 0)
        {
            Debug.LogError("DungeonData null file:DungeonManagers");
            return;
        }

        MasterSeed = seed;
        wallHeight = Mathf.Max(1, height);

        // Chọn data theo seed để cùng seed luôn cùng giao diện
        int index = (int)(((long)seed % listDungeonsData.Count + listDungeonsData.Count) % listDungeonsData.Count);
        DungeonData data = listDungeonsData[index];

        ClearTile();
        dungeon = new Dungeon(seed, Level, data);
        dungeonRooms = dungeon.rooms;
        renderRandom = new System.Random(dungeon.Seed);
        RenderDungeon();
    }
    #region Build_Dungeon
    private void RenderDungeon()
    {
        if (dungeon == null)
        {
            Debug.LogError("Dungeon null file:DungeonManagers");
            return;
        }

        DungeonData data = dungeon.dungeonData;
        if (data == null)
        {
            Debug.LogError("DungeonData null file:DungeonManagers");
            return;
        }

        // Dungeon luôn spawn tại (0, 0, 0)
        dungeonRoot = new GameObject("Dungeon").transform;
        dungeonRoot.position = Vector3.zero;

        foreach (DungeonTile tile in dungeon.dungeonTiles.Values)
        {
            // tile.x -> world x, tile.y -> world z, chiều cao là y
            Vector3 basePos = new Vector3(
                tile.Position.x * tileSize,
                0f,
                tile.Position.y * tileSize);

            switch (tile.Type)
            {
                case DungeonTileType.Floor:
                    SpawnFloor(GetPrefab(data.Floors,w => w.prefab, w => w.Rate), basePos);
                    break;

                case DungeonTileType.Corridor:
                    SpawnFloor(GetPrefab(data.Corridor,w => w.prefab, w => w.Rate), basePos);
                    break;

                case DungeonTileType.Wall:
                    SpawnWallColumn(data.wall, basePos, 0);
                    break;

                case DungeonTileType.Gate:
                    SpawnFloor(GetPrefab(data.Floors,w => w.prefab, w => w.Rate), basePos);// nền dưới gate
                    SpawnGate(data.Gate, tile.Position, basePos+ new Vector3(0,0.5f,0));
                    SpawnWallColumn(data.wall, basePos, GateHeight);    // tường phía trên gate
                    break;

                // Empty: không render
            }
        }
        BuildRoomNavMeshes();
        // SpawnRoof(data);
        SpawnRoomLights();
    }

    // Mặt trên của sàn nằm ở y = 0
    private void SpawnFloor(GameObject prefab, Vector3 basePos)
    {
        if (prefab == null) return;

        Instantiate(prefab,
                    basePos + Vector3.down * (tileSize * 0.5f),
                    Quaternion.identity,
                    dungeonRoot);
    }

    // Xếp block từ tầng startLevel đến wallHeight - 1
    private void SpawnWallColumn(GameObject prefab, Vector3 basePos, int startLevel)
    {
        if (prefab == null) return;

        for (int i = startLevel; i < wallHeight; i++)
        {
            Vector3 pos = basePos + Vector3.up * ((i + 0.5f) * tileSize);
            Instantiate(prefab, pos, Quaternion.identity, dungeonRoot);
        }
    }
// Mái phủ toàn bộ dungeon, nằm ngay trên đỉnh tường (tầng chỉ số = wallHeight)
    private void SpawnRoof(DungeonData data)
    {
        if (data.roof == null) return;

        // Gom mái dưới 1 object riêng cho gọn Hierarchy
        Transform roofRoot = new GameObject("Roof").transform;
        roofRoot.SetParent(dungeonRoot, false);

        RectInt b = dungeon.Bounds;
        float y = (wallHeight + 0.5f) * tileSize;

        for (int x = b.xMin; x < b.xMax; x++)
        {
            for (int z = b.yMin; z < b.yMax; z++)
            {
                Vector3 pos = new Vector3(x * tileSize, y, z * tileSize);
                Instantiate(data.roof, pos, Quaternion.identity, roofRoot);
            }
        }
    }
    // Mỗi room 1 đèn ở giữa phòng
    private void SpawnRoomLights()
    {
        foreach (DungeonRoom room in dungeon.rooms)
        {
            // Tâm vùng sàn (tính theo tile) -> world
            Vector2 center = (Vector2)(room.InnerMin + room.InnerMax) * 0.5f;

            GameObject go = new GameObject($"Light_Room_{room.idRoom}");
            go.transform.SetParent(dungeonRoot, false);
            go.transform.position = new Vector3(
                center.x * tileSize,
                lightHeight * tileSize,
                center.y * tileSize);

            Light l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = lightRange;
            l.intensity = lightIntensity;
            l.color = lightColor;
        }
    }
    private void SpawnGate(GameObject prefab, Vector2Int tilePos, Vector3 basePos)
    {
        if (prefab == null) return;

        // Gate nằm trên tường chạy theo trục X (trái + phải đều là Wall)
        // thì mặt gate hướng theo Z, ngược lại xoay 90 độ
        // bool wallAlongX = IsWall(tilePos + Vector2Int.left) && IsWall(tilePos + Vector2Int.right);
        bool wallAlongX = IsWalkable(tilePos + Vector2Int.up) ||
                          IsWalkable(tilePos + Vector2Int.down);

        Quaternion rot = wallAlongX ? Quaternion.identity : Quaternion.Euler(0f, 90f, 0f);

        Instantiate(prefab, basePos, rot, dungeonRoot);
    }
    private bool IsWalkable(Vector2Int p)
    {
        return dungeon.dungeonTiles.TryGetValue(p, out DungeonTile t) && t.IsWalkable;
    }
    private void BuildRoomNavMeshes()
    {
        foreach (DungeonRoom room in dungeon.rooms)
        {
            // Chỉ Normal và Boss
            if (room.roomType != RoomType.Normal &&
                room.roomType != RoomType.Boss) continue;

            // Tâm vùng sàn (tính theo tile), đổi sang world
            Vector2 center = (Vector2)(room.InnerMin + room.InnerMax) * 0.5f;

            GameObject go = new GameObject($"NavMesh_Room_{room.idRoom}");
            go.transform.SetParent(dungeonRoot, false);
            go.transform.position = new Vector3(
                center.x * tileSize,
                0f,
                center.y * tileSize);
            room.NavMeshRoom = go;
            NavMeshSurface surface = go.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Volume;
            surface.center = Vector3.zero;
            surface.size = new Vector3(
                room.InnerSize.x * tileSize - 0.1f,   // trừ nhỏ để không dính mesh tường sát mép
                2f,                                    // cao 2: phủ sàn (y từ -1 đến 0) và một chút phía trên
                room.InnerSize.y * tileSize - 0.1f);

            surface.BuildNavMesh();
        }
    }

    private bool IsWall(Vector2Int p)
    {
        return dungeon.dungeonTiles.TryGetValue(p, out DungeonTile t) &&
               t.Type == DungeonTileType.Wall;
    }
    private GameObject GetPrefab<T>(List<T> list,
        System.Func<T, GameObject> getPrefab,
        System.Func<T, int> getRate)
    {
        if (list == null || list.Count == 0) return null;

        int total = 0;
        foreach (T item in list)
            if (getPrefab(item) != null && getRate(item) > 0)
                total += getRate(item);

        if (total <= 0) return null;

        int roll = renderRandom.Next(total);
        foreach (T item in list)
        {
            int rate = getRate(item);
            if (getPrefab(item) == null || rate <= 0) continue;
            if (roll < rate) return getPrefab(item);
            roll -= rate;
        }
        return null;
    }
    #endregion
    public void ClearTile()
    {
        if (dungeonRoot == null) return;

        if (Application.isPlaying) Destroy(dungeonRoot.gameObject);
        else DestroyImmediate(dungeonRoot.gameObject);

        dungeonRoot = null;
    }
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
}
