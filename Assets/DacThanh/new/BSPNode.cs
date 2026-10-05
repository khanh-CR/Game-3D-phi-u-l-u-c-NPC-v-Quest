using UnityEngine;

public class BSPNode
{
    public RectInt Bounds;

    public BSPNode Left;
    public BSPNode Right;

    public DungeonRoom Room;

    public bool IsLeaf
    {
        get
        {
            return Left == null && Right == null;
        }
    }

    public BSPNode(RectInt bounds)
    {
        Bounds = bounds;
    }
}
