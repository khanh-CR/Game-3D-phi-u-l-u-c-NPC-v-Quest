using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public Animator animator;
    public float attackDamage = 20f;
    public float attackRange = 2f;
    public float attackCooldown = 0.6f;

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

        // Chạy animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Phát âm thanh kiếm
        PlaySwordSound();

        // Kiểm tra enemy
        if (attackPoint == null)
            return;

        Collider[] enemies = Physics.OverlapSphere(
            attackPoint.position,
            attackRange,
            enemyLayer
        );
        // if (enemies != null)
        // {
        //     foreach (var e in enemies)
        //     {
        //         var p = gameObject.GetComponent<PlayerTest>();
        //         e.GetComponent<ISkillable>().TakeDamage(p.currentAtk);
        //     }
        // }
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