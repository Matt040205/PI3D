// SelecaoManager.cs (Versão com TextMeshPro)
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // << IMPORTANTE: Adicione esta linha

public class SelecaoManager : MonoBehaviour
{
    [Header("Configuração dos Personagens")]
    public List<CharacterBase> todosOsPersonagens;

    [Header("Painéis da UI")]
    public GameObject painelEquipe;
    public GameObject painelEscolhaPersonagem;
    public GameObject painelDetalhes;

    [Header("Elementos da UI - Equipe (Automático)")]
    public GameObject slotEquipePrefab;
    public Transform gridEquipeContainer;
    public Button botaoJogar;
    public string nomeDaCenaDoJogo;

    [Header("Elementos da UI - Escolha (Automático)")]
    public GameObject slotEscolhaPrefab;
    public Transform gridEscolhaContainer;
    public Button botaoVoltarDaEscolha;

    [Header("Elementos da UI - Detalhes (TextMeshPro)")]
    public Image imagemDetalhes;
    public TextMeshProUGUI nomeDetalhes;    // << MUDANÇA AQUI
    public TextMeshProUGUI statusDetalhes;  // << MUDANÇA AQUI
    public Button botaoConfirmarEscolha;
    public Button botaoVoltarDosDetalhes;

    private List<Button> slotsEquipe = new List<Button>();
    private Dictionary<CharacterBase, Button> botoesDeEscolha = new Dictionary<CharacterBase, Button>();
    private int slotSendoEditado = -1;
    private CharacterBase personagemEmVisualizacao;

    void Start()
    {
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.LimparSelecao();
        }

        botaoJogar.interactable = false;

        CriarGridEquipe();
        PopularGridDeEscolha();
        ConfigurarBotoesDeVoltar();

        painelEquipe.SetActive(true);
        painelEscolhaPersonagem.SetActive(false);
        painelDetalhes.SetActive(false);
    }

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

    void ConfigurarBotoesDeVoltar()
    {
        if (botaoVoltarDaEscolha != null)
        {
            botaoVoltarDaEscolha.onClick.AddListener(VoltarParaPainelEquipe);
        }
        if (botaoVoltarDosDetalhes != null)
        {
            botaoVoltarDosDetalhes.onClick.AddListener(VoltarParaPainelEscolha);
        }
    }

    public void AbrirPainelEscolha(int slotIndex)
    {
        slotSendoEditado = slotIndex;
        painelEquipe.SetActive(false);
        painelEscolhaPersonagem.SetActive(true);
        painelDetalhes.SetActive(false);

        CharacterBase[] equipeAtual = GameDataManager.Instance.equipeSelecionada;
        foreach (var par in botoesDeEscolha)
        {
            bool jaEscolhido = equipeAtual.Contains(par.Key);
            bool noSlotAtual = equipeAtual[slotSendoEditado] == par.Key;

            if (jaEscolhido && !noSlotAtual)
            {
                par.Value.interactable = false;
            }
            else
            {
                par.Value.interactable = true;
            }
        }
    }

    public void AbrirPainelDetalhes(CharacterBase personagem)
    {
        painelEscolhaPersonagem.SetActive(false);
        painelDetalhes.SetActive(true);

        personagemEmVisualizacao = personagem;

        imagemDetalhes.sprite = personagem.characterIcon;

        // A lógica aqui continua exatamente a mesma
        nomeDetalhes.text = personagem.name;
        statusDetalhes.text = $"Vida: {personagem.maxHealth}\nDano: {personagem.damage}\nVelocidade: {personagem.moveSpeed}";

        botaoConfirmarEscolha.onClick.RemoveAllListeners();
        botaoConfirmarEscolha.onClick.AddListener(ConfirmarEscolha);
    }

    void ConfirmarEscolha()
    {
        GameDataManager.Instance.equipeSelecionada[slotSendoEditado] = personagemEmVisualizacao;
        slotsEquipe[slotSendoEditado].GetComponent<Image>().sprite = personagemEmVisualizacao.characterIcon;

        if (GameDataManager.Instance.equipeSelecionada[0] != null)
        {
            botaoJogar.interactable = true;
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
        SceneManager.LoadScene(nomeDaCenaDoJogo);
    }
}