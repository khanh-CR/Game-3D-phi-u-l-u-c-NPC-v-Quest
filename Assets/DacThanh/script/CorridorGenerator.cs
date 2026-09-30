using System.Collections.Generic;
using UnityEngine;

public class CorridorGenerator
{
    private readonly Dungeon dungeon;
    private readonly Dictionary<Vector2Int, DungeonTile> tiles;

    private readonly float loopChance;
    private readonly int maxLoopDistance;
    private readonly int corridorHalfWidth;

    private readonly List<(DungeonRoom a, DungeonRoom b)> edges =
        new List<(DungeonRoom a, DungeonRoom b)>();

    private readonly List<List<Vector2Int>> paths = new List<List<Vector2Int>>();

    public DungeonRoom StartRoom { get; private set; }
    public DungeonRoom EndRoom { get; private set; }

    private static readonly Vector2Int[] Dirs =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    public CorridorGenerator(Dungeon dungeon, float loopChance = 0.15f, int maxLoopDistance = 40, int corridorHalfWidth = 1)
    {
        this.dungeon = dungeon;
        this.tiles = dungeon.dungeonTiles;
        this.loopChance = loopChance;
        this.maxLoopDistance = maxLoopDistance;
        this.corridorHalfWidth = corridorHalfWidth;
    }

    public void Generate(BSPNode root)
    {
        List<DungeonRoom> rooms = dungeon.rooms;
        if (rooms.Count < 2) return;

        edges.Clear();
        paths.Clear();

        foreach (DungeonRoom r in rooms)
        {
            r.connections.Clear();
            r.gates.Clear();
            r.roomType = RoomType.Normal;
        }

        BuildTreeEdges(root);

        StartRoom = Farthest(rooms[0]);
        EndRoom = Farthest(StartRoom);
        StartRoom.roomType = RoomType.Start;
        EndRoom.roomType = RoomType.Boss;

        AddLoops();

        foreach (var e in edges)
            Connect(e.a, e.b);

        WidenCorridors();
        WrapCorridorsWithWall();
    }

    private void WidenCorridors()
    {
        if (corridorHalfWidth <= 0) return;

        int h = corridorHalfWidth;

        foreach (List<Vector2Int> path in paths)
        {
            foreach (Vector2Int p in path)
            {
                for (int dx = -h; dx <= h; dx++)
                {
                    for (int dz = -h; dz <= h; dz++)
                    {
                        Vector2Int q = new Vector2Int(p.x + dx, p.y + dz);

                        if (tiles.TryGetValue(q, out DungeonTile t) &&
                            t.Type == DungeonTileType.Empty)
                        {
                            SetType(q, DungeonTileType.Corridor);
                        }
                    }
                }
            }
        }
    }

    private void WrapCorridorsWithWall()
    {
        List<Vector2Int> corridorTiles = new List<Vector2Int>();

        foreach (KeyValuePair<Vector2Int, DungeonTile> pair in tiles)
        {
            if (pair.Value.Type == DungeonTileType.Corridor)
                corridorTiles.Add(pair.Key);
        }

        foreach (Vector2Int p in corridorTiles)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dz == 0) continue;

                    Vector2Int q = new Vector2Int(p.x + dx, p.y + dz);

