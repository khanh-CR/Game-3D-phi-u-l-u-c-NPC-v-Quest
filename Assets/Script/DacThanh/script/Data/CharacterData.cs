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
}
