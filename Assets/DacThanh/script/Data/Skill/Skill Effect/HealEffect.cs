using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Effect/Heal")]
public class HealEffect : SkillEffect
{
    public int healPerTick = 1;
    public int healPerLevel = 1;  
    public int tickInterval = 1;

    [Header("Hiệu ứng")]
    public GameObject vfxPrefab;
    public float vfxLifetime = 2f;   // hiệu ứng tự mất sau ngần này giây

    public override void Casting(SkillContext ctx)
    {
        if (ctx == null)
        {
            Debug.LogError("SkillContext bị null!");
            return;
        }

        if (ctx.Caster == null)
        {
            Debug.LogError("Caster bị null!");
            return;
        }

        if (ctx.Runner == null)
        {
            Debug.LogError("SkillContext.Runner bị null!");
            return;
        }
 
        if (!ctx.Caster.TryGetComponent(out ISkillable target)) return;
        if (vfxPrefab != null)
            Destroy(Instantiate(vfxPrefab, ctx.Caster.transform), vfxLifetime);
        SpawnVfx(vfxPrefab, ctx.Caster.transform, vfxLifetime);
        ctx.Runner.StartCoroutine(HealRoutine(target, ctx.Duration, Scale(healPerTick, healPerLevel, ctx.Level)));
    }

    // Hồi máu mỗi tick trong khoảng duration (duration = 0 -> hồi vĩnh viễn)
    private IEnumerator HealRoutine(ISkillable target, float duration, int amount)
    {
        float interval = Mathf.Max(0.05f, tickInterval);

        for (float t = 0f; duration <= 0f || t < duration; t += interval)
        {
            target.Heal(amount);
            yield return new WaitForSeconds(interval);
        }
    }
}