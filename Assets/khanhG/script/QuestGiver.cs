using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

// Gắn lên NPC (cần Collider với Is Trigger bật, to hơn NPC để tạo vùng nói chuyện).
// E: mở/đóng hội thoại. Phím 1..9: nhận quest, hoặc nhận thưởng nếu quest đã hoàn thành.
public class QuestGiver : MonoBehaviour
{
    [SerializeField] private string npcName = "NPC";
    [SerializeField] private QuestData[] quests;          // giao bao nhiêu quest (main/side) cũng được
    [SerializeField] private GameObject interactHint;     // chữ "Nhấn E để nói chuyện"
    [SerializeField] private QuestUIManager ui;

    private List<QuestData> shown = new List<QuestData>();
    private bool playerNear, talking;

    private void Start() => SetHint(false);

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null || !playerNear) return;

        if (kb.eKey.wasPressedThisFrame)
        {
            if (talking) Close(); else Open();
            return;
        }

        if (!talking) return;
        if (kb.escapeKey.wasPressedThisFrame) { Close(); return; }

        // Phím 1..9 chọn quest tương ứng trong danh sách
        for (int i = 0; i < shown.Count; i++)
        {
            if (kb[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame)
            {
                Select(shown[i]);
                break;
            }
        }
    }

    private void Open()
    {
        if (QuestManager.Instance == null || ui == null) return;

        talking = true;
        SetHint(false);
        Refresh();
    }

    private void Close()
    {
        talking = false;
        ui.HideDialog();
        SetHint(playerNear);
    }

    // Hiện các quest đã mở khóa và chưa nhận thưởng; Main xếp trước Side (tối đa 9 quest cùng lúc)
    private void Refresh()
    {
        QuestManager qm = QuestManager.Instance;
        shown = quests
            .Where(q => q != null && qm.IsUnlocked(q) && qm.GetState(q) != QuestState.Rewarded)
            .OrderBy(q => q.type)
            .Take(9)
            .ToList();

        ui.ShowDialog(npcName, shown);
    }

    private void Select(QuestData q)
    {
        QuestManager qm = QuestManager.Instance;
        QuestState state = qm.GetState(q);

        if (state == QuestState.Available) qm.Accept(q);            // nhận (hoặc nhận lại nếu đã thất bại)
        else if (state == QuestState.Completed) qm.Claim(q);        // về trả quest, nhận gold

        Refresh();
    }

    private void SetHint(bool on)
    {
        if (interactHint != null) interactHint.SetActive(on);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNear = true;
        SetHint(!talking);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNear = false;
        if (talking) Close();
        SetHint(false);
    }
}