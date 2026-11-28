
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class EnemyAI : MonoBehaviour, ISkillable
{
    [Header("Reference")]
    [SerializeField] public CharacterData data;
    [SerializeField] public GameObject player;
    [SerializeField] private GameObject attackArea;
    [SerializeField] private GameObject RightHand;
    [Header("attibute")]
    public int atk;
    public int hp;
    public int armor;
    public int currentHp;
    private void dataInit()
    {
        this.atk = data.atk;
        this.hp = data.hp;
        this.armor = data.armor;
        this.currentHp = data.hp;
    }
    [SerializeField] private float attackRange = 2.5f;
    private float attackSpeed = 0.5f;
    private bool isTargetInRange;
    private Animator animator;
    public int expReward = 20;
    
    
    [Header("vision check")]
    [SerializeField] private float viewDistance = 8f;
    [SerializeField, Range(0f, 360f)]
    private float viewAngle = 120f;
    [SerializeField] private float eyeHeight = 1.5f;
    [SerializeField] private float targetHeight = 1f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float stoppingDistance = 2f;
    [Header("Audio")]
    public AudioClip swordSound;
    public AudioClip walkSound;
    public AudioClip hitSound;
    [Range(0f, 1f)]
    public float swordVolume = 1f;
    private AudioSource attackAudioSource;
    [Header("agent AI")]
    private NavMeshAgent agent;
    private bool canSeePlayer;
    private float actionTime;
    private bool isWaiting;
    private bool isMoving;
    [Header("Damage popup")]
    [SerializeField] private DamagePopup damagePopupPrefab;
    [SerializeField] private float popupHeight = 2.2f;  
    private void Awake()
    {
        
        OnSpawn();
    }

    private void Start()
    {
        animator = gameObject.GetComponent<Animator>();
    }

    private bool isdead;
    private void Update()
    {
        if (isdead || player == null || !agent.isOnNavMesh)
            return;
        CheckHP();
        if (isdead) return;
        canSeePlayer = CanSeePlayer();
        Move();
        Attack();
        Animation();
        
    }

    public void CheckHP()
    {
        if (currentHp > 0 || isdead) return;

        isdead = true;
        if (player.TryGetComponent(out PlayerTest p)) p.AddExp(expReward);
        if (QuestManager.Instance != null)
            QuestManager.Instance.ReportKill();
        StopAllCoroutines();               // dừng Hit() đang bắn trigger attack
        isAttacking = false;
        agent.isStopped = true;
        agent.ResetPath();

        animator.ResetTrigger("attack");
        animator.ResetTrigger("run");
        animator.ResetTrigger("idle");
        animator.SetTrigger("die");

        Destroy(gameObject, 5f);
    }
    public void Move()
    {
        if (canSeePlayer)
        {
            if (IsInAttackRange())
            {
                agent.isStopped = true;
                agent.ResetPath();
                if(!isAttacking) actionTime = Time.time + 2f;
                isWaiting = true;
            }
            else if (isWaiting && Time.time < actionTime)
            {
                agent.isStopped = true;
            }
            else
            {
                isWaiting = false;
                agent.isStopped = false;
                agent.SetDestination(player.transform.position);
                
            }
        }
        else
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }
    public void Attack()
    {
        if (!IsInAttackRange())
            return;
        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                5f * Time.deltaTime
            );
        }
        if (!isAttacking)
        {
            PlaySwordSound();
            StartCoroutine(Hit());
        }
    }
    private bool isAttacking = false;
    private int n = 1;
    IEnumerator Hit()
    {
        isAttacking = true;
        attackArea.SetActive(true);
        while (canSeePlayer && IsInAttackRange())
        {
            animator.SetTrigger("attack");
            Debug.Log("Attack " + n);
            n++;

            yield return new WaitForSeconds(1f / attackSpeed);
        }
        isAttacking = false;
    }

    public void Animation()
    {
        isMoving = agent.isOnNavMesh && !agent.isStopped && agent.desiredVelocity.sqrMagnitude > 0.01f;
        animator.ResetTrigger("run");
        animator.ResetTrigger("idle");
        animator.SetTrigger(isMoving ? "run" : "idle");
    }
    private bool IsInAttackRange()
    {
        
        Vector3 a = transform.position;
        Vector3 b = player.transform.position;
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b) <= attackRange;
    }
    
    public void OnSpawn()
    {
        if (data != null) dataInit(); else currentHp = hp;
        var dm = DungeonManagers.instance;
        if (dm == null) return;
        player = dm.player;
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stoppingDistance;
        attackAudioSource = gameObject.AddComponent<AudioSource>();
        attackAudioSource.playOnAwake = false;
        attackAudioSource.loop = false;
        attackAudioSource.volume = 1f;
        attackAudioSource.spatialBlend = 0f;
        
    }
    private bool CanSeePlayer()
    {
        Vector3 eyePosition =
            transform.position + Vector3.up * eyeHeight;

        Vector3 targetPosition =
            player.transform.position + Vector3.up * targetHeight;

        Vector3 direction = targetPosition - eyePosition;
        float distance = direction.magnitude;

        // 1. Kiểm tra khoảng cách nhìn.
        if (distance > viewDistance)
            return false;

        // 2. Kiểm tra góc nhìn phía trước Enemy.
        float angle = Vector3.Angle(
            transform.forward,
            direction
        );

        if (angle > viewAngle * 0.5f)
            return false;

        // 3. Kiểm tra có tường/vật cản chắn tầm nhìn không.
        if (Physics.Raycast(
            eyePosition,
            direction.normalized,
            distance,
            obstacleMask,
            QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        // Vòng tròn phạm vi nhìn trên mặt đất.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        // Đường hướng nhìn.
        Vector3 left = Quaternion.Euler(
            0f, -viewAngle * 0.5f, 0f
        ) * transform.forward;

        Vector3 right = Quaternion.Euler(
            0f, viewAngle * 0.5f, 0f
        ) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(
            transform.position + Vector3.up * eyeHeight,
            left * viewDistance
        );

        Gizmos.DrawRay(
            transform.position + Vector3.up * eyeHeight,
            right * viewDistance
        );
    }
    void PlaySwordSound()
    {
        if (swordSound == null)
        {
            Debug.LogError("CHƯA GÁN SWORD SOUND!");
            return;
        }
        attackAudioSource.PlayOneShot(swordSound, swordVolume);
    }
    public void TakeDamage(int amount)
    {
        attackAudioSource.PlayOneShot(hitSound, swordVolume);
        if (isdead) return;
        currentHp = Mathf.Max(0, currentHp - amount);
        DamagePopup.Spawn(damagePopupPrefab, transform.position + Vector3.up * popupHeight, amount);
    }

    public void Heal(int amount) => currentHp = Mathf.Min(hp, currentHp + amount);
    public void AddAtk(int amount) => atk += amount;
}
