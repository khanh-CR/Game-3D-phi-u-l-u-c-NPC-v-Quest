using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using Object = UnityEngine.Object;
public struct DungeonRenderConfig
{
    public int wallHeight;
    public float tileSize;

    public float lightHeight;
    public float lightRange;
    public float lightIntensity;
    public Color lightColor;
}
public class RenderDungeon
{
    private const int GateHeight = 2;

    private readonly Dungeon dungeon;
    private readonly DungeonRenderConfig cfg;
    private readonly System.Random renderRandom;

    private Transform dungeonRoot;
    public Transform Root => dungeonRoot;

    // ---- Hierarchy ----
    private Transform globalWallRoot;                                   // wall không thuộc room / corridor nào
    private readonly Dictionary<Vector2Int, DungeonRoom> tileToRoom = new Dictionary<Vector2Int, DungeonRoom>();
    private readonly Dictionary<DungeonRoom, Transform> roomRoots = new Dictionary<DungeonRoom, Transform>();
    private readonly Dictionary<DungeonRoom, Transform> roomWallRoots = new Dictionary<DungeonRoom, Transform>();
    private readonly Dictionary<CorridorLink, Transform> corridorRoots = new Dictionary<CorridorLink, Transform>();
    private readonly Dictionary<CorridorLink, Transform> corridorWallRoots = new Dictionary<CorridorLink, Transform>();

    // Đổi format tên GO ở đây
    private static string RoomName(DungeonRoom r) => $"room id={r.idRoom}";
    private static string CorridorName(CorridorLink c) => $"Room{c.A.idRoom}-Room{c.B.idRoom}";

    private int wallHeight => cfg.wallHeight;
    private float tileSize => cfg.tileSize;

    public RenderDungeon(Dungeon dungeon, DungeonRenderConfig config)
    {
        this.dungeon = dungeon;
        cfg = config;
        renderRandom = new System.Random(dungeon.Seed);
    }
    
    public void Clear()
    {
        if (dungeonRoot == null) return;
        // clear navmesh
        if (DungeonNavMesh != null)
            DungeonNavMesh.RemoveData();
        dungeonRoot.gameObject.SetActive(false);
        //destroy dungeon
        if (Application.isPlaying) Object.Destroy(dungeonRoot.gameObject);
        else Object.DestroyImmediate(dungeonRoot.gameObject);
        //clear tile
        DungeonNavMesh = null;
        dungeonRoot = null;
        globalWallRoot = null;
        tileToRoom.Clear();
        roomRoots.Clear();
        roomWallRoots.Clear();
        corridorRoots.Clear();      
        corridorWallRoots.Clear();
    }

    #region Hierarchy
    private static Transform CreateChild(Transform parent, string name)
    {
        Transform t = new GameObject(name).transform;
        t.SetParent(parent, false);
        return t;
    }

    // Tạo sẵn GO cho từng room (+ GO "Wall" con) và GO "Wall" chung của dungeon
    private void BuildHierarchy()
    {
        tileToRoom.Clear();
        roomRoots.Clear();
        roomWallRoots.Clear();
        corridorRoots.Clear();
        corridorWallRoots.Clear();

        foreach (DungeonRoom room in dungeon.rooms)
        {
            foreach (Vector2Int p in room.roomTiles.Keys)
                tileToRoom[p] = room;

            Transform roomRoot = CreateChild(dungeonRoot, RoomName(room));
            roomRoots[room] = roomRoot;
            roomWallRoots[room] = CreateChild(roomRoot, "Wall");
        }

        globalWallRoot = CreateChild(dungeonRoot, "Wall");
    }

    // Tên GO của floor = toạ độ local trong room
    private string FloorTileName(Vector2Int pos)
    {
        if (tileToRoom.TryGetValue(pos, out DungeonRoom room))
        {
            Vector2Int local = pos - room.Min;
            return $"({local.x},{local.y})";
        }
        return $"({pos.x},{pos.y})";
    }

