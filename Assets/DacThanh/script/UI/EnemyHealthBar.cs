using UnityEngine;
using UnityEngine.UI;

// Gắn lên Canvas (World Space) là con của enemy. Tự tìm EnemyAI ở object cha.
public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image fill;   // Image Type = Filled, Horizontal

    private EnemyAI enemy;
    private Camera cam;

    private void Awake() => enemy = GetComponentInParent<EnemyAI>();

    private void LateUpdate()
    {
        if (enemy == null) return;

        // Enemy chết thì ẩn thanh máu
        if (enemy.currentHp <= 0) { gameObject.SetActive(false); return; }

        fill.fillAmount = (float)enemy.currentHp / Mathf.Max(1, enemy.hp);

        if (cam == null) cam = Camera.main;
        if (cam != null) transform.rotation = cam.transform.rotation;   // luôn quay mặt về camera
    }
}
