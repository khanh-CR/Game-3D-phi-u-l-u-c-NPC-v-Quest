using System.Collections.Generic;
using UnityEngine;

public class BSPDungeonGenerators
{
    private Dungeon dungeon;

    private BSPNode root;

    public BSPNode Root => root;

    private int minLeafSize;
    private int minRoomSize;
    private int roomPadding;

    public List<DungeonRoom> rooms = new List<DungeonRoom>();

    public BSPDungeonGenerators(Dungeon dungeon, int minLeafSize, int minRoomSize, int roomPadding)
    {
        this.dungeon = dungeon;
        this.minLeafSize = minLeafSize;
        this.minRoomSize = minRoomSize;
        this.roomPadding = roomPadding;
    }

    public void Generate()
    {
        rooms.Clear();

        root = new BSPNode(dungeon.Bounds);

        SplitNode(root);

        CreateRooms(root);
    }

    private void SplitNode(BSPNode node)
    {
        int width = node.Bounds.width;
        int height = node.Bounds.height;

        if (width < minLeafSize * 2 &&
            height < minLeafSize * 2)
        {
            return;
        }

        bool splitHorizontal;

        if (width > height)
        {
            splitHorizontal = false;
        }
        else if (height > width)
        {
            splitHorizontal = true;
        }
        else
        {
            splitHorizontal = Random.value > 0.5f;
        }

        if (splitHorizontal)
        {
            int split = Random.Range(minLeafSize, height - minLeafSize + 1);

            RectInt bottomBounds = new RectInt(
                node.Bounds.xMin,
                node.Bounds.yMin,
                width,
                split
            );

            RectInt topBounds = new RectInt(
                node.Bounds.xMin,
                node.Bounds.yMin + split,
                width,
                height - split
            );

            node.Left = new BSPNode(bottomBounds);
            node.Right = new BSPNode(topBounds);
        }
        else
        {
            int split = Random.Range(minLeafSize, width - minLeafSize + 1);

            RectInt leftBounds = new RectInt(
                node.Bounds.xMin,
                node.Bounds.yMin,
                split,
                height
            );

            RectInt rightBounds = new RectInt(
                node.Bounds.xMin + split,
                node.Bounds.yMin,
                width - split,
                height
            );

            node.Left = new BSPNode(leftBounds);
            node.Right = new BSPNode(rightBounds);
        }

        SplitNode(node.Left);
        SplitNode(node.Right);
    }

    private void CreateRooms(BSPNode node)
    {
        if (node == null)
            return;

        if (!node.IsLeaf)
        {
            CreateRooms(node.Left);
            CreateRooms(node.Right);
            return;
        }

        RectInt leafBounds = node.Bounds;

        int maxRoomWidth = leafBounds.width - roomPadding * 2;
        int maxRoomHeight = leafBounds.height - roomPadding * 2;

        if (maxRoomWidth < minRoomSize ||
            maxRoomHeight < minRoomSize)
        {
            return;
        }

        int roomWidth = Random.Range(minRoomSize, maxRoomWidth + 1);
        int roomHeight = Random.Range(minRoomSize, maxRoomHeight + 1);

        int roomX = Random.Range(
            leafBounds.xMin + roomPadding,
            leafBounds.xMax - roomPadding - roomWidth + 1
        );

        int roomZ = Random.Range(
            leafBounds.yMin + roomPadding,
            leafBounds.yMax - roomPadding - roomHeight + 1
        );

        Vector2Int roomPosition = new Vector2Int(roomX, roomZ);
        Vector2Int roomSize = new Vector2Int(roomWidth, roomHeight);

        DungeonRoom room = new DungeonRoom(roomPosition, roomSize, leafBounds);

        node.Room = room;

        rooms.Add(room);
    }
}
