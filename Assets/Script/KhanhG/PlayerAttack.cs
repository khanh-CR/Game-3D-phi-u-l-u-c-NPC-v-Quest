using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public Animator animator;
    public int attackDamage = 20;
    public float attackRange = 2f;
    public float attackCooldown = 0.5f;

    [Header("Attack Audio")]
    public AudioClip swordSound;
    [Range(0f, 1f)]
    public float swordVolume = 1f;

    private AudioSource attackAudioSource;

    [Header("Enemy Detection")]
    public Transform attackPoint;
    public LayerMask enemyLayer;

    private float nextAttackTime;

    void Awake()
    {
        // Tạo AudioSource riêng để phát tiếng kiếm
        attackAudioSource = gameObject.AddComponent<AudioSource>();

        attackAudioSource.playOnAwake = false;
        attackAudioSource.loop = false;
        attackAudioSource.volume = 1f;
        attackAudioSource.spatialBlend = 0f;
    }

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame &&
            Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void Attack()
    {
        Debug.Log("ATTACK ĐƯỢC GỌI!");

        if (animator != null)
            animator.SetTrigger("Attack");

        PlaySwordSound();

        if (attackPoint == null)
            return;

        // Tìm tất cả Collider trong vùng tấn công
        Collider[] targets = Physics.OverlapSphere(
            attackPoint.position,
            attackRange
        );

        PlayerTest p = GetComponent<PlayerTest>();
        int damage = (p != null) ? p.currentAtk : attackDamage;

        foreach (var target in targets)
        {
            // Kiểm tra Tag Enemy
            if (!target.CompareTag("Enemy"))
                continue;

            ISkillable enemySkillable = target.GetComponent<ISkillable>();

            if (enemySkillable == null)
                continue;

            enemySkillable.TakeDamage(damage);

            Debug.Log("Đã tấn công Enemy: " + target.name);
        }
    }

    void PlaySwordSound()
    {
        if (swordSound == null)
        {
            Debug.LogError("CHƯA GÁN SWORD SOUND!");
            return;
        }

        attackAudioSource.PlayOneShot(swordSound, swordVolume);

        Debug.Log("ĐÃ GỌI PHÁT TIẾNG KIẾM!");
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            attackPoint.position,
            attackRange
        );
    }
}