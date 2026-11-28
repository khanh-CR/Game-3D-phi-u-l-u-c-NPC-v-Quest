using System.Collections;
using UnityEngine;

public class AttackArea : MonoBehaviour
{
    [SerializeField] private EnemyAI enemy;
    

    private void OnTriggerEnter(Collider other)
    {
        // Chỉ tấn công Player
        if (!other.transform.root.CompareTag("Player"))
            return;

        ISkillable target = other.GetComponentInParent<ISkillable>();

        if (target != null)
        {
            target.TakeDamage(enemy.atk);
            Debug.Log("Enemy đã tấn công Player!");
        }

        StartCoroutine(Hit());
    }

    IEnumerator Hit()
    {
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }
}