
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class QuestUIManager : MonoBehaviour
{
    [Header("Hội thoại NPC")]
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TMP_Text dialogTitle;
    [SerializeField] private TMP_Text emptyText;
    [SerializeField] private Transform buttonContent;
    [SerializeField] private QuestButtonUI questButtonPrefab;

    [Header("Quest Tracker")]
    [SerializeField] private GameObject trackerPanel;
    [SerializeField] private TMP_Text trackerText;
    [SerializeField] private TMP_Text goldText;

    [Header("Thông báo")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private float messageTime = 3f;

    private QuestManager qm;
    private float messageUntil;

    private readonly StringBuilder sb = new StringBuilder();

    private void Start()
    {
        qm = QuestManager.Instance;

        if (qm == null)
        {
            Debug.LogError("Không tìm thấy QuestManager!");
            return;
        }

        qm.OnChanged += RefreshHud;
        qm.OnMessage += ShowMessage;
        HideDialog();
        RefreshHud();

        string pending = qm.TakePendingMessage();

        if (!string.IsNullOrEmpty(pending))
            ShowMessage(pending);
    }

    private void OnDestroy()
    {
        if (qm != null)
        {
            qm.OnChanged -= RefreshHud;
            qm.OnMessage -= ShowMessage;
        }
    }

    private void Update()
    {
        if (messageUntil > 0f && Time.time > messageUntil)
        {
            messageUntil = 0f;

            if (messageText != null)
                messageText.text = "";
        }

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            if (trackerPanel == null)
                return;

            trackerPanel.SetActive(!trackerPanel.activeSelf);
        }
    }

    // =====================================================
    // HỘI THOẠI NPC
    // =====================================================

    public void ShowDialog(
        string npcName,
        List<QuestData> quests,
        Action<QuestData> onSelect)
    {
        if (dialogPanel == null || buttonContent == null ||
            questButtonPrefab == null)
        {
            Debug.LogError("Chưa thiết lập đầy đủ Quest Dialog UI!");
            return;
        }

        dialogPanel.SetActive(true);

        if (dialogTitle != null)
            dialogTitle.text = npcName;

        // Xóa các Button cũ.
        for (int i = buttonContent.childCount - 1; i >= 0; i--)
        {
            Destroy(buttonContent.GetChild(i).gameObject);
        }

        bool hasQuest = quests != null && quests.Count > 0;

        if (emptyText != null)
            emptyText.gameObject.SetActive(!hasQuest);

        if (!hasQuest)
            return;

        foreach (QuestData q in quests)
        {
            if (q == null) continue;

            QuestButtonUI item =
                Instantiate(questButtonPrefab, buttonContent);

            QuestState state = qm.GetState(q);
            int kills = qm.GetKills(q);

            item.Setup(
                npcName,
                q,
                state,
                kills,
                () => onSelect?.Invoke(q)
            );
        }
    }

    public void HideDialog()
    {
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
    }

    // =====================================================
    // QUEST TRACKER
    // =====================================================
    
    private void RefreshHud()
    {
        if (qm == null)
            return;

        if (goldText != null)
            goldText.text = "   " + qm.Gold;

        sb.Clear();
        foreach (QuestData q in qm.ActiveQuests())
        {
            if (sb.Length > 0) sb.Append("   |   ");

            QuestState state = qm.GetState(q);
            int kills = qm.GetKills(q);

            string type = q.type == QuestType.Main
                ? "<color=#FFD54F>[Chính]</color>"
                : "<color=#80CBC4>[Phụ]</color>";

            if (state == QuestState.Completed)
            {
                sb.Append(
                    $"<b>{q.title}</b> {type}: " +
                    $"<color=#7CFC00> hoàn thành: ({kills}/{q.requiredKills}) - về gặp NPC nhận thưởng</color>   |   " +
                    $"Thưởng: {q.goldReward} Gold");
            }
            else
            {
                sb.Append(
                    $"<b>{q.title}</b> {type}: {q.description}  {kills}/{q.requiredKills} quái  |   " +
                    $"Phần thưởng: {q.goldReward} Gold");
            }
        }

        if (trackerText != null)
        {
            trackerText.text = sb.Length > 0
                ? sb.ToString()
                : "Chưa có nhiệm vụ đang thực hiện.";
        }
    }

    // =====================================================
    // THÔNG BÁO
    // =====================================================

    private void ShowMessage(string message)
    {
        if (messageText == null)
            return;

        messageText.text = message;
        messageUntil = Time.time + messageTime;
    }
}
