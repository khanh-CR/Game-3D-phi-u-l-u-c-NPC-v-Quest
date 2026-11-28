using UnityEngine;

// 1 file duy nhất cho skill Orb. Tạo nhiều asset (DarkOrb, FireOrb...) bằng cách đổi prefab/damage/radius.
[CreateAssetMenu(menuName = "Skill/Effect/Orb")]
public class OrbEffect : SkillEffect
{
    public GameObject orbPrefab;    // cần Collider (Is Trigger) + Rigidbody (Is Kinematic)
    public int count = 3;
    public float radius = 2f;
    public float height = 1f;       // độ cao quỹ đạo so với chân caster
    public float speed = 200f;      // độ / giây
    public int damage = 1;
    public int damagePerLevel = 1;
    public LayerMask targets = ~0;  // layer bị orb gây damage (caster luôn được bỏ qua)

    public override void Casting(SkillContext ctx)
    {
        int dmg = Scale(damage, damagePerLevel, ctx.Level);
        for (int i = 0; i < count; i++)
        {
            var orbPosition = ctx.Caster.transform.position + Vector3.up * height;
            GameObject orb = Instantiate(orbPrefab, orbPosition, Quaternion.identity);
            orb.AddComponent<SkillOrb>().Init(ctx.Caster.transform, 360f / count * i, this, dmg);

            if (ctx.Duration > 0f) Destroy(orb, ctx.Duration);   // duration = 0 -> tồn tại mãi
        }
    }
}

// Tự gắn vào orb lúc runtime (AddComponent), không cần kéo vào prefab.
public class SkillOrb : MonoBehaviour
{
    private Transform caster;
    private OrbEffect cfg;
    private float angle;
    private int damage;
    public void Init(Transform caster, float startAngle, OrbEffect cfg, int damage)
    {
        this.caster = caster;
        this.cfg = cfg;
        this.damage = damage;
        angle = startAngle;
    }

    private void Update()
    {
        if (caster == null) { Destroy(gameObject); return; }

        angle += cfg.speed * Time.deltaTime;
        float rad = angle * Mathf.Deg2Rad;
        Vector3 center = caster.position + Vector3.up * cfg.height;
        transform.position = center + new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * cfg.radius;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (caster == null || other.transform.IsChildOf(caster)) return;
        if ((cfg.targets.value & (1 << other.gameObject.layer)) == 0) return;

        ISkillable target = other.GetComponentInParent<ISkillable>();
        if (target != null) target.TakeDamage(damage);
    }
}