using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public struct MapeamentoUpgradePersonagem
{
    [Tooltip("Arraste o SO do Personagem (ex: Arqueira.asset)")]
    public CharacterBase personagemSO;
    [Tooltip("Arraste o SO do Upgrade (ex: Arqueira_Rastro_1.asset)")]
    public RastroUpgrade upgradeSO;
}


[RequireComponent(typeof(Button))]
public class BotaoHabilidade : MonoBehaviour
{
    [Header("Identificação da Habilidade")]
    public string idHabilidade;
    public string idCaminho;

    [Header("Requisitos para Desbloquear")]
    public int custo = 1;
    public string preRequisitoHabilidade;
    public int pontosGlobaisNecessarios = 0;
    public int pontosNoCaminhoNecessarios = 0;

    [Header("Mapeamento de Upgrades")]
    [Tooltip("Adicione um item a esta lista para CADA personagem que usar esta árvore.")]
    public List<MapeamentoUpgradePersonagem> upgradesPorPersonagem;

    [Header("Referências")]
    public Rastros managerRastros;
    private Button meuBotao;
    public Image iconeHabilidade;

    [Header("Cores de Estado")]
    public Color corBloqueado = new Color(0.5f, 0.5f, 0.5f, 1f);
    public Color corDisponivel = Color.white;
    public Color corDesbloqueado = Color.green;


    void Start()
    {
        meuBotao = GetComponent<Button>();
        meuBotao.onClick.RemoveAllListeners();
        meuBotao.onClick.AddListener(OnBotaoClicado);

        if (managerRastros == null)
        {
            managerRastros = FindObjectOfType<Rastros>();
        }

        if (managerRastros != null && !managerRastros.todosBotoesDaArvore.Contains(this))
        {
            managerRastros.todosBotoesDaArvore.Add(this);
        }
    }

    // OnEnable foi removido para impedir que corra antes do Rastros.Awake()

    void OnBotaoClicado()
    {
        if (managerRastros != null)
        {
            managerRastros.SelecionarHabilidade(this);
        }
    }

    public RastroUpgrade GetUpgradeParaPersonagem(CharacterBase personagemAtivo)
    {
        if (personagemAtivo == null) return null;

        foreach (var map in upgradesPorPersonagem)
        {
            if (map.personagemSO == personagemAtivo)
            {
                return map.upgradeSO;
            }
        }
        return null;
    }

    public void VerificarEstado(Rastros manager)
    {
        if (meuBotao == null) meuBotao = GetComponent<Button>();

        // --- PROTEÇÃO ADICIONADA ---
        if (manager == null || manager.habilidadesDesbloqueadas == null || manager.pontosPorCaminho == null)
        {
            // Tenta apanhar o manager se a referência se perdeu
            if (managerRastros == null)
                managerRastros = FindObjectOfType<Rastros>();

            // Se ainda assim for nulo, sai
            if (managerRastros == null || managerRastros.habilidadesDesbloqueadas == null)
            {
                if (meuBotao != null) meuBotao.interactable = false;
                return;
            }

            manager = managerRastros;
        }

        ColorBlock colors = meuBotao.colors;

        if (manager.habilidadesDesbloqueadas.Contains(idHabilidade))
        {
            // --- 1. COMPRADO (Claro / Verde) ---
            if (iconeHabilidade != null) iconeHabilidade.color = corDesbloqueado;
            colors.disabledColor = corDesbloqueado;
            meuBotao.interactable = false;
        }
        else
        {
            bool disponivel = true;
            if (manager.pontosDisponiveis < custo)
                disponivel = false;

            if (!string.IsNullOrEmpty(preRequisitoHabilidade) && !manager.habilidadesDesbloqueadas.Contains(preRequisitoHabilidade))
                disponivel = false;

            if (manager.pontosGastosGlobal < pontosGlobaisNecessarios)
                disponivel = false;

            // --- LÓGICA ATUALIZADA ---
            int pontosAtuaisCaminho = 0;
            foreach (var caminho in manager.pontosPorCaminho)
            {
                if (caminho.idCaminho == idCaminho)
                {
                    pontosAtuaisCaminho = caminho.pontosGastos;
                    break;
                }
            }
            if (pontosAtuaisCaminho < pontosNoCaminhoNecessarios)
                disponivel = false;

            if (disponivel)
            {
                // --- 2. DISPONÍVEL (Escuro, mas clicável e claro no hover) ---
                if (iconeHabilidade != null) iconeHabilidade.color = corBloqueado;
                colors.normalColor = corBloqueado;
                colors.highlightedColor = corDisponivel;
                colors.pressedColor = new Color(0.9f, 0.9f, 0.9f);
                colors.selectedColor = corBloqueado;
                meuBotao.interactable = true;
            }
            else
            {
                // --- 3. BLOQUEADO (Escuro) ---
                if (iconeHabilidade != null) iconeHabilidade.color = corBloqueado;
                colors.disabledColor = corBloqueado;
                meuBotao.interactable = false;
            }
        }
        meuBotao.colors = colors;
    }
}