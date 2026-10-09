
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class QuestGiver : MonoBehaviour
{
    [Header("Thong tin nhiem vu")]
    [SerializeField] private string questName = "Hoan thanh Dungeon 1";

    [TextArea(3, 6)]
    [SerializeField]
    private string questDescription =
        "Hay di vao Dungeon 1 va tieu diet quai vat.";

    [SerializeField] private string questReward = "100 Gold + 50 EXP";

    [Header("Quest UI")]
    [SerializeField] private GameObject interactUI;
    [SerializeField] private GameObject questUI;

    [SerializeField] private TMP_Text questNameText;
    [SerializeField] private TMP_Text questDescriptionText;
    [SerializeField] private TMP_Text questRewardText;

    private bool playerNear;
    private bool questAccepted;
    private bool questUIOpen;

    void Start()
    {
        if (interactUI != null)
            interactUI.SetActive(false);

        if (questUI != null)
            questUI.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        if (playerNear &&
            !questAccepted &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            AcceptQuest();
        }

        if (questUIOpen &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseQuestUI();
        }
    }

    void AcceptQuest()
    {
        questAccepted = true;

        if (interactUI != null)
            interactUI.SetActive(false);

        if (questNameText != null)
            questNameText.text = questName;

        if (questDescriptionText != null)
            questDescriptionText.text = questDescription;

        if (questRewardText != null)
            questRewardText.text = "Phan thuong: " + questReward;

        if (questUI != null)
            questUI.SetActive(true);

        questUIOpen = true;

        Debug.Log("Da nhan nhiem vu: " + questName);
    }

    public void CloseQuestUI()
    {
        if (questUI != null)
            questUI.SetActive(false);

        questUIOpen = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;

            if (!questAccepted && interactUI != null)
                interactUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;

            if (interactUI != null)
                interactUI.SetActive(false);
        }
    }
}
