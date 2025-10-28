using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

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
    private CanvasGroup panelCanvasGroup;
    private const int MAX_TOTAL_POINTS = 6;

    void Awake()
    {
        panelCanvasGroup = uiPanel.GetComponent<CanvasGroup>();
        if (panelCanvasGroup == null)
        {
            Debug.LogError("CanvasGroup não encontrado no uiPanel! Adicione um componente CanvasGroup.", uiPanel);
            panelCanvasGroup = uiPanel.AddComponent<CanvasGroup>();
        }
    }

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
            ResetTowerImageAlpha(); // Garante alpha no início
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

    void UpdatePanelInfo()
    {
        if (currentTower == null || panelCanvasGroup == null) return;

        int totalPointsSpent = currentTower.currentPathLevels.Sum();
        bool isFullyUpgraded = totalPointsSpent >= MAX_TOTAL_POINTS;

        // Atualiza cada caminho, passando o status de upgrade total
        UpdatePathButton(0, upgradeButton1, costText1, levelText1, isFullyUpgraded, totalPointsSpent);
        UpdatePathButton(1, upgradeButton2, costText2, levelText2, isFullyUpgraded, totalPointsSpent);
        UpdatePathButton(2, upgradeButton3, costText3, levelText3, isFullyUpgraded, totalPointsSpent);

        // Define a interatividade e alpha GLOBAIS do painel
        panelCanvasGroup.interactable = !isFullyUpgraded;
        panelCanvasGroup.alpha = isFullyUpgraded ? 0.5f : 1f;

        // Reseta o alpha da imagem da torre DEPOIS de ajustar o CanvasGroup
        ResetTowerImageAlpha();
    }

    // Método modificado para receber 'isFullyUpgraded' e 'totalPointsSpent'
    void UpdatePathButton(int pathIndex, Button button, TextMeshProUGUI costText, TextMeshProUGUI levelText, bool isFullyUpgraded, int totalPointsSpent)
    {
        UpgradeTooltip tooltip = button.GetComponent<UpgradeTooltip>();

        if (pathIndex >= currentTower.towerData.upgradePaths.Count || currentTower.towerData.upgradePaths[pathIndex] == null)
        {
            button.gameObject.SetActive(false);
            if (costText) costText.gameObject.SetActive(false);
            if (levelText) levelText.gameObject.SetActive(false);
            return;
        }
        else
        {
            button.gameObject.SetActive(true);
            if (costText) costText.gameObject.SetActive(true);
            if (levelText) levelText.gameObject.SetActive(true);
        }

        int currentLevel = currentTower.currentPathLevels[pathIndex];
        UpgradePath path = currentTower.towerData.upgradePaths[pathIndex];
        int maxLevelThisPath = path.upgradesInPath.Count;

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

        bool isLockedByChoice = false;
        string lockReason = "";

        if (pathsChosenCount >= 2 && currentLevel == 0)
        {
            isLockedByChoice = true;
            lockReason = "Só é possível escolher dois caminhos por torre.";
        }
        else if (currentLevel >= 2 && anotherPathIsTier3Plus)
        {
            isLockedByChoice = true;
            lockReason = "Outro caminho já é Nível 3+.";
        }

        levelText.text = $"Nível {currentLevel}/{maxLevelThisPath}";
        string tooltipTitleBase = path.pathName ?? "Caminho Desconhecido";
        string tooltipDescription = "";

        // Garante alpha normal para elementos individuais por padrão
        // (O CanvasGroup cuida do alpha global se isFullyUpgraded for true)
        SetElementAlpha(button.GetComponent<Image>(), 1f);
        if (costText) costText.color = new Color(costText.color.r, costText.color.g, costText.color.b, 1f);
        if (levelText) levelText.color = new Color(levelText.color.r, levelText.color.g, levelText.color.b, 1f);

        if (isLockedByChoice)
        {
            button.interactable = false;
            costText.text = "";
            levelText.text = "Bloqueado";
            tooltipDescription = lockReason;
            if (tooltip != null) tooltip.SetTooltipInfo($"{tooltipTitleBase} (BLOQUEADO)", tooltipDescription);
            // Aplica alpha baixo apenas se NÃO estiver totalmente upado (senão o CanvasGroup faz)
            if (!isFullyUpgraded)
            {
                SetElementAlpha(button.GetComponent<Image>(), 0.5f);
                if (costText) costText.color = new Color(costText.color.r, costText.color.g, costText.color.b, 0.5f);
                if (levelText) levelText.color = new Color(levelText.color.r, levelText.color.g, levelText.color.b, 0.5f);
            }
            return;
        }

        bool isMaxLevelForPath = currentLevel >= maxLevelThisPath;

        if (!isMaxLevelForPath) // Se AINDA não atingiu o nível máximo DESTE caminho
        {
            Upgrade nextUpgrade = path.upgradesInPath[currentLevel];
            int geoditeCost = nextUpgrade.geoditeCost;
            int darkEtherCost = nextUpgrade.darkEtherCost;

            string costString = "";
            List<string> costs = new List<string>();
            if (geoditeCost > 0) costs.Add($"<color=#76D7C4>{geoditeCost}G</color>");
            if (darkEtherCost > 0) costs.Add($"<color=#C39BD3>{darkEtherCost}E</color>");
            costString = costs.Count > 0 ? string.Join(" / ", costs) : "Grátis";


            bool canAfford = CurrencyManager.Instance.HasEnoughCurrency(geoditeCost, CurrencyType.Geodites) &&
                             CurrencyManager.Instance.HasEnoughCurrency(darkEtherCost, CurrencyType.DarkEther);

            // --- CORREÇÃO INTERATIVIDADE e CUSTO no 6º PONTO ---
            // O botão SÓ é interativo se puder pagar E se a torre NÃO estiver totalmente upada
            button.interactable = canAfford && !isFullyUpgraded;
            // Mostra o custo APENAS se a torre NÃO estiver totalmente upada
            costText.text = !isFullyUpgraded ? costString : "";
            // --- FIM DA CORREÇÃO ---


            tooltipDescription = $"<b>Próximo Nível ({currentLevel + 1}): {nextUpgrade.upgradeName}</b>\n{nextUpgrade.description}";
            if (tooltip != null) tooltip.SetTooltipInfo(tooltipTitleBase, tooltipDescription);

        }
        else // Nível Máximo neste caminho
        {
            button.interactable = false; // Botão sempre inativo no max
            costText.text = ""; // Custo sempre vazio no max
            levelText.text = "Nível MAX";
            tooltipDescription = "Este caminho já está no nível máximo.";
            if (tooltip != null) tooltip.SetTooltipInfo($"{tooltipTitleBase} (NÍVEL MÁXIMO)", tooltipDescription);
        }
    }

    void SetElementAlpha(Graphic element, float alpha)
    {
        if (element != null)
        {
            Color color = element.color;
            color.a = alpha;
            element.color = color;
        }
    }

    // Nova função para resetar o alpha da imagem da torre
    void ResetTowerImageAlpha()
    {
        if (towerImage != null)
        {
            Color imgColor = towerImage.color;
            imgColor.a = 1f; // Força alpha 1 (opaco)
            towerImage.color = imgColor;
        }
    }


    public void UpgradePath(int pathIndex)
    {
        if (currentTower == null) return;

        if (pathIndex >= currentTower.towerData.upgradePaths.Count || currentTower.towerData.upgradePaths[pathIndex] == null) return;
        UpgradePath path = currentTower.towerData.upgradePaths[pathIndex];
        int currentLevel = currentTower.currentPathLevels[pathIndex];
        if (currentLevel >= path.upgradesInPath.Count) return;

        int totalPointsSpent = currentTower.currentPathLevels.Sum();
        if (totalPointsSpent >= MAX_TOTAL_POINTS)
        {
            Debug.Log("Limite total de upgrades (6) atingido para esta torre!");
            return; // Já retorna aqui se o limite foi atingido
        }

        Upgrade nextUpgrade = path.upgradesInPath[currentLevel];
        int geoditeCost = nextUpgrade.geoditeCost;
        int darkEtherCost = nextUpgrade.darkEtherCost;

        if (CurrencyManager.Instance.HasEnoughCurrency(geoditeCost, CurrencyType.Geodites) &&
            CurrencyManager.Instance.HasEnoughCurrency(darkEtherCost, CurrencyType.DarkEther))
        {
            CurrencyManager.Instance.SpendCurrency(geoditeCost, CurrencyType.Geodites);
            CurrencyManager.Instance.SpendCurrency(darkEtherCost, CurrencyType.DarkEther);

            currentTower.ApplyUpgrade(nextUpgrade);
            currentTower.currentPathLevels[pathIndex]++;

            // A Corrotina já cuida da atualização da UI
            StartCoroutine(RefreshUIAfterFrame());
        }
        else
        {
            Debug.Log("Recursos insuficientes para este upgrade!");
        }
    }

    private IEnumerator RefreshUIAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        // Verifica se a torre ainda está selecionada ANTES de atualizar
        if (currentTower != null && uiPanel.activeInHierarchy)
        {
            UpdatePanelInfo(); // Reexecuta toda a lógica de atualização da UI
            if (uiPanel != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(uiPanel.GetComponent<RectTransform>());
            }
        }
    }
}