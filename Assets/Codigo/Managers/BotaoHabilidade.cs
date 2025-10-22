// Arquivo: BotaoHabilidade.cs (Substitua o seu por este)

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // Necessário para List

/// <summary>
/// Define um par: um ScriptableObject de Personagem e o
/// ScriptableObject de Upgrade correspondente a ele.
/// </summary>
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

    // --- NOVA VARIÁVEL ---
    [Header("Mapeamento de Upgrades")]
    [Tooltip("Adicione um item a esta lista para CADA personagem que usar esta árvore.")]
    public List<MapeamentoUpgradePersonagem> upgradesPorPersonagem;
    // ---------------------

    [Header("Referências")]
    public Rastros managerRastros;
    private Button meuBotao;
    public Image iconeHabilidade;

    [Header("Cores de Estado")]
    public Color corBloqueado = new Color(0.5f, 0.5f, 0.5f, 0.5f);
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

    void OnEnable()
    {
        if (managerRastros != null)
        {
            VerificarEstado(managerRastros);
        }
    }

    // Chamado quando o botão da UI é clicado
    void OnBotaoClicado()
    {
        managerRastros.SelecionarHabilidade(this);
    }

    // --- ESTA É A FUNÇÃO QUE ESTÁ FALTANDO ---
    /// <summary>
    /// Procura na lista 'upgradesPorPersonagem' e retorna o 
    /// RastroUpgrade correto para o personagem ativo.
    /// </summary>
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

        // Se não encontrar um mapeamento específico, retorna nulo
        return null;
    }
    // ------------------------------------

    /// <summary>
    /// Verifica e atualiza a aparência do botão com base no estado da árvore.
    /// (Função sem mudanças)
    /// </summary>
    public void VerificarEstado(Rastros manager)
    {
        if (manager.habilidadesDesbloqueadas.Contains(idHabilidade))
        {
            if (iconeHabilidade != null) iconeHabilidade.color = corDesbloqueado;
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

            int pontosAtuaisCaminho = manager.pontosPorCaminho.ContainsKey(idCaminho) ? manager.pontosPorCaminho[idCaminho] : 0;
            if (pontosAtuaisCaminho < pontosNoCaminhoNecessarios)
                disponivel = false;


            if (disponivel)
            {
                if (iconeHabilidade != null) iconeHabilidade.color = corDisponivel;
                meuBotao.interactable = true;
            }
            else
            {
                if (iconeHabilidade != null) iconeHabilidade.color = corBloqueado;
                meuBotao.interactable = false;
            }
        }
    }
}