                    if (tiles.TryGetValue(q, out DungeonTile t) &&
                        t.Type == DungeonTileType.Empty)
                    {
                        SetType(q, DungeonTileType.Wall);
                    }
                }
            }
        }
    }

    private List<DungeonRoom> BuildTreeEdges(BSPNode node)
    {
        List<DungeonRoom> result = new List<DungeonRoom>();
        if (node == null) return result;

        if (node.IsLeaf)
        {
            if (node.Room != null) result.Add(node.Room);
            return result;
        }

        List<DungeonRoom> left = BuildTreeEdges(node.Left);
        List<DungeonRoom> right = BuildTreeEdges(node.Right);

        if (left.Count > 0 && right.Count > 0)
        {
            DungeonRoom bestA = null, bestB = null;
            int best = int.MaxValue;

            foreach (DungeonRoom a in left)
            {
                foreach (DungeonRoom b in right)
                {
                    int d = (Center(a) - Center(b)).sqrMagnitude;
                    if (d < best) { best = d; bestA = a; bestB = b; }
                }
            }

            AddEdge(bestA, bestB);
        }

        result.AddRange(left);
        result.AddRange(right);
        return result;
    }

    private void AddEdge(DungeonRoom a, DungeonRoom b)
    {
        edges.Add((a, b));
        a.connections.Add(b);
        b.connections.Add(a);
    }

    private DungeonRoom Farthest(DungeonRoom from)
    {
        HashSet<DungeonRoom> visited = new HashSet<DungeonRoom> { from };
        Queue<DungeonRoom> queue = new Queue<DungeonRoom>();
        queue.Enqueue(from);
        DungeonRoom last = from;

        while (queue.Count > 0)
        {
            last = queue.Dequeue();
            foreach (DungeonRoom n in last.connections)
                if (visited.Add(n)) queue.Enqueue(n);
        }

        return last;
    }

    private void AddLoops()
    {
        if (loopChance <= 0f) return;

        foreach (DungeonRoom a in dungeon.rooms)
        {
            if (a == StartRoom || a == EndRoom) continue;
            if (Random.value > loopChance) continue;

            DungeonRoom best = null;
            int bestDist = maxLoopDistance * maxLoopDistance;

            foreach (DungeonRoom b in dungeon.rooms)
            {
                if (b == a || b == StartRoom || b == EndRoom) continue;
                if (a.connections.Contains(b)) continue;

                int d = (Center(a) - Center(b)).sqrMagnitude;
                if (d < bestDist) { bestDist = d; best = b; }
            }

            if (best != null) AddEdge(a, best);
        }
    }

    private void Connect(DungeonRoom a, DungeonRoom b)
    {
        for (int attempt = 0; attempt < 8; attempt++)
        {
            if (!TryPickGate(a, b, attempt, out Vector2Int gateA, out Vector2Int outA)) continue;
            if (!TryPickGate(b, a, attempt, out Vector2Int gateB, out Vector2Int outB)) continue;

            List<Vector2Int> path = FindPath(outA, outB);
            if (path == null) continue;

            SetType(gateA, DungeonTileType.Gate);
            SetType(gateB, DungeonTileType.Gate);
            a.gates.Add(gateA);
            b.gates.Add(gateB);

            foreach (Vector2Int p in path)
                if (tiles[p].Type == DungeonTileType.Empty)
                    SetType(p, DungeonTileType.Corridor);

            paths.Add(path);
            return;
        }

        Debug.LogWarning($"Không nối được room {a.idRoom} với room {b.idRoom}");
    }

    private bool TryPickGate(DungeonRoom room, DungeonRoom target, int attempt,
                             out Vector2Int gate, out Vector2Int outside)
    {
        gate = outside = Vector2Int.zero;

        Vector2Int delta = Center(target) - Center(room);
        Vector2Int horizontal = new Vector2Int(delta.x >= 0 ? 1 : -1, 0);
        Vector2Int vertical = new Vector2Int(0, delta.y >= 0 ? 1 : -1);

        bool horizontalFirst = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y);
        if (attempt % 2 == 1) horizontalFirst = !horizontalFirst;

        Vector2Int dir = horizontalFirst ? horizontal : vertical;
        RectInt r = room.Bounds;

        if (dir.x == 1)       gate = new Vector2Int(r.xMax - 1, Random.Range(r.yMin + 1, r.yMax - 1));
        else if (dir.x == -1) gate = new Vector2Int(r.xMin,     Random.Range(r.yMin + 1, r.yMax - 1));
        else if (dir.y == 1)  gate = new Vector2Int(Random.Range(r.xMin + 1, r.xMax - 1), r.yMax - 1);
        else                  gate = new Vector2Int(Random.Range(r.xMin + 1, r.xMax - 1), r.yMin);

        outside = gate + dir;
        return IsWalkable(outside);
    }

    private List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        if (!IsWalkable(start) || !IsWalkable(goal)) return null;

        MinHeap open = new MinHeap();
        Dictionary<Vector2Int, float> g = new Dictionary<Vector2Int, float> { { start, 0f } };
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        HashSet<Vector2Int> closed = new HashSet<Vector2Int>();

        open.Push(start, Manhattan(start, goal));

        while (open.Count > 0)
        {
            Vector2Int cur = open.Pop();
            if (cur == goal) return Reconstruct(cameFrom, cur);
            if (!closed.Add(cur)) continue;

            Vector2Int prevDir = cameFrom.TryGetValue(cur, out Vector2Int parent)
                ? cur - parent
                : Vector2Int.zero;

            foreach (Vector2Int d in Dirs)
            {
                Vector2Int next = cur + d;
                if (closed.Contains(next) || !IsWalkable(next)) continue;

                float step = tiles[next].Type == DungeonTileType.Corridor ? 0.4f : 1f;
                if (prevDir != Vector2Int.zero && d != prevDir) step += 0.5f;

                float tentative = g[cur] + step;
                if (!g.TryGetValue(next, out float old) || tentative < old)
                {
                    g[next] = tentative;
                    cameFrom[next] = cur;
                    open.Push(next, tentative + Manhattan(next, goal));
                }
            }
        }

        return null;
    }

    private static List<Vector2Int> Reconstruct(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int cur)
    {
        List<Vector2Int> path = new List<Vector2Int> { cur };
        while (cameFrom.TryGetValue(cur, out Vector2Int prev))
        {
            cur = prev;
            path.Add(cur);
        }
        path.Reverse();
        return path;
    }

    private bool IsWalkable(Vector2Int p)
    {
        return tiles.TryGetValue(p, out DungeonTile t) &&
               (t.Type == DungeonTileType.Empty || t.Type == DungeonTileType.Corridor);
    }

    private void SetType(Vector2Int p, DungeonTileType type)
    {
        DungeonTile t = tiles[p];
        t.Type = type;
        tiles[p] = t;
    }

    private static Vector2Int Center(DungeonRoom room)
    {
        RectInt b = room.Bounds;
        return new Vector2Int(b.x + b.width / 2, b.y + b.height / 2);
    }

    private static float Manhattan(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private class MinHeap
    {
        private readonly List<KeyValuePair<float, Vector2Int>> items =
            new List<KeyValuePair<float, Vector2Int>>();

        public int Count => items.Count;

        public void Push(Vector2Int pos, float priority)
        {
            items.Add(new KeyValuePair<float, Vector2Int>(priority, pos));
            int i = items.Count - 1;
            while (i > 0)
            {
                int p = (i - 1) / 2;
                if (items[p].Key <= items[i].Key) break;
                (items[p], items[i]) = (items[i], items[p]);
                i = p;
            }
        }

        public Vector2Int Pop()
        {
            Vector2Int top = items[0].Value;
            int last = items.Count - 1;
            items[0] = items[last];
            items.RemoveAt(last);

            int i = 0;
            while (true)
            {
                int l = i * 2 + 1, r = l + 1, s = i;
                if (l < items.Count && items[l].Key < items[s].Key) s = l;
                if (r < items.Count && items[r].Key < items[s].Key) s = r;
                if (s == i) break;
                (items[s], items[i]) = (items[i], items[s]);
                i = s;
            }
            return top;
        }
    }
}
