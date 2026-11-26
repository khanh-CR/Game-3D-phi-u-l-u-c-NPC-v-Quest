using UnityEngine;

public abstract class SkillEffect : ScriptableObject
{
    public abstract void Casting(SkillContext context);
    protected static int Scale(int baseValue, int perLevel, int level) => baseValue + perLevel * (level - 1);
    protected void SpawnVfx(GameObject prefab, Transform parent, float lifetime)
    {
        if (prefab != null) Destroy(Instantiate(prefab, parent), lifetime);
    }
}

public class SkillContext
{
    public readonly GameObject Caster;
    public readonly MonoBehaviour Runner;   // dùng để StartCoroutine từ ScriptableObject
    public readonly float Duration;         // 0 = vĩnh viễn
    public readonly int Level;
    public SkillContext(GameObject caster, MonoBehaviour runner = null, float duration = 0f, int level = 1)
    {
        Caster = caster;
        Runner = runner;
        Duration = duration;
        Level = level;
    }
}