// SelecaoManager.cs (Com botão Rastros)
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelecaoManager : MonoBehaviour
{
    [Header("Configuração dos Personagens")]
    public List<CharacterBase> todosOsPersonagens;

    [Header("Painéis Gerais da UI")]
    public GameObject painelEquipe;
    public GameObject painelEscolhaPersonagem;
    public GameObject painelDetalhes;

    [Header("Elementos da UI - Equipe")]
    public GameObject slotEquipePrefab;
    public Transform gridEquipeContainer;
    public Button botaoJogar;
    public string nomeDaCenaDoJogo;

    [Header("Elementos da UI - Escolha")]
    public GameObject slotEscolhaPrefab;
    public Transform gridEscolhaContainer;
    public Button botaoVoltarDaEscolha;

    [Header("Elementos da UI - Detalhes Estáticos")]
    public Image imagemDetalhes;
    public TextMeshProUGUI nomeDetalhes;
    public TextMeshProUGUI textoStatusPadrao;
    public Button botaoConfirmarEscolha;
    public Button botaoVoltarDosDetalhes;

    [Header("Elementos da UI - Detalhes Interativos")]
    public GameObject painelHabilidades;
    public GameObject painelUpgradesTorre;
    public TextMeshProUGUI textoHabilidadesComandante;

    [Header("Textos dos Upgrades da Torre")]
    public TextMeshProUGUI textoCaminho1;
    public TextMeshProUGUI textoCaminho2;
    public TextMeshProUGUI textoCaminho3;

    [Header("Botões de Navegação dos Detalhes")]
    public Button botaoAbaHabilidades;
    public Button botaoAbaTorre;
    public List<Button> botoesCaminhoTorre; // Lista para os 3 botões dos caminhos

    // --- NOVAS VARIÁVEIS PARA OS RASTROS ---
    [Header("Elementos da UI - Rastros")]
    public Button botaoRastros; // Arraste seu novo botão "Rastros" aqui
    public string nomeDaCenaRastros = "Rastros"; // Nome da cena da árvore
    // ------------------------------------

    private List<Button> slotsEquipe = new List<Button>();
    private Dictionary<CharacterBase, Button> botoesDeEscolha = new Dictionary<CharacterBase, Button>();
    private int slotSendoEditado = -1;
    private CharacterBase personagemEmVisualizacao;

    void Start() { StartCoroutine(SetupScene()); }

    IEnumerator SetupScene()
    {
        // ... (código do SetupScene continua igual)
        LimparGrid(gridEquipeContainer);
        LimparGrid(gridEscolhaContainer);
        slotsEquipe.Clear();
        botoesDeEscolha.Clear();
        painelEquipe.SetActive(false);
        painelEscolhaPersonagem.SetActive(false);
        painelDetalhes.SetActive(false);
        yield return new WaitForEndOfFrame();
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.LimparSelecao();
            GameDataManager.Instance.personagemParaRastros = null; // Limpa ao iniciar
        }
        ConfigurarBotoesPrincipais();
        CriarGridEquipe();
        PopularGridDeEscolha();
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridEquipeContainer.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridEscolhaContainer.GetComponent<RectTransform>());
        painelEquipe.SetActive(true);
    }

    public void AbrirPainelDetalhes(CharacterBase personagem)
    {
        painelEscolhaPersonagem.SetActive(false);
        painelDetalhes.SetActive(true);
        personagemEmVisualizacao = personagem;

        imagemDetalhes.sprite = personagem.characterIcon;
        nomeDetalhes.text = personagem.name;

        textoStatusPadrao.text = $"<b>Vida:</b> {personagem.maxHealth}\n" + $"<b>Dano:</b> {personagem.damage}\n" + $"<b>Velocidade:</b> {personagem.moveSpeed}\n";

        string textoHabilidades = "";
        if (personagem.passive != null) { textoHabilidades += $"<b>{personagem.passive.abilityName} (Passiva):</b>\n{personagem.passive.description}\n\n"; }
        if (personagem.ability1 != null) { textoHabilidades += $"<b>{personagem.ability1.abilityName}:</b>\n{personagem.ability1.description}\n\n"; }
        if (personagem.ability2 != null) { textoHabilidades += $"<b>{personagem.ability2.abilityName}:</b>\n{personagem.ability2.description}\n\n"; }
        if (personagem.ultimate != null) { textoHabilidades += $"<b>{personagem.ultimate.abilityName} (Ultimate):</b>\n{personagem.ultimate.description}\n\n"; }
        textoHabilidadesComandante.text = textoHabilidades;

        PreencherTextosDeUpgrade(personagem);

        MostrarPainelHabilidades();

        botaoConfirmarEscolha.onClick.RemoveAllListeners();
        botaoConfirmarEscolha.onClick.AddListener(ConfirmarEscolha);

        // --- ADICIONA O LISTENER PARA O BOTÃO RASTROS ---
        if (botaoRastros != null)
        {
            botaoRastros.onClick.RemoveAllListeners();
            botaoRastros.onClick.AddListener(AbrirCenaRastros);
        }
        // -------------------------------------------------
    }

    void PreencherTextosDeUpgrade(CharacterBase personagem)
    {
        var textosCaminhos = new[] { textoCaminho1, textoCaminho2, textoCaminho3 };
        for (int i = 0; i < textosCaminhos.Length; i++)
        {
            if (personagem.upgradePaths != null && i < personagem.upgradePaths.Count)
            {
                string textoFinalCaminho = "";
                var path = personagem.upgradePaths[i];
                foreach (var upgrade in path.upgradesInPath)
                {
                    var costs = new List<string>();
                    if (upgrade.geoditeCost > 0) costs.Add($"<color=#76D7C4>{upgrade.geoditeCost}G</color>");
                    if (upgrade.darkEtherCost > 0) costs.Add($"<color=#C39BD3>{upgrade.darkEtherCost}E</color>");
                    string costText = (costs.Count > 0) ? $" (Custo: {string.Join(" / ", costs)})" : "";
                    textoFinalCaminho += $" • <b>{upgrade.upgradeName}:</b> {upgrade.description}{costText}\n";
                }
                textosCaminhos[i].text = textoFinalCaminho;
            }
            else { textosCaminhos[i].text = "Caminho não disponível."; }
        }
    }

    // --- FUNÇÕES DE CONTROLE DA UI ATUALIZADAS ---
    public void MostrarPainelHabilidades()
    {
        if (painelHabilidades != null) painelHabilidades.SetActive(true);
        if (painelUpgradesTorre != null) painelUpgradesTorre.SetActive(false);

        if (botaoAbaHabilidades != null) botaoAbaHabilidades.interactable = false;
        if (botaoAbaTorre != null) botaoAbaTorre.interactable = true;
    }

    public void MostrarPainelUpgradesTorre()
    {
        if (painelHabilidades != null) painelHabilidades.SetActive(false);
        if (painelUpgradesTorre != null) painelUpgradesTorre.SetActive(true);

        if (botaoAbaHabilidades != null) botaoAbaHabilidades.interactable = true;
        if (botaoAbaTorre != null) botaoAbaTorre.interactable = false;

        MostrarCaminhoDeUpgrade(1);
    }

    public void MostrarCaminhoDeUpgrade(int numeroDoCaminho)
    {
        if (textoCaminho1 != null) textoCaminho1.gameObject.SetActive(numeroDoCaminho == 1);
        if (textoCaminho2 != null) textoCaminho2.gameObject.SetActive(numeroDoCaminho == 2);
        if (textoCaminho3 != null) textoCaminho3.gameObject.SetActive(numeroDoCaminho == 3);

        for (int i = 0; i < botoesCaminhoTorre.Count; i++)
        {
            botoesCaminhoTorre[i].interactable = (i != numeroDoCaminho - 1);
        }
    }

    // --- NOVA FUNÇÃO PARA O BOTÃO RASTROS ---
    public void AbrirCenaRastros()
    {
        if (personagemEmVisualizacao == null)
        {
            Debug.LogError("Nenhum personagem selecionado para ver os Rastros.");
            return;
        }

        if (GameDataManager.Instance == null)
        {
            Debug.LogError("GameDataManager não encontrado!");
            return;
        }

        // 1. Armazena o personagem selecionado no GameDataManager
        GameDataManager.Instance.personagemParaRastros = personagemEmVisualizacao;

        // 2. Carrega a cena "Rastros"
        if (!string.IsNullOrEmpty(nomeDaCenaRastros))
        {
            SceneManager.LoadScene(nomeDaCenaRastros);
        }
        else
        {
            Debug.LogError("O nome da cena 'Rastros' não foi definido no Inspector!");
        }
    }
    // ----------------------------------------

    #region Funções de Navegação e Setup (Sem Alterações)
    // ... (todas as outras funções como LimparGrid, ConfirmarEscolha, etc. continuam aqui, sem alterações)
    void LimparGrid(Transform grid) { if (grid == null) return; foreach (Transform child in grid) { if (child != null) Destroy(child.gameObject); } }
    void ConfigurarBotoesPrincipais() { if (botaoJogar != null) { botaoJogar.onClick.RemoveAllListeners(); botaoJogar.interactable = false; botaoJogar.onClick.AddListener(IniciarJogo); } if (botaoVoltarDaEscolha != null) { botaoVoltarDaEscolha.onClick.RemoveAllListeners(); botaoVoltarDaEscolha.onClick.AddListener(VoltarParaPainelEquipe); } if (botaoVoltarDosDetalhes != null) { botaoVoltarDosDetalhes.onClick.RemoveAllListeners(); botaoVoltarDosDetalhes.onClick.AddListener(VoltarParaPainelEscolha); } }
    void CriarGridEquipe() { for (int i = 0; i < 8; i++) { GameObject slotObj = Instantiate(slotEquipePrefab, gridEquipeContainer); Button slotButton = slotObj.GetComponent<Button>(); int index = i; slotButton.onClick.AddListener(() => AbrirPainelEscolha(index)); slotsEquipe.Add(slotButton); } }
    void PopularGridDeEscolha() { foreach (var personagem in todosOsPersonagens) { GameObject slotObj = Instantiate(slotEscolhaPrefab, gridEscolhaContainer); slotObj.GetComponent<Image>().sprite = personagem.characterIcon; Button slotButton = slotObj.GetComponent<Button>(); slotButton.onClick.AddListener(() => AbrirPainelDetalhes(personagem)); botoesDeEscolha.Add(personagem, slotButton); } }
    public void AbrirPainelEscolha(int slotIndex) { slotSendoEditado = slotIndex; painelEquipe.SetActive(false); painelEscolhaPersonagem.SetActive(true); painelDetalhes.SetActive(false); if (GameDataManager.Instance == null) return; CharacterBase[] equipeAtual = GameDataManager.Instance.equipeSelecionada; foreach (var par in botoesDeEscolha) { bool jaEscolhido = equipeAtual.Contains(par.Key); bool noSlotAtual = (slotSendoEditado >= 0 && slotSendoEditado < equipeAtual.Length) && equipeAtual[slotSendoEditado] == par.Key; par.Value.interactable = !jaEscolhido || noSlotAtual; } }
    void ConfirmarEscolha() { if (GameDataManager.Instance != null && slotSendoEditado != -1) { GameDataManager.Instance.equipeSelecionada[slotSendoEditado] = personagemEmVisualizacao; slotsEquipe[slotSendoEditado].GetComponent<Image>().sprite = personagemEmVisualizacao.characterIcon; if (GameDataManager.Instance.equipeSelecionada[0] != null) { botaoJogar.interactable = true; } } VoltarParaPainelEquipe(); }
    public void VoltarParaPainelEquipe() { painelEscolhaPersonagem.SetActive(false); painelDetalhes.SetActive(false); painelEquipe.SetActive(true); slotSendoEditado = -1; personagemEmVisualizacao = null; }
    public void VoltarParaPainelEscolha() { painelDetalhes.SetActive(false); painelEscolhaPersonagem.SetActive(true); personagemEmVisualizacao = null; }
    public void IniciarJogo() { if (!string.IsNullOrEmpty(nomeDaCenaDoJogo)) { SceneManager.LoadScene(nomeDaCenaDoJogo); } else { Debug.LogError("O nome da cena do jogo não foi definido no Inspector!"); } }
    #endregion
}