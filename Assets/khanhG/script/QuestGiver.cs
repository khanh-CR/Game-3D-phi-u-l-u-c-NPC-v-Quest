
using UnityEngine;
using UnityEngine.InputSystem;

public class QuestGiver : MonoBehaviour
{
    [Header("Thong tin nhiem vu")]
    [SerializeField]
    private string questName = "Hoan thanh Dungeon 1";

    [TextArea(3, 6)]
    [SerializeField]
    private string questDescription =
        "Hay di vao Dungeon 1 va tieu diet quai vat.";

    [SerializeField]
    private string questReward = "100 Gold + 50 EXP";

    [Header("NPC Interaction")]
    [SerializeField] private GameObject interactUI;

    [Header("Quest UI Manager")]
    [SerializeField] private QuestUIManager questUIManager;

    private bool playerNear = false;
    private bool questAccepted = false;

    void Start()
    {
        if (interactUI != null)
            interactUI.SetActive(false);
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
    }

    void AcceptQuest()
    {
        if (questAccepted || questUIManager == null)
            return;

        questAccepted = true;

        if (interactUI != null)
            interactUI.SetActive(false);

        questUIManager.SetQuest(
            questName,
            questDescription,
            questReward
        );

        Debug.Log("Da nhan nhiem vu: " + questName);
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
