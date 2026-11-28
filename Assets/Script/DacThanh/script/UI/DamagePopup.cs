using TMPro;
using UnityEngine;

// Gắn lên prefab "Text - TextMeshPro" (3D). Gọi DamagePopup.Spawn(...) khi enemy mất máu.
public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private float lifetime = 1.5f;   // tự hủy sau ngần này giây
    [SerializeField] private float riseSpeed = 1f;    // tốc độ bay lên (đơn vị / giây)

    private float age;
    private Camera cam;

    public static void Spawn(DamagePopup prefab, Vector3 position, int amount)
    {
        if (prefab == null) return;

        // Lệch ngẫu nhiên một chút để nhiều số cùng lúc không đè lên nhau
        DamagePopup popup = Instantiate(prefab, position + Random.insideUnitSphere * 0.2f, Quaternion.identity);
        popup.text.text = amount.ToString();
        Destroy(popup.gameObject, popup.lifetime);
    }

    private void Update()
    {
        age += Time.deltaTime;
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;
        text.alpha = 1f - age / lifetime;             // mờ dần (xóa dòng này nếu không muốn)

        if (cam == null) cam = Camera.main;
        if (cam != null) transform.rotation = cam.transform.rotation;   // luôn quay mặt về camera
    }
}
