using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Gắn lên mỗi ô skill (Skill_1, Skill_2, ...)
public class SkillSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;         // icon skill
    [SerializeField] private Image cooldownOverlay;   // Image Type = Filled, Radial 360
    [SerializeField] private TMP_Text cooldownText;   // số giây còn lại

    private bool showing;

    public void Setup(SkillData data)
    {
        if (iconImage != null)
            iconImage.sprite = data.icon;

        SetCooldown(0f, 1f);
    }

    public void SetCooldown(float remaining, float total)
    {
        bool onCooldown = remaining > 0f && total > 0f;

        if (cooldownOverlay != null)
        {
            if (cooldownOverlay.enabled != onCooldown)
                cooldownOverlay.enabled = onCooldown;

            if (onCooldown)
                cooldownOverlay.fillAmount = remaining / total;
        }

        if (cooldownText != null)
        {
            if (cooldownText.enabled != onCooldown)
                cooldownText.enabled = onCooldown;

            if (onCooldown)
            {
                // >= 1 giây: hiện số nguyên, < 1 giây: hiện 1 số thập phân
                cooldownText.text = remaining >= 1f
                    ? Mathf.CeilToInt(remaining).ToString()
                    : remaining.ToString("0.0");
            }
        }
    }
}