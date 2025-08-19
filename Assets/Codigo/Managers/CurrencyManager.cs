using UnityEngine;
using TMPro; // NECESSÁRIO para TextMeshPro

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    [Header("Dinheiro")]
    public int currentMoney = 500;
    public TextMeshProUGUI moneyText; // Use TextMeshProUGUI

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateMoneyUI();
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyUI();
    }

    public bool HasEnoughMoney(int amount)
    {
        return currentMoney >= amount;
    }

    public void SpendMoney(int amount)
    {
        currentMoney -= amount;
        UpdateMoneyUI();
    }

    void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = "Dinheiro: " + currentMoney;
        }
    }
}