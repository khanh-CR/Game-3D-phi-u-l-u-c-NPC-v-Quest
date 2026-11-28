using UnityEngine;

public class Gate : MonoBehaviour
{
    public DungeonRoom dungeonRoom;
    public Vector3 insidePos;    // ô sàn trong room, sát gate (RenderDungeon gán)
    public Vector3 outsidePos;   // ô corridor, sát gate (RenderDungeon gán)
    [SerializeField] float teleportYOffset = 0.5f;   // chỉnh theo chiều cao sàn/pivot player

    private PlayerTest playerInRange;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var playerUI = other.GetComponent<PlayerUI>();
        if (playerUI == null) { Debug.LogError("Không tìm thấy PlayerUI!"); return; }
        playerUI.currentGate = this;
        playerInRange = other.GetComponentInParent<PlayerTest>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var playerUI = other.GetComponent<PlayerUI>();
        if (playerUI != null && playerUI.currentGate == this) playerUI.currentGate = null;
        playerInRange = null;
    }

    // PlayerUI vẫn gọi hàm này khi bấm phím tương tác
    public void OpenTheGate()
    {
        if (playerInRange == null || dungeonRoom == null) return;

        if (IsInside(playerInRange.transform.position)) TryExit();
        else Enter();
    }

    private bool IsInside(Vector3 p)
    {
        Vector3 outDir = outsidePos - insidePos; outDir.y = 0;
        Vector3 d = p - (insidePos + outsidePos) * 0.5f; d.y = 0;   // tâm gate
        return Vector3.Dot(d, outDir) < 0;
    }

    private void Enter()
    {
        playerInRange.Teleport(insidePos + Vector3.up * teleportYOffset);
        dungeonRoom.playerInside = true;
        DungeonManagers.instance.EnterRoom(dungeonRoom);
        ClearPlayer();
    }

    private void TryExit()
    {
        if (!dungeonRoom.IsCleared)
        {
            Debug.Log($"Chưa clear room {dungeonRoom.idRoom} (wave {dungeonRoom.wave}/{dungeonRoom.MaxWave})");
            return;   // chỗ này có thể hiện thông báo UI
        }
        playerInRange.Teleport(outsidePos + Vector3.up * teleportYOffset);
        dungeonRoom.playerInside = false;
        DungeonManagers.instance.ExitRoom(dungeonRoom);
        ClearPlayer();
    }
    private void ClearPlayer()
    {
        if (playerInRange != null)
        {
            var ui = playerInRange.GetComponent<PlayerUI>();
            if (ui != null && ui.currentGate == this) ui.currentGate = null;
        }
        playerInRange = null;
    }
}