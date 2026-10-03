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

    [Header("Prefabs")]
    public GameObject Floor;      // pivot ở giữa, kích thước 1 ô
    public GameObject wall;       // pivot ở giữa, kích thước 1 ô
    public GameObject Corridor;   // pivot ở giữa, kích thước 1 ô
    public GameObject Gate;       // pivot Ở ĐÁY, cao 2 ô

    [Header("Decoration")]
    public List<Decor> Decor;
}

[System.Serializable]
public class Decor
{
    public GameObject decor;
    public int Rate;
}
