
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public GameObject settingPanel;

    void Start()
    {
        // Ẩn bảng Setting khi bắt đầu game
        settingPanel.SetActive(false);
    }

    // Hiện bảng Setting
    public void OpenSettings()
    {
        settingPanel.SetActive(true);
    }

    // Đóng bảng Setting
    public void CloseSettings()
    {
        settingPanel.SetActive(false);
    }
}