    private Transform GetFloorParent(Vector2Int pos)
    {
        if (tileToRoom.TryGetValue(pos, out DungeonRoom room))
            return roomRoots[room];
        return dungeonRoot;
    }

    // GO corridor "RoomA-RoomB" tạo khi cần, kèm GO "Wall" con
    private Transform GetCorridorRoot(CorridorLink link)
    {
        if (!corridorRoots.TryGetValue(link, out Transform root))
        {
            root = CreateChild(dungeonRoot, CorridorName(link));
            corridorRoots[link] = root;
        }
        return root;
    }

    private Transform GetCorridorParent(Vector2Int pos)
    {
        if (dungeon.corridorOwner.TryGetValue(pos, out CorridorLink link))
            return GetCorridorRoot(link);
        return dungeonRoot;
    }

    private Transform GetWallParent(Vector2Int pos)
    {
        // 1. Wall / gate-wall của room
        if (tileToRoom.TryGetValue(pos, out DungeonRoom room))
            return roomWallRoots[room];

        // 2. Wall bao quanh corridor
        if (dungeon.corridorOwner.TryGetValue(pos, out CorridorLink link))
        {
            if (!corridorWallRoots.TryGetValue(link, out Transform wallRoot))
            {
                wallRoot = CreateChild(GetCorridorRoot(link), "Wall");
                corridorWallRoots[link] = wallRoot;
            }
            return wallRoot;
        }

        // 3. Còn lại (viền dungeon...)
        return globalWallRoot;
    }
    #endregion

