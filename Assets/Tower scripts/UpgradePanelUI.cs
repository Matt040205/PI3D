// UpgradePanelUI.cs
using UnityEngine;
using UnityEngine.UI; // Essencial para manipular UI

public class UpgradePanelUI : MonoBehaviour
{
    [Header("Referências Gerais")]
    public GameObject uiPanel; // O objeto do painel em si

    [Header("Referências do Caminho 1")]
    public Button upgradeButton1;
    public Text nameText1;
    public Text descriptionText1;
    public Text costText1;

    [Header("Referências do Caminho 2")]
    public Button upgradeButton2;
    public Text nameText2;
    public Text descriptionText2;
    public Text costText2;

    [Header("Referências do Caminho 3")]
    public Button upgradeButton3;
    public Text nameText3;
    public Text descriptionText3;
    public Text costText3;

    private TowerController currentTower;

    void Start()
    {
        // Garante que o painel comece escondido
        HidePanel();
    }

    // Chamado pelo TowerSelectionManager quando uma torre é selecionada
    public void ShowPanel(TowerController tower)
    {
        currentTower = tower;
        uiPanel.SetActive(true);
        UpdatePanelInfo();
    }

    // Chamado para esconder o painel
    public void HidePanel()
    {
        currentTower = null;
        uiPanel.SetActive(false);
    }
    public bool IsPanelVisible()
    {
        // Retorna true se o painel estiver ativo na cena, false caso contrário.
        return uiPanel.activeSelf;
    }

    // Atualiza as informações nos 3 botões
    void UpdatePanelInfo()
    {
        if (currentTower == null) return;

        // Atualiza o Botão 1
        UpdatePathButton(0, upgradeButton1, nameText1, descriptionText1, costText1);
        // Atualiza o Botão 2
        UpdatePathButton(1, upgradeButton2, nameText2, descriptionText2, costText2);
        // Atualiza o Botão 3
        UpdatePathButton(2, upgradeButton3, nameText3, descriptionText3, costText3);
    }

    // Lógica genérica para atualizar um botão de caminho
    void UpdatePathButton(int pathIndex, Button button, Text nameText, Text descText, Text costText)
    {
        int currentLevel = currentTower.currentPathLevels[pathIndex];
        UpgradePath path = currentTower.towerData.upgradePaths[pathIndex];

        if (currentLevel < path.upgradesInPath.Count)
        {
            Upgrade nextUpgrade = path.upgradesInPath[currentLevel];
            button.interactable = true;
            nameText.text = nextUpgrade.upgradeName;
            descText.text = nextUpgrade.description;
            costText.text = $"Custo: {nextUpgrade.geoditeCost}"; // Exemplo de custo
        }
        else
        {
            // Caminho no nível máximo
            button.interactable = false;
            nameText.text = "MAX";
            descText.text = "Este caminho está no nível máximo.";
            costText.text = "";
        }
    }

    // --- MÉTODOS PARA SEREM CHAMADOS PELOS BOTÕES ---

    public void UpgradePath(int pathIndex)
    {
        if (currentTower == null) return;

        int currentLevel = currentTower.currentPathLevels[pathIndex];
        UpgradePath path = currentTower.towerData.upgradePaths[pathIndex];

        if (currentLevel < path.upgradesInPath.Count)
        {
            Upgrade nextUpgrade = path.upgradesInPath[currentLevel];

            // Futuramente, verificar se o jogador tem dinheiro
            // if (CurrencyManager.Instance.CanAfford(nextUpgrade.geoditeCost)) ...

            currentTower.ApplyUpgrade(nextUpgrade);
            currentTower.currentPathLevels[pathIndex]++;

            // Atualiza o painel para mostrar o próximo upgrade ou o estado de MAX
            UpdatePanelInfo();
        }
    }
}