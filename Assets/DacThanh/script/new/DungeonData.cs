using System.Collections.Generic;
using UnityEngine;

public enum DungeonWorld
{
    jungle,
    cave,
}

[CreateAssetMenu(fileName = "DungeonData", menuName = "Dungeon/Dungeon Data")]
public class DungeonData : ScriptableObject
{
    public DungeonWorld dungeonWorld = DungeonWorld.jungle;

    [Header("Dungeon brick")]
    public List<FloorPrefab> Floors;      // pivot ở giữa, kích thước 1 ô
    public GameObject wall;       // pivot ở giữa, kích thước 1 ô
    public List<CorridorPrefab> Corridor;   // pivot ở giữa, kích thước 1 ô
    public GameObject Gate;       // pivot Ở ĐÁY, cao 2 ô
    public List<DecorPrefab> Decor;
    public GameObject roof;   
    [Header("Monster")]
    public List<MonsterPrefab> monster;
    public GameObject Boss;
    public GameObject NPC;
}

[System.Serializable]
public class DecorPrefab
{
    public GameObject prefab;
    public int Rate;
}
[System.Serializable]
public class FloorPrefab
{
    public GameObject prefab;
    public int Rate = 1;
}
[System.Serializable]
public class CorridorPrefab
{
    public GameObject prefab;
    public int Rate = 1;
}
[System.Serializable]
public class MonsterPrefab
{
    public GameObject prefab;
    public int Rate = 1;
}