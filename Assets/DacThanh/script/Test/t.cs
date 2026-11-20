using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSkill : MonoBehaviour
{
    [Header("Skill Settings")]

    public List<SkillData> skills;
    public List<SkillSlotUI> skillSlots;   // kéo các ô skill (có SkillSlotUI) vào đây, cùng thứ tự với skills

    private float[] nextUseTimes;          // mỗi skill 1 mốc thời gian riêng

    private void Start()
    {
        nextUseTimes = new float[skills.Count];
        SetupSkill();
    }

    private void Update()
    {
        if (Keyboard.current == null || nextUseTimes == null)
            return;

        for (int i = 0; i < skills.Count; i++)
        {
            SkillData skill = skills[i];
            if (skill == null)
                continue;

            // Dùng phím cấu hình trong SkillData
            if (skill.skillType == SkillType.Active &&
                Keyboard.current[skill.keyPressed].wasPressedThisFrame)
            {
                UseSkill(i);
            }

            UpdateSlotUI(i);
        }
    }

    public void SetupSkill()
    {
        for (int i = 0; i < skillSlots.Count && i < skills.Count; i++)
        {
            skillSlots[i].Setup(skills[i]);
        }
    }

    private void UpdateSlotUI(int index)
    {
        if (index >= skillSlots.Count || skillSlots[index] == null)
            return;

        float remaining = Mathf.Max(0f, nextUseTimes[index] - Time.time);
        skillSlots[index].SetCooldown(remaining, skills[index].cooldown);
    }

    // public để có thể gọi từ Button.onClick (truyền index của skill)
    public void UseSkill(int index)
    {
        if (index < 0 || index >= skills.Count)
            return;

        SkillData skill = skills[index];
        if (skill == null)
            return;

        // Đang cooldown thì không dùng được
        if (Time.time < nextUseTimes[index])
            return;

        nextUseTimes[index] = Time.time + skill.cooldown;

        SkillContext context = new SkillContext(caster: gameObject);

        if (skill.effects != null)
        {
            foreach (SkillEffect effect in skill.effects)
            {
                if (effect != null)
                    effect.Casting(context);
            }
        }

        Debug.Log("Used skill: " + skill.name);
    }

    public bool IsOnCooldown(int index)
    {
        return Time.time < nextUseTimes[index];
    }
}