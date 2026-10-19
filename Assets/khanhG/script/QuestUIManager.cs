
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class QuestUIManager : MonoBehaviour
{
    [Header("Quest UI")]
    [SerializeField] private GameObject questPanel;

    [Header("Buttons")]
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;

    [Header("Quest Text")]
    [SerializeField] private TMP_Text questNameText;
    [SerializeField] private TMP_Text questDescriptionText;
    [SerializeField] private TMP_Text questRewardText;

    private bool hasQuest = false;

    void Awake()
    {
        // Ban dau an bang nhiem vu
        if (questPanel != null)
            questPanel.SetActive(false);
    }

    void OnEnable()
    {
        if (openButton != null)
        {
            openButton.onClick.RemoveListener(OpenQuest);
            openButton.onClick.AddListener(OpenQuest);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseQuest);
            closeButton.onClick.AddListener(CloseQuest);
        }
    }

    void OnDisable()
    {
        if (openButton != null)
            openButton.onClick.RemoveListener(OpenQuest);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseQuest);
    }

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseQuest();
        }
    }

    // NPC goi ham nay khi nhan nhiem vu
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

        OpenQuest();
    }

    // Bam icon Quest de mo bang
    public void OpenQuest()
    {
        if (questPanel == null)
        {
            Debug.LogError("Chua gan QuestPanel!");
            return;
        }

        if (!hasQuest)
        {
            Debug.Log("Chua nhan nhiem vu!");
            return;
        }

        questPanel.SetActive(true);

        Debug.Log("DA MO LAI BANG NHIEM VU");
    }

    // Bam X hoac ESC de dong
    public void CloseQuest()
    {
        if (questPanel != null)
            questPanel.SetActive(false);

        Debug.Log("DA DONG BANG NHIEM VU");
    }

    // Ho tro neu script khac goi ToggleQuest
    public void ToggleQuest()
    {
        if (questPanel == null || !hasQuest)
            return;

        if (questPanel.activeSelf)
            CloseQuest();
        else
            OpenQuest();
    }
}
