using UnityEngine;

public class Gate : MonoBehaviour
{
    public DungeonRoom dungeonRoom;
    public Vector3 insidePos;    // ô sàn trong room, sát gate (RenderDungeon gán)
    public Vector3 outsidePos;   // ô corridor, sát gate (RenderDungeon gán)
    [SerializeField] float teleportYOffset = 0.5f;   // chỉnh theo chiều cao sàn/pivot player
    [SerializeField] float hideRadius = 1f;          // sau khi teleport, ẩn gợi ý cho tới khi player đi xa chỗ vừa đáp quá ngần này

    private PlayerTest playerInRange;
    private Vector3? landPos;                        // vị trí vừa teleport tới

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Vừa teleport: điểm đáp nằm sát gate nên player vẫn ở trong vùng trigger.
        // Không hiện lại gợi ý cho tới khi player đi ra xa chỗ vừa đáp.
        if (landPos.HasValue)
        {
            Vector3 d = other.transform.position - landPos.Value; d.y = 0;
            if (d.sqrMagnitude < hideRadius * hideRadius) return;
            landPos = null;
        }

        var playerUI = other.GetComponent<PlayerUI>();
        if (playerUI == null) { Debug.LogError("Không tìm thấy PlayerUI!"); return; }
        playerUI.currentGate = this;
        playerUI.tmp.gameObject.SetActive(true);
        playerUI.UpdateText("Nhấn E để mở cửa");
        playerInRange = other.GetComponentInParent<PlayerTest>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var playerUI = other.GetComponent<PlayerUI>();
        if (playerUI != null)
        {
            if (playerUI.currentGate == this) playerUI.currentGate = null;
            // Chỉ ẩn nếu không có gate nào khác đang giữ gợi ý
            if (playerUI.currentGate == null) playerUI.tmp.gameObject.SetActive(false);
        }
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
        Vector3 land = insidePos + Vector3.up * teleportYOffset;
        playerInRange.Teleport(land);
        dungeonRoom.playerInside = true;
        DungeonManagers.instance.EnterRoom(dungeonRoom);
        ClearPlayer(land);
    }

    private void TryExit()
    {
        if (!dungeonRoom.IsCleared)
        {
            Debug.Log($"Chưa clear room {dungeonRoom.idRoom} (wave {dungeonRoom.wave}/{dungeonRoom.MaxWave})");
            return;   // chỗ này có thể hiện thông báo UI
        }

        Vector3 land = outsidePos + Vector3.up * teleportYOffset;
        playerInRange.Teleport(land);
        dungeonRoom.playerInside = false;
        DungeonManagers.instance.ExitRoom(dungeonRoom);
        ClearPlayer(land);
    }

    // Gọi ngay sau khi teleport: bỏ tham chiếu gate và ẩn gợi ý
    private void ClearPlayer(Vector3 land)
    {
        landPos = land;

        if (playerInRange != null)
        {
            var ui = playerInRange.GetComponent<PlayerUI>();
            if (ui != null)
            {
                if (ui.currentGate == this) ui.currentGate = null;
                ui.tmp.gameObject.SetActive(false);   // ẩn gợi ý ngay khi teleport
            }
        }
        playerInRange = null;
    }
}