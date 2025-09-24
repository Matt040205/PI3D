// UpgradePanelUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradePanelUI : MonoBehaviour
{
    [Header("Referências Gerais")]
    public GameObject uiPanel;

    [Header("Referências do Caminho 1")]
    public Button upgradeButton1;
    public Text nameText1;
    public Text descriptionText1;
    public TextMeshProUGUI costText1;

    [Header("Referências do Caminho 2")]
    public Button upgradeButton2;
    public Text nameText2;
    public Text descriptionText2;
    public TextMeshProUGUI costText2;

    [Header("Referências do Caminho 3")]
    public Button upgradeButton3;
    public Text nameText3;
    public Text descriptionText3;
    public TextMeshProUGUI costText3;

    private TowerController currentTower;
    private const int BASE_COST = 50;

    void Start()
    {
        HidePanel();
    }

    public void ShowPanel(TowerController tower)
    {
        currentTower = tower;
        uiPanel.SetActive(true);
        UpdatePanelInfo();
    }

    public void HidePanel()
    {
        currentTower = null;
        uiPanel.SetActive(false);
    }
    public bool IsPanelVisible()
    {
        return uiPanel.activeSelf;
    }

    void UpdatePanelInfo()
    {
        if (currentTower == null) return;

        bool isAnyPathChosen = false;
        int chosenPathIndex = -1;

        // Verifica se algum caminho já foi escolhido
        for (int i = 0; i < currentTower.currentPathLevels.Length; i++)
        {
            if (currentTower.currentPathLevels[i] > 0)
            {
                isAnyPathChosen = true;
                chosenPathIndex = i;
                break;
            }
        }

        UpdatePathButton(0, upgradeButton1, nameText1, descriptionText1, costText1, isAnyPathChosen, chosenPathIndex);
        UpdatePathButton(1, upgradeButton2, nameText2, descriptionText2, costText2, isAnyPathChosen, chosenPathIndex);
        UpdatePathButton(2, upgradeButton3, nameText3, descriptionText3, costText3, isAnyPathChosen, chosenPathIndex);
    }

    void UpdatePathButton(int pathIndex, Button button, Text nameText, Text descText, TextMeshProUGUI costText, bool isAnyPathChosen, int chosenPathIndex)
    {
        if (isAnyPathChosen && pathIndex != chosenPathIndex)
        {
            button.interactable = false;
            nameText.text = "BLOQUEADO";
            descText.text = "Escolha de caminho já feita.";
            costText.text = "";
            return;
        }

        int currentLevel = currentTower.currentPathLevels[pathIndex];
        UpgradePath path = currentTower.towerData.upgradePaths[pathIndex];

        if (currentLevel < path.upgradesInPath.Count)
        {
            Upgrade nextUpgrade = path.upgradesInPath[currentLevel];
            int upgradeCost = Mathf.FloorToInt(BASE_COST * Mathf.Pow(1.5f, currentLevel));
            bool canAfford = CurrencyManager.Instance.HasEnoughCurrency(upgradeCost, CurrencyType.Geodites);

            button.interactable = canAfford;
            nameText.text = nextUpgrade.upgradeName;
            descText.text = nextUpgrade.description;
            costText.text = $"Custo: {upgradeCost} Geoditas";
            costText.color = canAfford ? Color.green : Color.red;
        }
        else
        {
            button.interactable = false;
            nameText.text = "MAX";
            descText.text = "Este caminho está no nível máximo.";
            costText.text = "";
        }
    }

    public void UpgradePath(int pathIndex)
    {
        if (currentTower == null) return;

        // Impede upgrades se o jogador já escolheu outro caminho
        for (int i = 0; i < currentTower.currentPathLevels.Length; i++)
        {
            if (currentTower.currentPathLevels[i] > 0 && i != pathIndex)
            {
                Debug.Log("Você já escolheu um caminho de upgrade para esta torre!");
                return;
            }
        }

        int currentLevel = currentTower.currentPathLevels[pathIndex];
        UpgradePath path = currentTower.towerData.upgradePaths[pathIndex];

        if (currentLevel < path.upgradesInPath.Count)
        {
            Upgrade nextUpgrade = path.upgradesInPath[currentLevel];
            int upgradeCost = Mathf.FloorToInt(BASE_COST * Mathf.Pow(1.5f, currentLevel));

            if (CurrencyManager.Instance.HasEnoughCurrency(upgradeCost, CurrencyType.Geodites))
            {
                CurrencyManager.Instance.SpendCurrency(upgradeCost, CurrencyType.Geodites);
                currentTower.ApplyUpgrade(nextUpgrade);
                currentTower.currentPathLevels[pathIndex]++;
                UpdatePanelInfo();
            }
            else
            {
                Debug.Log("Você não tem Geoditas suficientes para este upgrade!");
            }
        }
    }
}