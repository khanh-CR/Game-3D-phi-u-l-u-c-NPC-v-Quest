using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DungeonChunkRenderer : MonoBehaviour
{
    private const int TypeCount = 5;

    [Header("Chunk")]
    [SerializeField] private int chunkSize = 16;
    [SerializeField] private int viewDistance = 4;
    [SerializeField] private float blockSize = 1f;
    [SerializeField] private bool generateCollider = true;

    [Header("View")]
    [SerializeField] private Transform viewTarget;

    private Dungeon dungeon;
    private DungeonData data;

    private readonly Material[] materials = new Material[TypeCount];
    private readonly int[] heights = new int[TypeCount];

    private readonly Dictionary<Vector2Int, GameObject> chunks =
        new Dictionary<Vector2Int, GameObject>();

    private Vector2Int chunkCount;
    private Vector2Int lastChunk;
    private bool hasLastChunk;

    private static readonly Vector2Int[] SideDirs =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    public void Build(Dungeon builtDungeon, Transform target)
    {
        Clear();

        dungeon = builtDungeon;
        data = dungeon.dungeonData;
        viewTarget = target;

        for (int i = 0; i < TypeCount; i++)
        {
            DungeonTileType type = (DungeonTileType)i;
            materials[i] = data.GetMaterial(type);
            heights[i] = data.GetHeight(type);
        }

        chunkCount = new Vector2Int(
            Mathf.CeilToInt(dungeon.dungeonSize.x / (float)chunkSize),
            Mathf.CeilToInt(dungeon.dungeonSize.y / (float)chunkSize)
        );

        hasLastChunk = false;

        Refresh();
    }

    public void Clear()
    {
        foreach (GameObject chunk in chunks.Values)
        {
            if (chunk == null) continue;

            MeshFilter filter = chunk.GetComponent<MeshFilter>();
            if (filter != null && filter.sharedMesh != null)
                Destroy(filter.sharedMesh);

            Destroy(chunk);
        }

        chunks.Clear();
        dungeon = null;
        hasLastChunk = false;
    }

    public Vector3 GridToWorld(Vector2Int tile, float yBlock)
    {
        return transform.TransformPoint(new Vector3(
            (tile.x + 0.5f) * blockSize,
            yBlock * blockSize,
            (tile.y + 0.5f) * blockSize
        ));
    }

    private void Update()
    {
        if (dungeon == null || viewTarget == null) return;

        Vector2Int current = WorldToChunk(viewTarget.position);

        if (hasLastChunk && current == lastChunk) return;

        Refresh();
    }

    private Vector2Int WorldToChunk(Vector3 worldPosition)
    {
        Vector3 local = transform.InverseTransformPoint(worldPosition);
        float size = blockSize * chunkSize;

        return new Vector2Int(
            Mathf.FloorToInt(local.x / size),
            Mathf.FloorToInt(local.z / size)
        );
    }

    private void Refresh()
    {
        bool showAll = viewTarget == null;
        Vector2Int center = showAll ? Vector2Int.zero : WorldToChunk(viewTarget.position);

        lastChunk = center;
        hasLastChunk = true;

        for (int cx = 0; cx < chunkCount.x; cx++)
        {
            for (int cz = 0; cz < chunkCount.y; cz++)
            {
                bool visible = showAll ||
                    Mathf.Max(Mathf.Abs(cx - center.x), Mathf.Abs(cz - center.y)) <= viewDistance;

                Vector2Int key = new Vector2Int(cx, cz);

                if (!chunks.TryGetValue(key, out GameObject chunk))
                {
                    if (!visible) continue;

                    chunk = CreateChunk(key);
                    chunks.Add(key, chunk);
                }

                if (chunk != null) chunk.SetActive(visible);
            }
        }
    }

    private GameObject CreateChunk(Vector2Int chunkCoord)
    {
        MeshBuilder builder = new MeshBuilder(blockSize);

        int minX = chunkCoord.x * chunkSize;
        int minZ = chunkCoord.y * chunkSize;

        for (int x = minX; x < minX + chunkSize; x++)
        {
            for (int z = minZ; z < minZ + chunkSize; z++)
            {
                if (!dungeon.dungeonTiles.TryGetValue(new Vector2Int(x, z), out DungeonTile tile))
                    continue;

                int type = (int)tile.Type;

                if (materials[type] == null) continue;

                int height = heights[type];

                AddTop(builder, x, height - 1, z, type);

                foreach (Vector2Int dir in SideDirs)
                {
                    int neighborHeight = HeightAt(x + dir.x, z + dir.y);

                    for (int y = neighborHeight; y < height; y++)
                        AddSide(builder, x, y, z, dir, type);
                }
            }
        }

        if (builder.Vertices.Count == 0) return null;

        List<int> usedTypes = new List<int>();
        for (int i = 0; i < TypeCount; i++)
            if (builder.Triangles[i].Count > 0) usedTypes.Add(i);

        Mesh mesh = new Mesh();
        mesh.name = $"DungeonChunk_{chunkCoord.x}_{chunkCoord.y}";
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.SetVertices(builder.Vertices);
        mesh.SetNormals(builder.Normals);
        mesh.SetUVs(0, builder.Uvs);
        mesh.subMeshCount = usedTypes.Count;

        Material[] chunkMaterials = new Material[usedTypes.Count];
        for (int i = 0; i < usedTypes.Count; i++)
        {
            mesh.SetTriangles(builder.Triangles[usedTypes[i]], i);
            chunkMaterials[i] = materials[usedTypes[i]];
        }

        mesh.RecalculateBounds();

        GameObject go = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
        go.transform.SetParent(transform, false);

        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterials = chunkMaterials;

        if (generateCollider)
        {
            MeshCollider meshCollider = go.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
        }

        return go;
    }

    private int HeightAt(int x, int z)
    {
        if (!dungeon.dungeonTiles.TryGetValue(new Vector2Int(x, z), out DungeonTile tile))
            return int.MaxValue;

        return heights[(int)tile.Type];
    }

    private static void AddTop(MeshBuilder b, int x, int y, int z, int sub)
    {
        b.AddQuad(
            new Vector3(x, y + 1, z),
            new Vector3(x, y + 1, z + 1),
            new Vector3(x + 1, y + 1, z + 1),
            new Vector3(x + 1, y + 1, z),
            Vector3.up,
            sub
        );
    }

    private static void AddSide(MeshBuilder b, int x, int y, int z, Vector2Int dir, int sub)
    {
        if (dir.x == 1)
        {
            b.AddQuad(
                new Vector3(x + 1, y, z),
                new Vector3(x + 1, y + 1, z),
                new Vector3(x + 1, y + 1, z + 1),
                new Vector3(x + 1, y, z + 1),
                Vector3.right,
                sub
            );
        }
        else if (dir.x == -1)
        {
            b.AddQuad(
                new Vector3(x, y, z + 1),
                new Vector3(x, y + 1, z + 1),
                new Vector3(x, y + 1, z),
                new Vector3(x, y, z),
                Vector3.left,
                sub
            );
        }
        else if (dir.y == 1)
        {
            b.AddQuad(
                new Vector3(x + 1, y, z + 1),
                new Vector3(x + 1, y + 1, z + 1),
                new Vector3(x, y + 1, z + 1),
                new Vector3(x, y, z + 1),
                Vector3.forward,
                sub
            );
        }
        else
        {
            b.AddQuad(
                new Vector3(x, y, z),
                new Vector3(x, y + 1, z),
                new Vector3(x + 1, y + 1, z),
                new Vector3(x + 1, y, z),
                Vector3.back,
                sub
            );
        }
    }

    private class MeshBuilder
    {
        public readonly List<Vector3> Vertices = new List<Vector3>();
        public readonly List<Vector3> Normals = new List<Vector3>();
        public readonly List<Vector2> Uvs = new List<Vector2>();
        public readonly List<int>[] Triangles = new List<int>[TypeCount];

        private readonly float scale;

        public MeshBuilder(float scale)
        {
            this.scale = scale;

            for (int i = 0; i < TypeCount; i++)
                Triangles[i] = new List<int>();
        }

        public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal, int sub)
        {
            int start = Vertices.Count;

            Vertices.Add(a * scale);
            Vertices.Add(b * scale);
            Vertices.Add(c * scale);
            Vertices.Add(d * scale);

            Normals.Add(normal);
            Normals.Add(normal);
            Normals.Add(normal);
            Normals.Add(normal);

            Uvs.Add(new Vector2(0f, 0f));
            Uvs.Add(new Vector2(0f, 1f));
            Uvs.Add(new Vector2(1f, 1f));
            Uvs.Add(new Vector2(1f, 0f));

            List<int> tris = Triangles[sub];
            tris.Add(start);
            tris.Add(start + 1);
            tris.Add(start + 2);
            tris.Add(start);
            tris.Add(start + 2);
            tris.Add(start + 3);
        }
    }
}
