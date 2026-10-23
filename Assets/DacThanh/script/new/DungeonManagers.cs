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

    private void Update()
    {
        
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

            
            wallHeight = Mathf.Max(1, wallHeight);

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

            wallHeight = Mathf.Max(1, wallHeight);
        }
        
        ClearTile();
        dungeon = new Dungeon(MasterSeed, Level, data);
        dungeonRooms = dungeon.rooms;

        // Dữ liệu (Dungeon) và phần dựng ra scene (RenderDungeon) tách riêng
        dungeonRenderer = new RenderDungeon(dungeon, BuildRenderConfig());
        dungeonRenderer.Render();
        
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

        int count = Mathf.Min(Random.Range(1, 2), tiles.Count);   // 7-10 con

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
}