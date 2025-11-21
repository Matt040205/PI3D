using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelecaoManager : MonoBehaviour
{
    public List<CharacterBase> todosOsPersonagens;

    [Header("Painéis")]
    public GameObject painelEquipe;
    public GameObject painelEscolhaPersonagem;
    public GameObject painelDetalhes;

    [Header("UI Principal")]
    public GameObject slotEquipePrefab;
    public Transform gridEquipeContainer;
    public Button botaoJogar;
    public string nomeDaCenaDoJogo;

    [Header("Modo Remover")]
    public Button botaoRemover;
    public Color corModoRemover = Color.red;
    private bool isRemoveMode = false;
    private Color corOriginalBotaoRemover;

    [Header("Seleção de Personagem")]
    public GameObject slotEscolhaPrefab;
    public Transform gridEscolhaContainer;
    public Button botaoVoltarDaEscolha;

    [Header("Detalhes do Personagem")]
    public Image imagemDetalhes;
    public TextMeshProUGUI nomeDetalhes;
    public TextMeshProUGUI textoStatusPadrao;
    public Button botaoConfirmarEscolha;
    public Button botaoVoltarDosDetalhes;

    [Header("Abas de Detalhes")]
    public GameObject painelHabilidades;
    public GameObject painelUpgradesTorre;
    public TextMeshProUGUI textoHabilidadesComandante;
    public TextMeshProUGUI textoCaminho1;
    public TextMeshProUGUI textoCaminho2;
    public TextMeshProUGUI textoCaminho3;
    public Button botaoAbaHabilidades;
    public Button botaoAbaTorre;
    public List<Button> botoesCaminhoTorre;
    public Button botaoRastros;
    public string nomeDaCenaRastros = "Rastros";

    private List<SlotEquipeUI> slotsEquipe = new List<SlotEquipeUI>();
    private Dictionary<CharacterBase, Button> botoesDeEscolha = new Dictionary<CharacterBase, Button>();
    private int slotSendoEditado = -1;
    private CharacterBase personagemEmVisualizacao;

    void Start()
    {
        if (botaoRemover != null)
        {
            corOriginalBotaoRemover = botaoRemover.image.color;
            botaoRemover.onClick.AddListener(ToggleRemoveMode);
        }
        StartCoroutine(SetupScene());
    }

    IEnumerator SetupScene()
    {
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
            if (GameDataManager.Instance.personagemParaRastros != null)
            {
                Debug.Log("SELECAO_MANAGER: Voltando da cena de Rastros.");
                if (TutorialManager.Instance != null)
                {
                    TutorialManager.Instance.TriggerTutorial("GO_TO_ACTION");
                }
                GameDataManager.Instance.personagemParaRastros = null;
            }
            else
            {
                Debug.Log("SELECAO_MANAGER: Iniciando cena do zero. Limpando seleção.");
                if (TutorialManager.Instance != null)
                {
                    TutorialManager.Instance.TriggerTutorial("SELECT_COMMANDER");
                }
                GameDataManager.Instance.LimparSelecao();
                GameDataManager.Instance.personagemParaRastros = null;
            }
        }

        ConfigurarBotoesPrincipais();
        CriarGridEquipe();
        PopularGridDeEscolha();
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridEquipeContainer.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridEscolhaContainer.GetComponent<RectTransform>());
        painelEquipe.SetActive(true);

        AtualizarEstadoBotaoJogar();
    }

    void ToggleRemoveMode()
    {
        isRemoveMode = !isRemoveMode;
        if (botaoRemover != null)
        {
            botaoRemover.image.color = isRemoveMode ? corModoRemover : corOriginalBotaoRemover;
        }
    }

    void OnSlotClicked(int slotIndex)
    {
        if (isRemoveMode)
        {
            if (GameDataManager.Instance != null)
            {
                if (GameDataManager.Instance.equipeSelecionada[slotIndex] != null)
                {
                    Destroy(GameDataManager.Instance.equipeSelecionada[slotIndex]);
                    GameDataManager.Instance.equipeSelecionada[slotIndex] = null;
                }

                slotsEquipe[slotIndex].LimparSlot();
                AtualizarEstadoBotaoJogar();

                ToggleRemoveMode();
            }
        }
        else
        {
            AbrirPainelEscolha(slotIndex);
        }
    }

    public void AbrirPainelDetalhes(CharacterBase personagem)
    {
        painelEscolhaPersonagem.SetActive(false);
        painelDetalhes.SetActive(true);
        personagemEmVisualizacao = personagem;

        if (TutorialManager.Instance != null)
        {
            if (slotSendoEditado == 0)
            {
                TutorialManager.Instance.TriggerTutorial("COMMANDER_SKILLS");
            }
            else if (slotSendoEditado == 1)
            {
                TutorialManager.Instance.TriggerTutorial("TOWER_UPGRADES");
            }
        }

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
        AtualizarTextoBotoesCaminho(personagem);

        if (slotSendoEditado == 0)
        {
            MostrarPainelHabilidades();
        }
        else
        {
            MostrarPainelUpgradesTorre();
        }

        botaoConfirmarEscolha.onClick.RemoveAllListeners();
        botaoConfirmarEscolha.onClick.AddListener(ConfirmarEscolha);

        if (botaoRastros != null)
        {
            CharacterBase instanceInSlot = null;
            if (GameDataManager.Instance != null && slotSendoEditado != -1 && slotSendoEditado < GameDataManager.Instance.equipeSelecionada.Length)
            {
                instanceInSlot = GameDataManager.Instance.equipeSelecionada[slotSendoEditado];
            }

            if (instanceInSlot != null && instanceInSlot.name.StartsWith(personagem.name))
            {
                Debug.Log($"SELECAO_MANAGER: Botão Rastros aponta para a INSTÂNCIA (editável): {instanceInSlot.name}");
                botaoRastros.interactable = true;
                botaoRastros.onClick.RemoveAllListeners();
                botaoRastros.onClick.AddListener(() => AbrirCenaRastros(instanceInSlot));
            }
            else
            {
                Debug.Log($"SELECAO_MANAGER: Botão Rastros aponta para o ASSET (visualização): {personagemEmVisualizacao.name}. (Será ativado após confirmar)");
                botaoRastros.interactable = false;
                botaoRastros.onClick.RemoveAllListeners();
            }
        }
    }

    void AtualizarTextoBotoesCaminho(CharacterBase personagem)
    {
        if (botoesCaminhoTorre == null || personagem == null || personagem.upgradePaths == null) return;

        for (int i = 0; i < botoesCaminhoTorre.Count; i++)
        {
            Button botao = botoesCaminhoTorre[i];
            if (botao != null)
            {
                TextMeshProUGUI textoBotao = botao.GetComponentInChildren<TextMeshProUGUI>();
                if (textoBotao != null)
                {
                    if (i < personagem.upgradePaths.Count && personagem.upgradePaths[i] != null)
                    {
                        textoBotao.text = personagem.upgradePaths[i].pathName;
                        botao.gameObject.SetActive(true);
                    }
                    else
                    {
                        textoBotao.text = "-";
                        botao.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    void PreencherTextosDeUpgrade(CharacterBase personagem)
    {
        var textosCaminhos = new[] { textoCaminho1, textoCaminho2, textoCaminho3 };
        for (int i = 0; i < textosCaminhos.Length; i++)
        {
            if (personagem.upgradePaths != null && i < personagem.upgradePaths.Count)
            {
                var path = personagem.upgradePaths[i];
                string textoFinalCaminho = $"<b>{path.pathName}</b>\n\n";

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

    public void MostrarPainelHabilidades()
    {
        if (painelHabilidades != null) painelHabilidades.SetActive(true);
        if (painelUpgradesTorre != null) painelUpgradesTorre.SetActive(false);

        if (botaoAbaHabilidades != null) botaoAbaHabilidades.interactable = false;
        if (botaoAbaTorre != null) botaoAbaTorre.interactable = true;

        SetBotoesCaminhoVisiveis(false);

        if (textoCaminho1 != null) textoCaminho1.gameObject.SetActive(false);
        if (textoCaminho2 != null) textoCaminho2.gameObject.SetActive(false);
        if (textoCaminho3 != null) textoCaminho3.gameObject.SetActive(false);
    }

    public void MostrarPainelUpgradesTorre()
    {
        if (painelHabilidades != null) painelHabilidades.SetActive(false);
        if (painelUpgradesTorre != null) painelUpgradesTorre.SetActive(true);

        if (botaoAbaHabilidades != null) botaoAbaHabilidades.interactable = true;
        if (botaoAbaTorre != null) botaoAbaTorre.interactable = false;

        SetBotoesCaminhoVisiveis(true);

        MostrarCaminhoDeUpgrade(1);
    }

    private void SetBotoesCaminhoVisiveis(bool visivel)
    {
        if (botoesCaminhoTorre == null) return;

        for (int i = 0; i < botoesCaminhoTorre.Count; i++)
        {
            Button botao = botoesCaminhoTorre[i];
            if (botao != null)
            {
                bool hasPath = personagemEmVisualizacao != null &&
                          personagemEmVisualizacao.upgradePaths != null &&
                          i < personagemEmVisualizacao.upgradePaths.Count &&
                          personagemEmVisualizacao.upgradePaths[i] != null;

                botao.gameObject.SetActive(visivel && hasPath);
            }
        }
    }


    public void MostrarCaminhoDeUpgrade(int numeroDoCaminho)
    {
        if (textoCaminho1 != null) textoCaminho1.gameObject.SetActive(numeroDoCaminho == 1);
        if (textoCaminho2 != null) textoCaminho2.gameObject.SetActive(numeroDoCaminho == 2);
        if (textoCaminho3 != null) textoCaminho3.gameObject.SetActive(numeroDoCaminho == 3);

        for (int i = 0; i < botoesCaminhoTorre.Count; i++)
        {
            if (botoesCaminhoTorre[i] != null)
            {
                botoesCaminhoTorre[i].interactable = (i != numeroDoCaminho - 1);
            }
        }
    }

    public void AbrirCenaRastros(CharacterBase personagemParaEditar)
    {
        if (personagemParaEditar == null)
        {
            Debug.LogError("SELECAO_MANAGER: Personagem para Rastros é nulo.");
            return;
        }

        if (GameDataManager.Instance == null)
        {
            Debug.LogError("SELECAO_MANAGER: GameDataManager não encontrado!");
            return;
        }

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.TriggerTutorial("EXPLAIN_TRAILS");
        }

        Debug.Log($"SELECAO_MANAGER: Enviando personagem para Rastros: {personagemParaEditar.name}");
        GameDataManager.Instance.personagemParaRastros = personagemParaEditar;

        if (!string.IsNullOrEmpty(nomeDaCenaRastros))
        {
            SceneManager.LoadScene(nomeDaCenaRastros);
        }
        else
        {
            Debug.LogError("O nome da cena 'Rastros' não foi definido no Inspector!");
        }
    }

    void LimparGrid(Transform grid) { if (grid == null) return; foreach (Transform child in grid) { if (child != null) Destroy(child.gameObject); } }
    void ConfigurarBotoesPrincipais() { if (botaoJogar != null) { botaoJogar.onClick.RemoveAllListeners(); botaoJogar.interactable = false; botaoJogar.onClick.AddListener(IniciarJogo); } if (botaoVoltarDaEscolha != null) { botaoVoltarDaEscolha.onClick.RemoveAllListeners(); botaoVoltarDaEscolha.onClick.AddListener(VoltarParaPainelEquipe); } if (botaoVoltarDosDetalhes != null) { botaoVoltarDosDetalhes.onClick.RemoveAllListeners(); botaoVoltarDosDetalhes.onClick.AddListener(VoltarParaPainelEscolha); } }

    void CriarGridEquipe()
    {
        for (int i = 0; i < 8; i++)
        {
            GameObject slotObj = Instantiate(slotEquipePrefab, gridEquipeContainer);
            Button slotButton = slotObj.GetComponent<Button>();
            SlotEquipeUI slotUI = slotObj.GetComponent<SlotEquipeUI>();

            int index = i;
            slotButton.onClick.AddListener(() => OnSlotClicked(index));

            if (GameDataManager.Instance != null && GameDataManager.Instance.equipeSelecionada[i] != null)
            {
                slotUI.SetPersonagem(GameDataManager.Instance.equipeSelecionada[i]);
            }
            else
            {
                slotUI.LimparSlot();
            }

            slotsEquipe.Add(slotUI);
        }
    }

    void PopularGridDeEscolha() { foreach (var personagem in todosOsPersonagens) { GameObject slotObj = Instantiate(slotEscolhaPrefab, gridEscolhaContainer); slotObj.GetComponent<Image>().sprite = personagem.characterIcon; Button slotButton = slotObj.GetComponent<Button>(); slotButton.onClick.AddListener(() => AbrirPainelDetalhes(personagem)); botoesDeEscolha.Add(personagem, slotButton); } }

    public void AbrirPainelEscolha(int slotIndex)
    {
        slotSendoEditado = slotIndex;
        painelEquipe.SetActive(false);
        painelEscolhaPersonagem.SetActive(true);
        painelDetalhes.SetActive(false);

        if (GameDataManager.Instance == null) return;
        CharacterBase[] equipeAtual = GameDataManager.Instance.equipeSelecionada;
        foreach (var par in botoesDeEscolha)
        {
            bool jaEscolhido = false;
            foreach (CharacterBase instance in equipeAtual)
            {
                if (instance != null && instance.name.StartsWith(par.Key.name))
                {
                    jaEscolhido = true;
                    break;
                }
            }

            bool noSlotAtual = (slotSendoEditado >= 0 && slotSendoEditado < equipeAtual.Length) &&
                              (equipeAtual[slotSendoEditado] != null && equipeAtual[slotSendoEditado].name.StartsWith(par.Key.name));

            par.Value.interactable = !jaEscolhido || noSlotAtual;
        }
    }

    void ConfirmarEscolha()
    {
        if (GameDataManager.Instance != null && slotSendoEditado != -1)
        {
            if (TutorialManager.Instance != null)
            {
                if (slotSendoEditado == 0)
                {
                    TutorialManager.Instance.TriggerTutorial("SELECT_TOWER");
                }
                else if (slotSendoEditado == 1)
                {
                    TutorialManager.Instance.TriggerTutorial("EXPLAIN_TRAILS");
                }
            }

            if (GameDataManager.Instance.equipeSelecionada[slotSendoEditado] != null)
            {
                Debug.Log($"SELECAO_MANAGER: Destruindo instância antiga em {slotSendoEditado}");
                Destroy(GameDataManager.Instance.equipeSelecionada[slotSendoEditado]);
            }

            Debug.Log($"SELECAO_MANAGER: Criando nova instância de {personagemEmVisualizacao.name} no slot {slotSendoEditado}");
            GameDataManager.Instance.equipeSelecionada[slotSendoEditado] = Instantiate(personagemEmVisualizacao);
            slotsEquipe[slotSendoEditado].SetPersonagem(GameDataManager.Instance.equipeSelecionada[slotSendoEditado]);
            AtualizarEstadoBotaoJogar();
        }
        VoltarParaPainelEquipe();
    }

    void AtualizarEstadoBotaoJogar()
    {
        if (GameDataManager.Instance != null && botaoJogar != null)
        {
            bool podeJogar = GameDataManager.Instance.equipeSelecionada[0] != null &&
                                    GameDataManager.Instance.equipeSelecionada[1] != null;
            botaoJogar.interactable = podeJogar;
        }
        else if (botaoJogar != null)
        {
            botaoJogar.interactable = false;
        }
    }

    public void VoltarParaPainelEquipe()
    {
        painelEscolhaPersonagem.SetActive(false); painelDetalhes.SetActive(false); painelEquipe.SetActive(true); slotSendoEditado = -1; personagemEmVisualizacao = null;
    }
    public void VoltarParaPainelEscolha() { painelDetalhes.SetActive(false); painelEscolhaPersonagem.SetActive(true); personagemEmVisualizacao = null; }
    public void IniciarJogo() { if (!string.IsNullOrEmpty(nomeDaCenaDoJogo)) { SceneManager.LoadScene(nomeDaCenaDoJogo); } else { Debug.LogError("O nome da cena do jogo não foi definido no Inspector!"); } }
}