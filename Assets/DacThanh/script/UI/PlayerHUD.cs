using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Gắn lên Canvas HUD (Screen Space - Overlay). Đọc chỉ số từ PlayerTest, chỉ cập nhật UI khi giá trị đổi.
public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private PlayerTest player;   // bỏ trống nếu player được spawn lúc chạy game
    [SerializeField] private Slider hpFill;        // Image Type = Filled, Horizontal
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Slider expFill;       // Image Type = Filled, Horizontal
    [SerializeField] private TMP_Text levelText;

    private int lastHp = -1, lastMaxHp = -1, lastExp = -1, lastLevel = -1;

    private void Update()
    {
        if (player == null)
        {
            if (DungeonManagers.instance == null || DungeonManagers.instance.player == null) return;
            player = DungeonManagers.instance.player.GetComponent<PlayerTest>();
            if (player == null) return;
        }

        if (player.currentHp != lastHp || player.hp != lastMaxHp)
        {
            lastHp = player.currentHp;
            lastMaxHp = player.hp;
            hpFill.value = player.hp > 0 ? (float)player.currentHp / player.hp : 0f;
            hpText.text = $"HP: {player.currentHp} / {player.hp}";
        }

        if (player.exp != lastExp || player.level != lastLevel)
        {
            lastExp = player.exp;
            lastLevel = player.level;

            bool max = player.level >= player.maxLevel;
            expFill.value = max ? 1f : (float)player.exp / player.ExpToNext;
            levelText.text = max ? $"Lv {player.level} (MAX)" : $"Lv {player.level}";
        }
    }
}
