
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum QuestState
{
    Available,
    Accepted,
    InProgress,
    Completed,
    Rewarded
}

[Serializable]
public class QuestProgress
{
    public string id;
    public QuestState state;
    public int kills;
}

[Serializable]
public class QuestOfferRecord
{
    public string npcId;
    public List<string> questIds = new List<string>();
}

[Serializable]
public class QuestSave
{
    public int gold;
    public List<QuestProgress> quests = new List<QuestProgress>();
    public List<QuestOfferRecord> offers = new List<QuestOfferRecord>();
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Quest khởi đầu")]
    [SerializeField] private QuestData initialMainQuest;

    [Header("Phân phối")]
    [SerializeField] private int maxQuestPerNPC = 5;

    public int Gold { get; private set; }

    public event Action OnChanged;
    public event Action<string> OnMessage;

    private readonly Dictionary<string, QuestData> catalog = new();
    private readonly Dictionary<string, QuestProgress> progress = new();

    // NPC đã đăng ký trong scene hiện tại.
    private readonly Dictionary<string, QuestGiver> givers = new();

    // Kho phân phối chung: NPC ID -> danh sách Quest ID.
    private readonly Dictionary<string, List<string>> offers = new();

    private string pendingMessage;

    private string SavePath =>
        Path.Combine(Application.persistentDataPath, "quests.json");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadCatalog();
        Load();
    }

    private void LoadCatalog()
    {
        catalog.Clear();

        QuestData[] allQuests =
            Resources.LoadAll<QuestData>("Quests");

        foreach (QuestData q in allQuests)
        {
            if (q == null || string.IsNullOrWhiteSpace(q.id))
                continue;

            if (catalog.ContainsKey(q.id))
            {
                Debug.LogError("Quest ID bị trùng: " + q.id);
                continue;
            }

            catalog.Add(q.id, q);
        }

        if (initialMainQuest != null)
        {
            catalog[initialMainQuest.id] = initialMainQuest;
        }
    }

    // =====================================================
    // ĐĂNG KÝ NPC
    // =====================================================

    public void RegisterGiver(QuestGiver giver)
    {
        if (giver == null || string.IsNullOrWhiteSpace(giver.GiverId))
            return;

        string id = giver.GiverId;

        if (givers.ContainsKey(id) && givers[id] != giver)
        {
            Debug.LogError("NPC ID bị trùng: " + id);
            return;
        }

        givers[id] = giver;

        foreach (QuestData q in giver.Quests)
        {
            if (q == null) continue;

            if (string.IsNullOrWhiteSpace(q.id))
            {
                Debug.LogError("Quest chưa có ID: " + q.name);
                continue;
            }

            catalog[q.id] = q;
        }

        // Đợi các NPC trong scene đăng ký xong rồi mới phân phối.
        StopAllCoroutines();
        StartCoroutine(RebuildOffersNextFrame());
    }

    private IEnumerator RebuildOffersNextFrame()
    {
        yield return null;

        RebuildOffers();
        Save();
        OnChanged?.Invoke();
    }

    // =====================================================
    // TRẠNG THÁI QUEST
    // =====================================================

    public QuestState GetState(QuestData q)
    {
        if (q == null) return QuestState.Available;

        return progress.TryGetValue(q.id, out QuestProgress p)
            ? p.state
            : QuestState.Available;
    }

    public int GetKills(QuestData q)
    {
        if (q == null) return 0;

        return progress.TryGetValue(q.id, out QuestProgress p)
            ? p.kills
            : 0;
    }

    public bool IsInitialMainQuestRewarded()
    {
        return initialMainQuest != null &&
               GetState(initialMainQuest) == QuestState.Rewarded;
    }

    public bool IsUnlocked(QuestData q)
    {
        if (q == null) return false;

        // Chỉ Main Quest khởi đầu được phép mở từ đầu game.
        if (q == initialMainQuest)
            return true;

        // Các Quest còn lại phải đợi Main Quest đầu tiên nhận thưởng.
        if (!IsInitialMainQuestRewarded())
            return false;

        // Không cho phép Quest không có điều kiện tự mở.
        if (q.requires == null)
            return false;

        return GetState(q.requires) == QuestState.Rewarded;
    }

    // =====================================================
    // KHO QUEST CHUNG
    // =====================================================

    
