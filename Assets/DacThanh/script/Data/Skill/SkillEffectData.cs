using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Effects/Spawn Fire Orbs")]
public class SpawnFireOrbEffect : SkillEffect
{
    public GameObject fireOrbPrefab;

    public int orbCount = 3;
    public float radius = 2f;

    public override void Casting(SkillContext context)
    {
        if (context.Caster == null || fireOrbPrefab == null)
            return;

        for (int i = 0; i < orbCount; i++)
        {
            float angle = i * 360f / orbCount;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                0f,
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ) * radius;

            Vector3 spawnPosition =
                context.Caster.transform.position + offset;

            GameObject orb = Instantiate(
                fireOrbPrefab,
                spawnPosition,
                Quaternion.identity
            );

            // Gán Caster cho quả cầu lửa
            FireOrb projectile = orb.GetComponent<FireOrb>();

            if (projectile != null)
                projectile.SetCaster(context.Caster);
        }
    }
}