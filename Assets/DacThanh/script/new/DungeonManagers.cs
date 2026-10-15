using System.Collections.Generic;
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
    [Header("Light")]
    [SerializeField] float lightHeight = 3f;      // độ cao đèn (tính theo ô, từ mặt sàn)
    [SerializeField] float lightRange = 15f;
    [SerializeField] float lightIntensity = 2f;
    [SerializeField] Color lightColor = Color.white;

    private Dungeon dungeon;
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

    #region BuildDungeon
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
}