private void RebuildOffers()
{
    HashSet<string> usedQuestIds = new HashSet<string>();

    // Giữ lại những Quest chưa nhận thưởng đã được phân phối.
    foreach (var pair in offers)
    {
        pair.Value.RemoveAll(id =>
            !catalog.ContainsKey(id) ||
            GetState(catalog[id]) == QuestState.Rewarded);

        foreach (string id in pair.Value)
            usedQuestIds.Add(id);
    }

    Dictionary<string, List<QuestData>> candidates =
        new Dictionary<string, List<QuestData>>();

    List<string> npcOrder = new List<string>();

    foreach (var pair in givers)
    {
        QuestGiver giver = pair.Value;

        if (giver == null)
            continue;

        string npcId = pair.Key;

        if (!offers.ContainsKey(npcId))
            offers[npcId] = new List<string>();

        if (offers[npcId].Count >= maxQuestPerNPC)
            continue;

        List<QuestData> list = new List<QuestData>();

        foreach (QuestData q in giver.Quests)
        {
            if (q == null || string.IsNullOrWhiteSpace(q.id))
                continue;

            if (usedQuestIds.Contains(q.id))
                continue;

            if (GetState(q) != QuestState.Available)
                continue;

            if (!IsUnlocked(q))
                continue;

            list.Add(q);
        }

        // Xáo trộn Quest của NPC.
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);

            QuestData temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        candidates[npcId] = list;
        npcOrder.Add(npcId);
    }

    // Xáo trộn thứ tự NPC.
    for (int i = npcOrder.Count - 1; i > 0; i--)
    {
        int j = UnityEngine.Random.Range(0, i + 1);

        string temp = npcOrder[i];
        npcOrder[i] = npcOrder[j];
        npcOrder[j] = temp;
    }

    // Phân phối từng Quest theo vòng.
    bool added;

    do
    {
        added = false;

        foreach (string npcId in npcOrder)
        {
            List<string> npcOffers = offers[npcId];

            if (npcOffers.Count >= maxQuestPerNPC)
                continue;

            List<QuestData> list = candidates[npcId];

            if (list.Count == 0)
                continue;

            QuestData q = list[0];
            list.RemoveAt(0);

            if (usedQuestIds.Contains(q.id))
                continue;

            npcOffers.Add(q.id);
            usedQuestIds.Add(q.id);

            added = true;
        }
    }
    while (added);
}


    public List<QuestData> GetOffersFor(QuestGiver giver)
    {
        List<QuestData> result = new List<QuestData>();

        if (giver == null ||
            !offers.TryGetValue(giver.GiverId, out List<string> ids))
            return result;

        foreach (string id in ids)
        {
            if (!catalog.TryGetValue(id, out QuestData q))
                continue;

            if (GetState(q) == QuestState.Rewarded)
                continue;

            if (!IsUnlocked(q))
                continue;

            result.Add(q);
        }

        return result;
    }

    // =====================================================
    // NHẬN QUEST / NHẬN THƯỞNG
    // =====================================================

    public bool Accept(QuestData q)
    {
        if (q == null ||
            !IsUnlocked(q) ||
            GetState(q) != QuestState.Available)
            return false;

        progress[q.id] = new QuestProgress
        {
            id = q.id,
            state = QuestState.Accepted,
            kills = 0
        };

        Changed("Đã nhận nhiệm vụ: " + q.title);
        return true;
    }

    public bool Claim(QuestData q)
    {
        if (q == null || GetState(q) != QuestState.Completed)
            return false;

        progress[q.id].state = QuestState.Rewarded;
        Gold += q.goldReward;

        // Khi nhận thưởng, Quest được giải phóng khỏi NPC cũ.
        // Những NPC khác có thể nhận Quest đủ điều kiện mới.
        Changed("Hoàn thành \"" + q.title +
                "\"! +" + q.goldReward + " Gold");

        return true;
    }

    // =====================================================
    // TIẾN ĐỘ QUÁI VẬT
    // =====================================================

    public void ReportKill()
    {
        bool changed = false;
        string message = null;

        foreach (QuestProgress p in progress.Values)
        {
            bool active =
                p.state == QuestState.Accepted ||
                p.state == QuestState.InProgress;

            if (!active || !catalog.TryGetValue(p.id, out QuestData q))
                continue;

            p.kills++;

            p.state = p.kills >= q.requiredKills
                ? QuestState.Completed
                : QuestState.InProgress;

            if (p.state == QuestState.Completed)
            {
                message = "Đã hoàn thành mục tiêu \"" +
                          q.title + "\"! Hãy quay về gặp NPC.";
            }

            changed = true;
        }

        if (changed)
            Changed(message);
    }

    public void FailUnfinished()
    {
        List<string> failed = new List<string>();

        foreach (QuestProgress p in progress.Values)
        {
            if (p.state == QuestState.Accepted ||
                p.state == QuestState.InProgress)
            {
                failed.Add(p.id);
            }
        }

        if (failed.Count == 0) return;

        foreach (string id in failed)
            progress.Remove(id);

        pendingMessage =
            failed.Count + " nhiệm vụ chưa hoàn thành đã thất bại.";

        Changed();
    }

    // =====================================================
    // TRACKER
    // =====================================================

    public IEnumerable<QuestData> ActiveQuests()
    {
        foreach (QuestProgress p in progress.Values)
        {
            if (p.state != QuestState.Rewarded &&
                catalog.TryGetValue(p.id, out QuestData q))
            {
                yield return q;
            }
        }
    }

    public string TakePendingMessage()
    {
        string message = pendingMessage;
        pendingMessage = null;
        return message;
    }

    // =====================================================
    // SAVE / LOAD
    // =====================================================

    private void Changed(string message = null)
    {
        if (message != null)
            OnMessage?.Invoke(message);

        RebuildOffers();

        OnChanged?.Invoke();
        Save();
    }

    private void Save()
    {
        QuestSave data = new QuestSave
        {
            gold = Gold,
            quests = new List<QuestProgress>(progress.Values),
            offers = new List<QuestOfferRecord>()
        };

        foreach (var pair in offers)
        {
            data.offers.Add(new QuestOfferRecord
            {
                npcId = pair.Key,
                questIds = new List<string>(pair.Value)
            });
        }

        try
        {
            File.WriteAllText(
                SavePath,
                JsonUtility.ToJson(data, true));
        }
        catch (Exception e)
        {
            Debug.LogError("Lỗi lưu Quest: " + e.Message);
        }
    }

    private void Load()
    {
        if (!File.Exists(SavePath))
            return;

        try
        {
            QuestSave data =
                JsonUtility.FromJson<QuestSave>(
                    File.ReadAllText(SavePath));

            if (data == null) return;

            Gold = data.gold;

            if (data.quests != null)
            {
                foreach (QuestProgress p in data.quests)
                {
                    if (p != null && !string.IsNullOrEmpty(p.id))
                        progress[p.id] = p;
                }
            }

            if (data.offers != null)
            {
                foreach (QuestOfferRecord record in data.offers)
                {
                    if (record == null ||
                        string.IsNullOrEmpty(record.npcId))
                        continue;

                    offers[record.npcId] =
                        record.questIds != null
                            ? new List<string>(record.questIds)
                            : new List<string>();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Không đọc được save Quest: " + e.Message);
        }
    }

    [ContextMenu("Xóa save quest")]
    private void ResetSave()
    {
        progress.Clear();
        offers.Clear();
        Gold = 0;
        pendingMessage = null;

        if (File.Exists(SavePath))
            File.Delete(SavePath);

        OnChanged?.Invoke();
    }
}
