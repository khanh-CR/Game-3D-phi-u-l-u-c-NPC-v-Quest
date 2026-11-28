using System;
using System.Collections;
using UnityEngine;

public class AttackArea : MonoBehaviour
{
    private bool wasPlayerDamage;
    [SerializeField] private EnemyAI enemy;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            var player = other.GetComponent<PlayerTest>();
            ISkillable skillable = other.GetComponent<ISkillable>();
            if (skillable != null)
            {
                player.TakeDamage(enemy.atk);
                wasPlayerDamage = true;
            }
            
        }
    }

    IEnumerator Hit()
    {
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }
}
