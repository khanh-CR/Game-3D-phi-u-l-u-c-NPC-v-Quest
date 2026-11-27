using UnityEngine;

public enum QuestType { Main, Side }

// Mỗi quest là 1 asset: Create > Quest > Quest Data. Đặt trong Assets/Resources/Quests/
[CreateAssetMenu(menuName = "Quest/Quest Data")]
public class QuestData : ScriptableObject
{
    public string id;                        // duy nhất, dùng để lưu save (tự lấy tên asset nếu để trống)
    public string title = "Dọn dẹp Dungeon";
    [TextArea(2, 5)] public string description = "Hãy vào Dungeon và tiêu diệt quái vật.";
    public QuestType type = QuestType.Side;

    public int requiredKills = 10;           // số quái cần tiêu diệt
    public int goldReward = 100;

    [Tooltip("Quest phải nhận thưởng xong thì quest này mới mở. Để trống = mở ngay")]
    public QuestData requires;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id)) id = name;
    }
}
