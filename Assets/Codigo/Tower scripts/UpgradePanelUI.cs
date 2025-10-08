// UpgradePanelUI.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections; // PRECISAMOS DESTA LINHA PARA USAR CORROTINAS
using System.Linq;

public class UpgradePanelUI : MonoBehaviour
{
    [Header("Referências Gerais")]
    public GameObject uiPanel;
    public Image towerImage;

    [Header("Referências do Caminho 1")]
    public Button upgradeButton1;
    public TextMeshProUGUI costText1;
    public TextMeshProUGUI levelText1;

    [Header("Referências do Caminho 2")]
    public Button upgradeButton2;
    public TextMeshProUGUI costText2;
    public TextMeshProUGUI levelText2;

    [Header("Referências do Caminho 3")]
    public Button upgradeButton3;
    public TextMeshProUGUI costText3;
    public TextMeshProUGUI levelText3;

    private TowerController currentTower;
    private const int BASE_COST = 50;

    // --- Nenhuma mudança de Start() até abaixo de IsPanelVisible() ---

    void Start()
    {
        HidePanel();
    }

    public void ShowPanel(TowerController tower)
    {
        currentTower = tower;
        uiPanel.SetActive(true);

        if (towerImage != null && currentTower.towerData.characterIcon != null)
        {
            towerImage.sprite = currentTower.towerData.characterIcon;
        }

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

    // --- A função UpdatePanelInfo() e UpdatePathButton() permanecem iguais à versão anterior ---

    void UpdatePanelInfo()
    {
        if (currentTower == null) return;

        UpdatePathButton(0, upgradeButton1, costText1, levelText1);
        UpdatePathButton(1, upgradeButton2, costText2, levelText2);
        UpdatePathButton(2, upgradeButton3, costText3, levelText3);
    }

    void UpdatePathButton(int pathIndex, Button button, TextMeshProUGUI costText, TextMeshProUGUI levelText)
    {
        UpgradeTooltip tooltip = button.GetComponent<UpgradeTooltip>();
        int currentLevel = currentTower.currentPathLevels[pathIndex];
        UpgradePath path = currentTower.towerData.upgradePaths[pathIndex];

        int pathsChosenCount = currentTower.currentPathLevels.Count(level => level > 0);

        bool anotherPathIsTier3Plus = false;
        for (int i = 0; i < currentTower.currentPathLevels.Length; i++)
        {
            if (i != pathIndex && currentTower.currentPathLevels[i] > 2)
            {
                anotherPathIsTier3Plus = true;
                break;
            }
        }

        bool isLocked = false;
        string lockReason = "";

        if (pathsChosenCount >= 2 && currentTower.currentPathLevels[pathIndex] == 0)
        {
            isLocked = true;
            lockReason = "Você só pode escolher dois caminhos de upgrade por torre.";
        }
        else if (currentLevel >= 2 && anotherPathIsTier3Plus)
        {
            isLocked = true;
            lockReason = "Outro caminho já foi escolhido como principal (Nível 3+).";
        }

        if (isLocked)
        {
            button.interactable = false;
            costText.text = "";
            levelText.text = "Bloqueado";
            if (tooltip != null) tooltip.SetTooltipInfo("BLOQUEADO", lockReason);
            return;
        }

        levelText.text = $"Nível {currentLevel}/{path.upgradesInPath.Count}";

        if (currentLevel < path.upgradesInPath.Count)
        {
            Upgrade nextUpgrade = path.upgradesInPath[currentLevel];
            int upgradeCost = Mathf.FloorToInt(BASE_COST * Mathf.Pow(1.5f, currentLevel));
            bool canAfford = CurrencyManager.Instance.HasEnoughCurrency(upgradeCost, CurrencyType.Geodites);

            button.interactable = canAfford;
            costText.text = $"{upgradeCost} Geoditas";
            costText.color = canAfford ? Color.green : Color.red;

            if (tooltip != null) tooltip.SetTooltipInfo(nextUpgrade.upgradeName, nextUpgrade.description);
        }
        else
        {
            button.interactable = false;
            costText.text = "";
            levelText.text = "Nível MAX";
            if (tooltip != null) tooltip.SetTooltipInfo("NÍVEL MÁXIMO", "Este caminho de upgrade já está no máximo.");
        }
    }

    // --- MUDANÇA PRINCIPAL AQUI ---
    public void UpgradePath(int pathIndex)
    {
        if (currentTower == null) return;

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

                // Em vez de chamar UpdatePanelInfo() diretamente, iniciamos a corrotina.
                StartCoroutine(RefreshUIAfterFrame());
            }
            else
            {
                Debug.Log("Você não tem Geoditas suficientes para este upgrade!");
            }
        }
    }

    // --- NOVA FUNÇÃO DE CORROTINA ---
    private IEnumerator RefreshUIAfterFrame()
    {
        // 1. Espera até o final do frame atual. Neste ponto, todas as lógicas do jogo já rodaram.
        yield return new WaitForEndOfFrame();

        // 2. Agora, com os dados 100% atualizados, mandamos a UI recalcular tudo.
        UpdatePanelInfo();

        // 3. (Opcional, mas muito poderoso) Força o Canvas a reconstruir o layout dos elementos filhos deste painel.
        // Isso resolve problemas com Layout Groups que não se atualizam.
        // O "uiPanel" deve ter um componente RectTransform, que é o padrão.
        if (uiPanel != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(uiPanel.GetComponent<RectTransform>());
        }
    }
}