using System.Collections.Generic;
using UnityEngine;

// public enum EnemyType
// {
//     Minion,
//     Boss
//     
// }

[CreateAssetMenu(fileName = "New Character", menuName = "Character/Character data")]
public class CharacterData : ScriptableObject
{
    // public EnemyType dungeonWorld = EnemyType.Minion;

    [Header("Attibute")]
    public int atk;
    public int hp;
    public int armor;
    
    [Header("Skill")]
    public List<SkillData> skills;
    [Header("Equipment")]
    public List<SwordPrefabs> swordPrefabs;
    public List<ArmorPrefabs> armorPrefab;
}
[System.Serializable]
public class SwordPrefabs
{
    public GameObject prefab;
    public int Rate = 1;
}
public class ArmorPrefabs
{
    public GameObject prefab;
    public int Rate = 1;
}