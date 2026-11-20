using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum SkillType
{
    Active,
    Passive
}

[CreateAssetMenu(
    fileName = "New Skill",
    menuName = "Skill/Skill Data"
)]
public class SkillData : ScriptableObject
{
    public SkillType skillType = SkillType.Active;
    public Sprite icon;
    public float cooldown = 1f;
    [Header("Input")]
    public Key keyPressed = Key.A;
    [Header("Skill Effects")]
    public List<SkillEffect> effects;
}


public abstract class SkillEffect : ScriptableObject
{
    public abstract void Casting(SkillContext context);
}


public class SkillContext
{
    public GameObject Caster;
    public GameObject Target;
    public List<GameObject> Targets;

    public SkillContext(
        GameObject caster,
        GameObject target = null,
        List<GameObject> targets = null)
    {
        Caster = caster;
        Target = target;
        Targets = targets;
    }

    public List<GameObject> GetTargets()
    {
        if (Targets != null && Targets.Count > 0)
            return Targets;

        if (Target != null)
            return new List<GameObject> { Target };

        return new List<GameObject>();
    }
}