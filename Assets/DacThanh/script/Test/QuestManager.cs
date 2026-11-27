using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Available (chưa nhận) -> Accepted (đã nhận) -> InProgress (đang làm) -> Completed (xong mục tiêu) -> Rewarded (đã nhận thưởng)
public enum QuestState { Available, Accepted, InProgress, Completed, Rewarded }

[Serializable]
public class QuestProgress
{
    public string id;
    public QuestState state;
    public int kills;
}

[Serializable]
class QuestSave
{
    public int gold;
    public List<QuestProgress> quests = new List<QuestProgress>();
}

// Đặt ở scene Lobby, trên GameObject GỐC (không phải con của object khác).
// Tồn tại xuyên scene nên không bị mất khi DungeonManagers xóa player và load lại lobby.
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public int Gold { get; private set; }
    public event Action OnChanged;            // quest/gold đổi -> UI vẽ lại
    public event Action<string> OnMessage;    // thông báo ngắn cho UI

    private readonly Dictionary<string, QuestData> catalog = new Dictionary<string, QuestData>();
    private readonly Dictionary<string, QuestProgress> progress = new Dictionary<string, QuestProgress>();
    private string pendingMessage;            // thông báo chờ hiện ở scene kế tiếp (vd: quest thất bại)

    private string SavePath => Path.Combine(Application.persistentDataPath, "quests.json");

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }   // lobby load lại -> bỏ bản trùng
        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (QuestData q in Resources.LoadAll<QuestData>("Quests"))
            catalog[q.id] = q;

        Load();
    }

    #region Tra cứu
    public QuestState GetState(QuestData q) => progress.TryGetValue(q.id, out QuestProgress p) ? p.state : QuestState.Available;
    public int GetKills(QuestData q) => progress.TryGetValue(q.id, out QuestProgress p) ? p.kills : 0;

    // Quest đã mở khóa chưa (quest tiên quyết đã nhận thưởng)
    public bool IsUnlocked(QuestData q) => q.requires == null || GetState(q.requires) == QuestState.Rewarded;

    // Các quest đang nhận / đang làm / chờ nhận thưởng (dùng cho bảng theo dõi nhiệm vụ)
    public IEnumerable<QuestData> ActiveQuests()
    {
        foreach (QuestProgress p in progress.Values)
            if (p.state != QuestState.Rewarded && catalog.TryGetValue(p.id, out QuestData q))
                yield return q;
    }

    public string TakePendingMessage()
    {
        string m = pendingMessage;
        pendingMessage = null;
        return m;
    }
    #endregion

    #region Hành động
    public bool Accept(QuestData q)
    {
        if (!IsUnlocked(q) || GetState(q) != QuestState.Available) return false;

        progress[q.id] = new QuestProgress { id = q.id, state = QuestState.Accepted };
        Changed($"Đã nhận nhiệm vụ: {q.title}");
        return true;
    }

    // Về gặp NPC: nhận thưởng gold
    public bool Claim(QuestData q)
    {
        if (GetState(q) != QuestState.Completed) return false;

        progress[q.id].state = QuestState.Rewarded;
        Gold += q.goldReward;
        Changed($"Hoàn thành \"{q.title}\"! +{q.goldReward} Gold");
        return true;
    }

    // EnemyAI gọi mỗi khi 1 quái chết
    public void ReportKill()
    {
        bool changed = false;
        string message = null;

        foreach (QuestProgress p in progress.Values)
        {
            bool active = p.state == QuestState.Accepted || p.state == QuestState.InProgress;
            if (!active || !catalog.TryGetValue(p.id, out QuestData q)) continue;

            p.kills++;
            p.state = p.kills >= q.requiredKills ? QuestState.Completed : QuestState.InProgress;
            if (p.state == QuestState.Completed)
                message = $"Đã xong mục tiêu \"{q.title}\"! Hãy quay về gặp NPC.";
            changed = true;
        }

        if (changed) Changed(message);
    }

    // Rời dungeon mà chưa xong mục tiêu: quest thất bại, phải nhận lại. Quest đã Completed thì giữ nguyên.
    public void FailUnfinished()
    {
        List<string> failed = new List<string>();
        foreach (QuestProgress p in progress.Values)
            if (p.state == QuestState.Accepted || p.state == QuestState.InProgress)
                failed.Add(p.id);

        if (failed.Count == 0) return;

        foreach (string id in failed) progress.Remove(id);   // xóa tiến trình = quay về Available
        pendingMessage = $"{failed.Count} nhiệm vụ chưa hoàn thành nên đã thất bại. Hãy gặp NPC để nhận lại.";
        Changed();
    }

    private void Changed(string message = null)
    {
        if (message != null) OnMessage?.Invoke(message);
        OnChanged?.Invoke();
        Save();
    }
    #endregion

    #region Save / Load (JSON trong persistentDataPath)
    private void Save()
    {
        QuestSave data = new QuestSave { gold = Gold, quests = new List<QuestProgress>(progress.Values) };
        File.WriteAllText(SavePath, JsonUtility.ToJson(data));
    }

    private void Load()
    {
        if (!File.Exists(SavePath)) return;

        try
        {
            QuestSave data = JsonUtility.FromJson<QuestSave>(File.ReadAllText(SavePath));
            Gold = data.gold;
            foreach (QuestProgress p in data.quests) progress[p.id] = p;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Không đọc được file save quest: " + e.Message);
        }
    }

    [ContextMenu("Xóa save quest")]
    private void ResetSave()
    {
        progress.Clear();
        Gold = 0;
        if (File.Exists(SavePath)) File.Delete(SavePath);
        OnChanged?.Invoke();
    }
    #endregion
}
