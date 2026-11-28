
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public Animator animator;
    public float attackDamage = 20f;
    public float attackRange = 2f;
    public float attackCooldown = 0.6f;

    [Header("Enemy Detection")]
    public Transform attackPoint;
    public LayerMask enemyLayer;

    private float nextAttackTime;

    void Update()
    {
        // New Input System - Left Mouse Click
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
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        if (attackPoint == null) return;

        Collider[] enemies = Physics.OverlapSphere(
            attackPoint.position,
            attackRange,
            enemyLayer
        );
        if (enemies != null)
        {
            foreach (var e in enemies)
            {
                var p = gameObject.GetComponent<PlayerTest>();
                e.GetComponent<ISkillable>().TakeDamage(p.currentAtk);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            attackPoint.position,
            attackRange
        );
    }
}
