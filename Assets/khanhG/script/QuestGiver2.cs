
using UnityEngine;
using UnityEngine.InputSystem;

public class QuestGiver2 : MonoBehaviour
{
    [Header("UI Tuong tac")]
    [SerializeField] private GameObject interactUI;

    [Header("UI Nang cap vu khi")]
    [SerializeField] private GameObject upgradeUI;

    private bool playerNear = false;
    private bool isUIOpen = false;

    void Start()
    {
        // Tat UI khi bat dau game
        if (interactUI != null)
            interactUI.SetActive(false);

        if (upgradeUI != null)
            upgradeUI.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        if (playerNear && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleUpgradeUI();
        }
    }

    // Mo / Tat UI nang cap
    void ToggleUpgradeUI()
    {
        if (upgradeUI == null)
            return;

        isUIOpen = !isUIOpen;

        upgradeUI.SetActive(isUIOpen);

        // An hien thong bao nhan E
        if (interactUI != null)
            interactUI.SetActive(!isUIOpen);

        Debug.Log(isUIOpen
            ? "Mo bang nang cap vu khi"
            : "Dong bang nang cap vu khi");
    }

    // Khi nguoi choi di vao vung Collider
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;

            if (!isUIOpen && interactUI != null)
                interactUI.SetActive(true);
        }
    }

    // Khi nguoi choi roi khoi vung Collider
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            isUIOpen = false;

            if (interactUI != null)
                interactUI.SetActive(false);

            if (upgradeUI != null)
                upgradeUI.SetActive(false);
        }
    }
}
