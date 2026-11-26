using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Gắn vào bất kỳ class nào (Player, Enemy, NPC...) muốn bị skill tác động.
// Skill chỉ cần GetComponent<ISkillable>() - có thì tác động, không có thì bỏ qua.
public interface ISkillable
{
    void TakeDamage(int amount);
    void Heal(int amount);
    void AddAtk(int amount);
}

// Class thường (không phải MonoBehaviour): nhúng vào Player/Enemy để dùng skill.
// Cách dùng:  public SkillBook skillBook;
//   Start():  skillBook.Init(this);
//   Update(): skillBook.Tick();             // enemy: skillBook.Tick(false) rồi AI gọi skillBook.Use(i)
[Serializable]
public class SkillBook
{
    public List<SkillData> skills;
    public List<SkillSlotUI> slots;   // cùng thứ tự với skills (enemy có thể để trống)
    private MonoBehaviour owner;
    private float[] nextUse;          // mỗi skill 1 mốc cooldown riêng
    private int[] levels; 
    private int unlocked = int.MaxValue; 
    public void Init(MonoBehaviour owner)
    {
        this.owner = owner;
        nextUse = new float[skills.Count];

        // for (int i = 0; i < slots.Count && i < skills.Count; i++)
        //     if (slots[i] != null && skills[i] != null)
        //         slots[i].Setup(skills[i]);
        levels = new int[skills.Count];
        for (int i = 0; i < skills.Count; i++) 
        {
            levels[i] = 1;
            if (i < slots.Count && slots[i] != null && skills[i] != null)
                slots[i].Setup(skills[i]); 
        }
    }
    // Mở khóa count skill đầu tiên; slot chưa mở sẽ bị ẩn
    public void SetUnlocked(int count)
        {
        unlocked = count;
        for (int i = 0; i < slots.Count; i++)
            if (slots[i] != null) slots[i].gameObject.SetActive(i < unlocked);
        }

    public void Tick(bool readInput = true)
    {
        if (nextUse == null) return;

        for (int i = 0; i < skills.Count && i < unlocked; i++)
        {
            SkillData skill = skills[i];
            if (skill == null) continue;

            if (readInput && Keyboard.current != null && skill.skillType == SkillType.Active && skill.keyPressed != Key.None &&
                Keyboard.current[skill.keyPressed].wasPressedThisFrame)
                Use(i);

            if (i < slots.Count && slots[i] != null)
                slots[i].SetCooldown(Mathf.Max(0f, nextUse[i] - Time.time), skill.cooldown);
        }
    }

    // public để gọi từ Button.onClick hoặc AI của enemy
    public void Use(int index)
    {
        if (index < 0 || index >= skills.Count || index >= unlocked || skills[index] == null || IsOnCooldown(index))
            return;

        SkillData skill = skills[index];
        nextUse[index] = Time.time + skill.cooldown;

        var context = new SkillContext(owner.gameObject, owner, skill.duration, levels[index]);
        foreach (SkillEffect effect in skill.effects)
            if (effect != null) effect.Casting(context);
    }
    public bool UpgradeSkill(int index)
    {
        if (index < 0 || index >= skills.Count || index >= unlocked || skills[index] == null)
            return false;
        if (levels[index] >= skills[index].maxLevel)
            return false;

        levels[index]++;
        return true;
    }

    public int GetLevel(int index) => levels[index];
    public bool IsOnCooldown(int index) => Time.time < nextUse[index];
}