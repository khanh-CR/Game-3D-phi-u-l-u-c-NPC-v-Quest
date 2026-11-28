
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuestGiver : MonoBehaviour
{
    [Header("Thông tin NPC")]
    [SerializeField] private string giverId = "NPC_01";
    [SerializeField] private string npcName = "NPC";

    [Header("Danh sách Quest NPC có thể cung cấp")]
    [SerializeField] private QuestData[] quests;

    [Header("UI")]
    [SerializeField] private GameObject interactHint;
    [SerializeField] private QuestUIManager ui;

    [Header("Tương tác")]
    [SerializeField] private Key keyPressed = Key.E;

    public string GiverId => giverId;
    public QuestData[] Quests => quests;

    private bool playerNear;
    private bool talking;

    private QuestManager qm;

    private void Start()
    {
        qm = QuestManager.Instance;

        if (qm == null)
        {
            Debug.LogError("Không tìm thấy QuestManager!");
            return;
        }

        qm.RegisterGiver(this);
        SetHint(false);
    }

    private void OnDestroy()
    {
        if (qm != null)
            qm.OnChanged -= RefreshIfTalking;
    }

    private void Update()
    {
        if (!playerNear || Keyboard.current == null)
            return;

        if (Keyboard.current[keyPressed].wasPressedThisFrame)
        {
            if (talking)
                Close();
            else
                Open();
        }

        if (talking &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
    }

    private void Open()
    {
        if (qm == null || ui == null)
            return;

        talking = true;
        SetHint(false);

        qm.OnChanged -= RefreshIfTalking;
        qm.OnChanged += RefreshIfTalking;

        Refresh();
    }

    private void Close()
    {
        talking = false;

        if (qm != null)
            qm.OnChanged -= RefreshIfTalking;

        if (ui != null)
            ui.HideDialog();

        SetHint(playerNear);
    }

    private void RefreshIfTalking()
    {
        if (talking)
            Refresh();
    }

    private void Refresh()
    {
        if (qm == null || ui == null)
            return;

        List<QuestData> shown = qm.GetOffersFor(this);

        ui.ShowDialog(
            npcName,
            shown,
            SelectQuest
        );
    }

    private void SelectQuest(QuestData q)
    {
        if (qm == null || q == null)
            return;

        QuestState state = qm.GetState(q);

        if (state == QuestState.Available)
        {
            qm.Accept(q);
        }
        else if (state == QuestState.Completed)
        {
            qm.Claim(q);
        }

        Refresh();
    }

    private void SetHint(bool on)
    {
        if (interactHint != null)
            interactHint.SetActive(on);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerNear = true;

        if (!talking)
            SetHint(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerNear = false;

        if (talking)
            Close();

        SetHint(false);
    }
}
