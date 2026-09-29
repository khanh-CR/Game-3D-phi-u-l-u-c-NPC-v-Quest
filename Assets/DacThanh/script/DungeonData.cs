using System.Collections.Generic;
using UnityEngine;

public enum DungeonWorld
{
    jungle,
    cave
}

[CreateAssetMenu(fileName = "DungeonData", menuName = "Dungeon/Dungeon Data")]
public class DungeonData : ScriptableObject
{
    public DungeonWorld dungeonWorld = DungeonWorld.jungle;

    [Header("Materials")]
    public Material voidMaterial;
    public Material floorMaterial;
    public Material wallMaterial;
    public Material corridorMaterial;
    public Material gateMaterial;

    [Header("Height (số block theo trục Y)")]
    [Min(1)] public int voidHeight = 1;
    [Min(1)] public int floorHeight = 1;
    [Min(1)] public int wallHeight = 5;
    [Min(1)] public int corridorHeight = 1;
    [Min(1)] public int gateHeight = 1;

    [Header("Decoration")]
    public List<Decor> Decor;

    public Material GetMaterial(DungeonTileType type)
    {
        switch (type)
        {
            case DungeonTileType.Floor: return floorMaterial;
            case DungeonTileType.Wall: return wallMaterial;
            case DungeonTileType.Corridor: return corridorMaterial;
            case DungeonTileType.Gate: return gateMaterial;
            default: return voidMaterial;
        }
    }

    public int GetHeight(DungeonTileType type)
    {
        switch (type)
        {
            case DungeonTileType.Floor: return Mathf.Max(1, floorHeight);
            case DungeonTileType.Wall: return Mathf.Max(1, wallHeight);
            case DungeonTileType.Corridor: return Mathf.Max(1, corridorHeight);
            case DungeonTileType.Gate: return Mathf.Max(1, gateHeight);
            default: return Mathf.Max(1, voidHeight);
        }
    }
}

[System.Serializable]
public class Decor
{
    public GameObject prefab;
    public int Rate;
}
