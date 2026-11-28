using UnityEngine;
using UnityEngine.InputSystem;

public enum SkillType { Active, Passive }

[CreateAssetMenu(menuName = "Skill/Skill Data")]
public class SkillData : ScriptableObject
{
    public Sprite icon;
    public SkillType skillType = SkillType.Active;
    public Key keyPressed;
    public float cooldown = 5f;
    public int maxLevel = 3;

    [Tooltip("Thời gian skill tồn tại (giây). 0 = vĩnh viễn")]
    public float duration = 0f;

    public SkillEffect[] effects;
}