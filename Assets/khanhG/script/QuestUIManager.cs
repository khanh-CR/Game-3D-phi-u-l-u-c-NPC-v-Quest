
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class QuestUIManager : MonoBehaviour
{
    [Header("Quest UI")]
    [SerializeField] private GameObject questPanel;

    [Header("Quest Icon - Always Visible")]
    [SerializeField] private GameObject questIcon;
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;

    [Header("Quest Text")]
    [SerializeField] private TMP_Text questNameText;
    [SerializeField] private TMP_Text questDescriptionText;
    [SerializeField] private TMP_Text questRewardText;

    [Header("NPC Interaction")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform npc;
    [SerializeField] private float interactDistance = 3f;

    private bool hasQuest = false;

    private bool IsNearNPC()
    {
        if (player == null || npc == null)
            return false;

        return Vector3.Distance(player.position, npc.position)
            <= interactDistance;
    }

    private void Awake()
    {
        if (questPanel != null)
            questPanel.SetActive(false);

        if (questIcon != null)
            questIcon.SetActive(true);
    }

    private void OnEnable()
    {
        if (openButton != null)
        {
            openButton.onClick.RemoveListener(ToggleQuestFromIcon);
            openButton.onClick.AddListener(ToggleQuestFromIcon);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseQuest);
            closeButton.onClick.AddListener(CloseQuest);
        }
    }

    private void OnDisable()
    {
        if (openButton != null)
            openButton.onClick.RemoveListener(ToggleQuestFromIcon);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseQuest);
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // E chi hoat dong khi gan NPC
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (IsNearNPC())
            {
                if (hasQuest)
                    ToggleQuest();
                else
                    Debug.Log("Chua nhan nhiem vu!");
            }
            else
            {
                Debug.Log("Qua xa NPC!");
            }
        }

        // ESC dong bang, khong tat icon
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            CloseQuest();
    }

    public void SetQuest(
        string questName,
        string description,
        string reward)
    {
        hasQuest = true;

        if (questNameText != null)
            questNameText.text = questName;

        if (questDescriptionText != null)
            questDescriptionText.text = description;

        if (questRewardText != null)
            questRewardText.text = "Phan thuong: " + reward;

        CloseQuest();

        if (questIcon != null)
            questIcon.SetActive(true);
    }

    // Bam icon de mo/tat bang o bat ky dau
    public void ToggleQuestFromIcon()
    {
        if (!hasQuest)
        {
            Debug.Log("Chua nhan nhiem vu!");
            return;
        }

        ToggleQuest();
    }

    public void OpenQuest()
    {
        if (questPanel != null && hasQuest)
            questPanel.SetActive(true);
    }

    public void CloseQuest()
    {
        if (questPanel != null)
            questPanel.SetActive(false);

        // Luon giu icon hien thi
        if (questIcon != null)
            questIcon.SetActive(true);
    }

    public void ToggleQuest()
    {
        if (questPanel == null)
            return;

        if (questPanel.activeSelf)
            CloseQuest();
        else
            OpenQuest();
    }
}
