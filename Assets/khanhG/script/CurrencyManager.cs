
using UnityEngine;
using TMPro;
using System;

public class CurrencyManager : MonoBehaviour
{
    public double gold = 100;
    public double goldPerSecond = 1;

    public TMP_Text goldText;

    void Update()
    {
        gold += goldPerSecond * Time.deltaTime;
        UpdateGoldUI();
    }

    public bool SpendGold(double amount)
    {
        if (gold < amount)
            return false;

        gold -= amount;
        UpdateGoldUI();
        return true;
    }

    void UpdateGoldUI()
    {
        if (goldText != null)
            goldText.text = Math.Floor(gold).ToString("N0");
    }
}
