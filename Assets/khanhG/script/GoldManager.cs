
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class GoldManager : MonoBehaviour
{
    [Header("Upgrade")]
    public double upgradeCost = 10;
    public double multiplier = 1.2;

    [Header("Currency Manager")]
    public CurrencyManager currencyManager;

    [Header("UI")]
    public TMP_Text upgradeCostText;
    public Button upgradeButton;

    void Start()
    {
        UpdateUI();
    }

    public void Upgrade()
    {
        // Kiểm tra và trừ vàng từ CurrencyManager
        if (currencyManager == null)
            return;

        if (!currencyManager.SpendGold(upgradeCost))
            return;

        // Tăng tốc độ kiếm vàng x1.2
        currencyManager.goldPerSecond *= multiplier;

        // Tăng giá nâng cấp tiếp theo x1.2
        upgradeCost = Math.Ceiling(upgradeCost * multiplier);

        UpdateUI();
    }

    void Update()
    {
        if (upgradeButton != null && currencyManager != null)
        {
            upgradeButton.interactable =
                currencyManager.gold >= upgradeCost;
        }
    }

    void UpdateUI()
    {
        if (upgradeCostText != null)
        {
            upgradeCostText.text =
                Math.Ceiling(upgradeCost).ToString("N0");
        }
    }
}
