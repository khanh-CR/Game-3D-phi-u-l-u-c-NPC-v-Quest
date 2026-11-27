using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

// Gắn lên Canvas. Đặt ở scene nào thì scene đó hiện được quest / gold / thông báo.
public class QuestUIManager : MonoBehaviour
{
    [Header("Hội thoại NPC")]
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TMP_Text dialogText;

    [Header("HUD")]
    [SerializeField] private TMP_Text trackerText;    // danh sách nhiệm vụ đang làm
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text messageText;    // thông báo ngắn
    [SerializeField] private float messageTime = 3f;

    private QuestManager qm;
    private float messageUntil;
    private readonly StringBuilder sb = new StringBuilder();

    private void Start()
    {
        qm = QuestManager.Instance;   // lấy ở Start để chắc chắn QuestManager đã Awake
        if (qm == null) return;

        qm.OnChanged += RefreshHud;
        qm.OnMessage += ShowMessage;
        RefreshHud();
        HideDialog();

        string pending = qm.TakePendingMessage();   // vd: "nhiệm vụ thất bại" sau khi rời dungeon
        if (pending != null) ShowMessage(pending);
    }

    private void OnDestroy()
    {
        if (qm == null) return;
        qm.OnChanged -= RefreshHud;
        qm.OnMessage -= ShowMessage;
    }

    private void Update()
    {
        if (messageUntil > 0f && Time.time > messageUntil)
        {
            messageUntil = 0f;
            SetText(messageText, "");
        }
    }

    #region Hội thoại
    public void ShowDialog(string npcName, List<QuestData> list)
    {
        sb.Clear();
        sb.AppendLine($"<b>{npcName}</b>\n");

        if (list.Count == 0) sb.AppendLine("Hiện không có nhiệm vụ nào.");

        for (int i = 0; i < list.Count; i++)
        {
            QuestData q = list[i];
            string kind = q.type == QuestType.Main ? "<color=#FFD54F>[Chính]</color>" : "[Phụ]";
            sb.AppendLine($"[{i + 1}] {kind} {q.title} (+{q.goldReward} Gold) - {StateLabel(q)}");
            sb.AppendLine($"      {q.description}");
        }

        sb.AppendLine("\nNhấn số để chọn   |   E / Esc để đóng");
        SetText(dialogText, sb.ToString());
        if (dialogPanel != null) dialogPanel.SetActive(true);
    }

    public void HideDialog()
    {
        if (dialogPanel != null) dialogPanel.SetActive(false);
    }

    private string StateLabel(QuestData q)
    {
        switch (qm.GetState(q))
        {
            case QuestState.Available: return "Nhấn số để nhận";
            case QuestState.Completed: return "<color=#7CFC00>Hoàn thành - nhấn số để nhận thưởng</color>";
            default: return $"Đang làm {qm.GetKills(q)}/{q.requiredKills}";
        }
    }
    #endregion

    #region HUD
    private void RefreshHud()
    {
        SetText(goldText, $"Gold: {qm.Gold}");

        sb.Clear();
        foreach (QuestData q in qm.ActiveQuests())
        {
            sb.AppendLine(qm.GetState(q) == QuestState.Completed
                ? $"{q.title}: <color=#7CFC00>Hoàn thành - về gặp NPC</color>"
                : $"{q.title}: {qm.GetKills(q)}/{q.requiredKills}");
        }
        SetText(trackerText, sb.ToString());
    }

    private void ShowMessage(string message)
    {
        SetText(messageText, message);
        messageUntil = Time.time + messageTime;
    }

    private static void SetText(TMP_Text t, string s)
    {
        if (t != null) t.text = s;
    }
    #endregion
}
