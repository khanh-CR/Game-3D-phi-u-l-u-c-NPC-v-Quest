using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonManagers : MonoBehaviour
{
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

    private Dungeon dungeon;
    public List<DungeonRoom> dungeonRooms;
    private Transform dungeonRoot;
    private System.Random renderRandom;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (generateOnStart) DungeonGenerator();
    }

    // Không truyền seed: random seed mới, hoặc dùng MasterSeed nếu randomSeed = false
    [ContextMenu("Generate Dungeon")]
    public void DungeonGenerator()
    {
        if (randomSeed)
            MasterSeed = Random.Range(int.MinValue, int.MaxValue);

        CreateDungeon(MasterSeed);
    }

    // Truyền seed => luôn ra đúng dungeon đó
    public void CreateDungeon(int seed)
    {
        CreateDungeon(seed, wallHeight);
    }

    // Truyền seed + chiều cao tường
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
                    SpawnFloor(data.Corridor, basePos);
                    break;

                case DungeonTileType.Wall:
                    SpawnWallColumn(data.wall, basePos, 0);
                    break;

                case DungeonTileType.Gate:
                    SpawnFloor(data.Corridor, basePos);                 // nền dưới gate
                    SpawnGate(data.Gate, tile.Position, basePos+ new Vector3(0,0.5f,0));
                    SpawnWallColumn(data.wall, basePos, GateHeight);    // tường phía trên gate
                    break;

                // Empty: không render
            }
        }
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

    private void SpawnGate(GameObject prefab, Vector2Int tilePos, Vector3 basePos)
    {
        if (prefab == null) return;

        // Gate nằm trên tường chạy theo trục X (trái + phải đều là Wall)
        // thì mặt gate hướng theo Z, ngược lại xoay 90 độ
        bool wallAlongX = IsWall(tilePos + Vector2Int.left) && IsWall(tilePos + Vector2Int.right);
        Quaternion rot = wallAlongX ? Quaternion.identity : Quaternion.Euler(0f, 90f, 0f);

        Instantiate(prefab, basePos, rot, dungeonRoot);
    }

    private bool IsWall(Vector2Int p)
    {
        return dungeon.dungeonTiles.TryGetValue(p, out DungeonTile t) &&
               t.Type == DungeonTileType.Wall;
    }

    public void ClearTile()
    {
        if (dungeonRoot == null) return;

        if (Application.isPlaying) Destroy(dungeonRoot.gameObject);
        else DestroyImmediate(dungeonRoot.gameObject);

        dungeonRoot = null;
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
}
