
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class GoldManager : MonoBehaviour
{
    public enum UpgradeType
    {
        Atk,
        Def,
        Hp
    }

    [Header("Upgrade")]
    public double upgradeCost = 10;
    public double multiplier = 1.2;

    public UpgradeType upgradeType;
    public int upgradeAmount = 1;

    [Header("Player")]
    public PlayerTest player;

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
        if (currencyManager == null || player == null)
            return;

        if (!currencyManager.SpendGold(upgradeCost))
            return;

        // Cộng chỉ số trực tiếp vào Player
        switch (upgradeType)
        {
            case UpgradeType.Atk:
                player.UpgradeAtk(upgradeAmount);
                break;

            case UpgradeType.Def:
                player.UpgradeDef(upgradeAmount);
                break;

            case UpgradeType.Hp:
                player.UpgradeHp(upgradeAmount);
                break;
        }

        // Tăng giá nâng cấp tiếp theo
        upgradeCost = Math.Ceiling(upgradeCost * multiplier);

        UpdateUI();

        Debug.Log("Upgrade thành công: " + upgradeType);
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
