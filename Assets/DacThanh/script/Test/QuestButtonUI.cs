using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text buttonText;

    public void Setup(
        string npcName,
        QuestData quest,
        QuestState state,
        int kills,
        Action onClick)
    {
        if (quest == null)
            return;

        if (button == null)
            button = GetComponent<Button>();

        if (button == null || buttonText == null)
        {
            Debug.LogError(
                "QuestButtonUI thiếu Button hoặc TMP_Text!");
            return;
        }

        string type = quest.type == QuestType.Main
            ? "<color=#FFD54F>[Nhiệm vụ chính]</color>"
            : "<color=#80CBC4>[Nhiệm vụ phụ]</color>";

        string status;

        switch (state)
        {
            case QuestState.Available:
                status = "Nhấn để nhận nhiệm vụ";
                break;

            case QuestState.Accepted:
            case QuestState.InProgress:
                status = $"Đang thực hiện: {kills}/{quest.requiredKills}";
                break;

            case QuestState.Completed:
                status = "<color=#7CFC00>Hoàn thành - Nhấn để nhận thưởng</color>";
                break;

            default:
                status = "Đã nhận thưởng";
                break;
        }

        buttonText.text =
            $"NPC: {npcName}   |   " +
            $"<b>{quest.title}</b> {type}   |   " +
            $"Số lượng: {quest.requiredKills}   |   " +
            $"Phần thưởng: {quest.goldReward} Gold   |   " +
            $"{quest.description}   |   " +
            $"{status}";

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());

        button.interactable =
            state == QuestState.Available ||
            state == QuestState.Completed;
    }
}