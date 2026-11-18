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
    [Header("Dungeon attibutes")]
    public DungeonWorld dungeonWorld = DungeonWorld.jungle;
    public int level = 1;
    public bool randomSeed = true;
    public int seed;
    public int maxFloor;
    
    [Header("Dungeon brick")]
    public GameObject portal;
    public List<FloorPrefab> Floors;     
    public GameObject wall;       
    public List<CorridorPrefab> Corridor;   
    public GameObject Gate;       
    public List<DecorPrefab> Decor;
    public GameObject roof; 
    
    [Header("Dungeon monster")]
    public List<MonsterPrefab> monster;
    public GameObject Boss;
    public List<Others> others;
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
[System.Serializable]
public class Others
{
    public GameObject prefab;
    public int Rate = 1;
}