    #region Method Build
    public void Render()
    {
        if (dungeon == null)
        {
            Debug.LogError("Dungeon null file:RenderDungeon");
            return;
        }

        DungeonData data = dungeon.dungeonData;
        if (data == null)
        {
            Debug.LogError("DungeonData null file:RenderDungeon");
            return;
        }

        // Dungeon luôn spawn tại (0, 0, 0)
        dungeonRoot = new GameObject("Dungeon").transform;
        dungeonRoot.position = Vector3.zero;

        BuildHierarchy();

        foreach (DungeonTile tile in dungeon.dungeonTiles.Values)
        {
            Vector2Int pos = tile.Position;

            // tile.x -> world x, tile.y -> world z, chiều cao là y
            Vector3 basePos = new Vector3(pos.x * tileSize, 0f, pos.y * tileSize);

            switch (tile.Type)
            {
                case DungeonTileType.Floor:
                    SpawnFloor(GetPrefab(data.Floors, w => w.prefab, w => w.Rate),
                               basePos, GetFloorParent(pos), FloorTileName(pos));
                    break;

                case DungeonTileType.Corridor:
                    SpawnFloor(GetPrefab(data.Corridor, w => w.prefab, w => w.Rate),
                               basePos, GetCorridorParent(pos), $"({pos.x},{pos.y})");
                    break;

                case DungeonTileType.Wall:
                    SpawnWallColumn(data.wall, basePos, 0, GetWallParent(pos));
                    break;

                case DungeonTileType.Gate:
                    // Gate nằm trên tường room => thuộc room đó
                    SpawnFloor(GetPrefab(data.Floors, w => w.prefab, w => w.Rate),
                               basePos, GetFloorParent(pos), FloorTileName(pos));           // nền dưới gate
                    SpawnGate(data.Gate, pos, basePos, GetFloorParent(pos));
                    SpawnWallColumn(data.wall, basePos, GateHeight, GetWallParent(pos));    // tường phía trên gate
                    break;

                // Empty: không render
            }
        }

        BuildDungeonNavMesh();
        // BuildRoomNavMeshes();
        // SpawnRoof(data);
        SpawnRoomLights();
    }
        private void SpawnFloor(GameObject prefab, Vector3 basePos, Transform parent, string tileName)
    {
        if (prefab == null) return;

        GameObject go = Object.Instantiate(prefab,
                                           basePos + Vector3.down * (tileSize * 0.5f),
                                           Quaternion.identity,
                                           parent);
        go.name = tileName;
    }
    private void SpawnWallColumn(GameObject prefab, Vector3 basePos, int startLevel, Transform parent)
    {
        if (prefab == null) return;

        for (int i = startLevel; i < wallHeight; i++)
        {
            Vector3 pos = basePos + Vector3.up * ((i + 0.5f) * tileSize);
            Object.Instantiate(prefab, pos, Quaternion.identity, parent);
        }
    }
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
                Object.Instantiate(data.roof, pos, Quaternion.identity, roofRoot);
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
                cfg.lightHeight * tileSize,
                center.y * tileSize);

            Light l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = cfg.lightRange;
            l.intensity = cfg.lightIntensity;
            l.color = cfg.lightColor;
        }
    }
    private void SpawnGate(GameObject prefab, Vector2Int tilePos, Vector3 basePos, Transform parent)
    {
        if (prefab == null) return;

        
        bool wallAlongX = IsWalkable(tilePos + Vector2Int.up) ||
                          IsWalkable(tilePos + Vector2Int.down);

        Quaternion rot = wallAlongX ? Quaternion.identity : Quaternion.Euler(0f, 90f, 0f);

        var gateObj = Object.Instantiate(prefab, basePos, rot, parent);
        if (tileToRoom.TryGetValue(tilePos, out DungeonRoom room))
        {
            Gate gate = gateObj.GetComponentInChildren<Gate>();
            if (gate != null) gate.dungeonRoom = room;
        }
        
    }


    #endregion
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
                room.InnerSize.x * tileSize - 0.2f,   
                2f,                                    
                room.InnerSize.y * tileSize - 0.2f);

            surface.BuildNavMesh();
        }
    }
    public NavMeshSurface DungeonNavMesh { get; private set; }
    private void BuildDungeonNavMesh()
    {
        RectInt b = dungeon.Bounds;

        // Phần bên trong = bỏ 1 ô tường viền mỗi cạnh
        int innerW = b.width - 2;
        int innerH = b.height - 2;
        if (innerW <= 0 || innerH <= 0) return;

        // Tile (x) có tâm tại x * tileSize, trải từ x-0.5 đến x+0.5 (theo tileSize).
        // Vùng trong: từ tile xMin+1 đến xMax-2  =>  tâm = (xMin + xMax - 1) / 2
        float centerX = (b.xMin + b.xMax - 1) * 0.5f * tileSize;
        float centerZ = (b.yMin + b.yMax - 1) * 0.5f * tileSize;

        // Sàn có mặt trên ở y = 0, tường cao wallHeight => volume phủ từ dưới sàn đến trên đỉnh tường
        float volumeHeight = wallHeight * tileSize + 1f;
        float centerY = wallHeight * tileSize * 0.5f;

        GameObject go = new GameObject("NavMesh_Dungeon");
        go.transform.SetParent(dungeonRoot, false);
        go.transform.position = new Vector3(centerX, centerY, centerZ);

        NavMeshSurface surface = go.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.Volume;
        surface.center = Vector3.zero;
        surface.size = new Vector3(
            innerW * tileSize,
            volumeHeight,
            innerH * tileSize);

        surface.BuildNavMesh();
        DungeonNavMesh = surface;

        // Giữ tương thích với code cũ đang dùng room.NavMeshRoom
        foreach (DungeonRoom room in dungeon.rooms)
            room.NavMeshRoom = go;
    }

    #region  Method Support

    private bool IsNextToGate(Vector2Int p)
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int d in dirs)
        {
            if (dungeon.dungeonTiles.TryGetValue(p + d, out DungeonTile t) &&
                t.Type == DungeonTileType.Gate)
                return true;
        }
        return false;
    }
    private bool IsWalkable(Vector2Int p)
    {
        return dungeon.dungeonTiles.TryGetValue(p, out DungeonTile t) && t.IsWalkable;
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

}