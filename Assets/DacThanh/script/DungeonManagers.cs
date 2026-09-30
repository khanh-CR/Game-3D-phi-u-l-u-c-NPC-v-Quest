using System.Collections.Generic;
using UnityEngine;

public class DungeonManagers : MonoBehaviour
{
    [Header("seed and Floor")]
    [SerializeField] public int MasterSeed;
    [SerializeField] public int currentFloor;

    [SerializeField] List<DungeonData> listDungeonsData;

    [Header("Dungeon Formation")]
    [SerializeField] int Level;

    [Header("Render")]
    [SerializeField] DungeonChunkRenderer chunkRenderer;
    [SerializeField] Transform player;

    private Dungeon dungeon;
    public List<DungeonRoom> dungeonRooms;

    public void DungeonGenerator()
    {
        Level = Random.Range(0, 20);

        if (listDungeonsData == null || listDungeonsData.Count == 0)
        {
            Debug.LogError("DungeonData null file:DungeonManagers");
            return;
        }

        if (chunkRenderer == null)
        {
            Debug.LogError("DungeonChunkRenderer null file:DungeonManagers");
            return;
        }

        int index = Random.Range(0, listDungeonsData.Count);
        CreateDungeon(listDungeonsData[index]);
    }

    public void CreateDungeon(DungeonData dungeonData)
    {
        ClearTile();

        dungeon = new Dungeon(Level, dungeonData);
        dungeonRooms = dungeon.rooms;

        chunkRenderer.Build(dungeon, player);

        PlacePlayerAtStart(dungeonData);
    }

    public void ClearTile()
    {
        if (chunkRenderer == null)
        {
            Debug.LogError("DungeonChunkRenderer null! Không thể ClearTile. file:DungeonManagers");
            return;
        }

        chunkRenderer.Clear();
    }

    private void PlacePlayerAtStart(DungeonData dungeonData)
    {
        if (player == null || dungeon.startRoom == null) return;

        RectInt bounds = dungeon.startRoom.Bounds;

        Vector2Int center = new Vector2Int(
            bounds.x + bounds.width / 2,
            bounds.y + bounds.height / 2
        );

        player.position = chunkRenderer.GridToWorld(center, dungeonData.floorHeight + 1);
    }
}
