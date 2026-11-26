using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Effect/Atk Buff")]
public class AtkBuffEffect : SkillEffect
{
    public int bonusAtk = 1;
    public int bonusAtkPerLevel = 1; 
    [Header("Hiệu ứng")]
    public GameObject vfxPrefab;
    public float vfxLifetime = 2f;

    public override void Casting(SkillContext ctx)
    {
        // Chỉ tác động nếu caster có ISkillable
        if (!ctx.Caster.TryGetComponent(out ISkillable target)) return;

        int amount = Scale(bonusAtk, bonusAtkPerLevel, ctx.Level);
        target.AddAtk(amount);
        SpawnVfx(vfxPrefab, ctx.Caster.transform, vfxLifetime);

        // duration > 0: hết thời gian thì gỡ buff. duration = 0: giữ vĩnh viễn
        if (ctx.Duration > 0f)
            ctx.Runner.StartCoroutine(Revert(target, ctx.Duration, amount));
    }

    private IEnumerator Revert(ISkillable target, float duration, int amount)
    {
        yield return new WaitForSeconds(duration);
        target.AddAtk(-amount);
    }
}