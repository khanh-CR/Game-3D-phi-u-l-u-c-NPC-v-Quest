using System.Collections.Generic;
using UnityEngine;

public class CastingSkill : MonoBehaviour
{
    public void CastSkill(
        SkillData skill,
        GameObject caster,
        GameObject target = null,
        List<GameObject> targets = null)
    {
        if (skill == null || caster == null)
            return;

        SkillContext context = new SkillContext(caster, target, targets);

        foreach (SkillEffect effect in skill.effects)
        {
            if (effect == null)
                continue;

            effect.Casting(context);
        }
    }
}