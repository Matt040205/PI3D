// SelecaoManager.cs (Versão Final com Corrotina)
using System.Collections; // Necessário para Corrotinas
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SelecaoManager : MonoBehaviour
{
    [Header("Configuração dos Personagens")]
    public List<CharacterBase> todosOsPersonagens;

    [Header("Painéis da UI")]
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

    [Header("Elementos da UI - Detalhes")]
    public Image imagemDetalhes;
    public TextMeshProUGUI nomeDetalhes;
    public TextMeshProUGUI statusDetalhes;
    public Button botaoConfirmarEscolha;
    public Button botaoVoltarDosDetalhes;

    private List<Button> slotsEquipe = new List<Button>();
    private Dictionary<CharacterBase, Button> botoesDeEscolha = new Dictionary<CharacterBase, Button>();
    private int slotSendoEditado = -1;
    private CharacterBase personagemEmVisualizacao;

    void Start()
    {
        // Inicia a corrotina que vai configurar a UI de forma segura.
        StartCoroutine(SetupScene());
    }

    IEnumerator SetupScene()
    {
        // 1. Limpa tudo que possa ter sobrado da carga anterior da cena.
        LimparGrid(gridEquipeContainer);
        LimparGrid(gridEscolhaContainer);
        slotsEquipe.Clear();
        botoesDeEscolha.Clear();

        // 2. Garante que os painéis comecem no estado correto (invisíveis)
        //    para evitar que pisquem na tela por um frame.
        painelEquipe.SetActive(false);
        painelEscolhaPersonagem.SetActive(false);
        painelDetalhes.SetActive(false);

        // 3. (A PARTE MAIS IMPORTANTE) Espera até o final do frame.
        //    Isso dá tempo para a Unity inicializar completamente o Canvas e o sistema de UI
        //    após o carregamento da cena.
        yield return new WaitForEndOfFrame();

        // 4. Agora que a UI está pronta, configure e popule os grids.
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.LimparSelecao();
        }

        ConfigurarBotoesPrincipais();

        // Popula os grids
        CriarGridEquipe();
        PopularGridDeEscolha();

        // 5. Força o sistema de layout a recalcular as posições imediatamente.
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridEquipeContainer.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridEscolhaContainer.GetComponent<RectTransform>());

        // 6. Finalmente, ativa o painel inicial.
        painelEquipe.SetActive(true);
    }

    void LimparGrid(Transform grid)
    {
        if (grid == null) return;
        foreach (Transform child in grid)
        {
            if (child != null) Destroy(child.gameObject);
        }
    }

    void ConfigurarBotoesPrincipais()
    {
        if (botaoJogar != null)
        {
            botaoJogar.onClick.RemoveAllListeners();
            botaoJogar.interactable = false;
            botaoJogar.onClick.AddListener(IniciarJogo);
        }
        if (botaoVoltarDaEscolha != null)
        {
            botaoVoltarDaEscolha.onClick.RemoveAllListeners();
            botaoVoltarDaEscolha.onClick.AddListener(VoltarParaPainelEquipe);
        }
        if (botaoVoltarDosDetalhes != null)
        {
            botaoVoltarDosDetalhes.onClick.RemoveAllListeners();
            botaoVoltarDosDetalhes.onClick.AddListener(VoltarParaPainelEscolha);
        }
    }

    // As funções abaixo continuam iguais
    void CriarGridEquipe()
    {
        for (int i = 0; i < 8; i++)
        {
            GameObject slotObj = Instantiate(slotEquipePrefab, gridEquipeContainer);
            Button slotButton = slotObj.GetComponent<Button>();
            int index = i;
            slotButton.onClick.AddListener(() => AbrirPainelEscolha(index));
            slotsEquipe.Add(slotButton);
        }
    }

    void PopularGridDeEscolha()
    {
        foreach (var personagem in todosOsPersonagens)
        {
            GameObject slotObj = Instantiate(slotEscolhaPrefab, gridEscolhaContainer);
            slotObj.GetComponent<Image>().sprite = personagem.characterIcon;
            Button slotButton = slotObj.GetComponent<Button>();
            slotButton.onClick.AddListener(() => AbrirPainelDetalhes(personagem));
            botoesDeEscolha.Add(personagem, slotButton);
        }
    }

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
            bool jaEscolhido = equipeAtual.Contains(par.Key);
            bool noSlotAtual = (slotSendoEditado >= 0 && slotSendoEditado < equipeAtual.Length) && equipeAtual[slotSendoEditado] == par.Key;
            par.Value.interactable = !jaEscolhido || noSlotAtual;
        }
    }

    public void AbrirPainelDetalhes(CharacterBase personagem)
    {
        painelEscolhaPersonagem.SetActive(false);
        painelDetalhes.SetActive(true);
        personagemEmVisualizacao = personagem;
        imagemDetalhes.sprite = personagem.characterIcon;
        nomeDetalhes.text = personagem.name;
        statusDetalhes.text = $"Vida: {personagem.maxHealth}\nDano: {personagem.damage}\nVelocidade: {personagem.moveSpeed}";
        botaoConfirmarEscolha.onClick.RemoveAllListeners();
        botaoConfirmarEscolha.onClick.AddListener(ConfirmarEscolha);
    }

    void ConfirmarEscolha()
    {
        if (GameDataManager.Instance != null && slotSendoEditado != -1)
        {
            GameDataManager.Instance.equipeSelecionada[slotSendoEditado] = personagemEmVisualizacao;
            slotsEquipe[slotSendoEditado].GetComponent<Image>().sprite = personagemEmVisualizacao.characterIcon;
            if (GameDataManager.Instance.equipeSelecionada[0] != null)
            {
                botaoJogar.interactable = true;
            }
        }
        VoltarParaPainelEquipe();
    }

    public void VoltarParaPainelEquipe()
    {
        painelEscolhaPersonagem.SetActive(false);
        painelDetalhes.SetActive(false);
        painelEquipe.SetActive(true);
        slotSendoEditado = -1;
        personagemEmVisualizacao = null;
    }

    public void VoltarParaPainelEscolha()
    {
        painelDetalhes.SetActive(false);
        painelEscolhaPersonagem.SetActive(true);
        personagemEmVisualizacao = null;
    }

    public void IniciarJogo()
    {
        if (!string.IsNullOrEmpty(nomeDaCenaDoJogo))
        {
            SceneManager.LoadScene(nomeDaCenaDoJogo);
        }
        else
        {
            Debug.LogError("O nome da cena do jogo não foi definido no Inspector!");
        }
    }
}