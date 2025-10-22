// Arquivo: Rastros.cs (Atualizado para usar Mapeamento)

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class Rastros : MonoBehaviour
{
    [Header("Pontos do Jogador")]
    public int pontosDisponiveis = 10;
    public int pontosGastosGlobal = 0;
    public Dictionary<string, int> pontosPorCaminho = new Dictionary<string, int>();
    public HashSet<string> habilidadesDesbloqueadas = new HashSet<string>();

    [Header("Referências da UI (Árvore)")]
    public List<BotaoHabilidade> todosBotoesDaArvore;
    public Text textoPontosDisponiveis;

    [Header("Painel de Detalhes da Habilidade")]
    public TextMeshProUGUI textoNomePersonagem;
    public TextMeshProUGUI textoDescricaoHabilidade;
    public Button botaoConfirmarCompra;
    public GameObject painelDescricao;

    private CharacterBase personagemSendoUprado;
    private BotaoHabilidade habilidadeSelecionada;

    void Start()
    {
        // --- LÓGICA DE CARREGAMENTO DO PERSONAGEM (sem mudanças) ---
        if (GameDataManager.Instance != null && GameDataManager.Instance.personagemParaRastros != null)
        {
            personagemSendoUprado = GameDataManager.Instance.personagemParaRastros;
            Debug.Log("Configurando Rastros para: " + personagemSendoUprado.name);
            if (textoNomePersonagem != null)
            {
                textoNomePersonagem.text = personagemSendoUprado.name;
            }
        }
        else
        {
            Debug.LogError("Personagem para Rastros não encontrado no GameDataManager!");
            if (textoNomePersonagem != null)
            {
                textoNomePersonagem.text = "Personagem Desconhecido";
            }
            return;
        }

        InicializarCaminhos();

        // --- LÓGICA DE SAVE/LOAD (sem mudanças) ---
        // TODO: Carregar 'habilidadesDesbloqueadas'
        ReaplicarUpgradesSalvos(); // <-- Esta função foi atualizada abaixo

        // --- Configura o painel de descrição (sem mudanças) ---
        if (painelDescricao != null)
        {
            painelDescricao.SetActive(false);
        }
        if (botaoConfirmarCompra != null)
        {
            botaoConfirmarCompra.onClick.RemoveAllListeners();
            botaoConfirmarCompra.onClick.AddListener(ConfirmarCompraHabilidade);
        }

        AtualizarUI();
        AtualizarEstadoBotoes();
    }

    void InicializarCaminhos()
    {
        if (!pontosPorCaminho.ContainsKey("Central")) pontosPorCaminho.Add("Central", 0);
        if (!pontosPorCaminho.ContainsKey("Caminho1")) pontosPorCaminho.Add("Caminho1", 0);
        if (!pontosPorCaminho.ContainsKey("Caminho2")) pontosPorCaminho.Add("Caminho2", 0);
        if (!pontosPorCaminho.ContainsKey("Caminho3")) pontosPorCaminho.Add("Caminho3", 0);
        if (!pontosPorCaminho.ContainsKey("Caminho4")) pontosPorCaminho.Add("Caminho4", 0);
    }

    /// <summary>
    /// Chamado pelo BotaoHabilidade.cs quando um botão é clicado.
    /// </summary>
    public void SelecionarHabilidade(BotaoHabilidade botao)
    {
        habilidadeSelecionada = botao;

        if (habilidadeSelecionada == null || habilidadesDesbloqueadas.Contains(habilidadeSelecionada.idHabilidade))
        {
            if (painelDescricao != null) painelDescricao.SetActive(false);
            habilidadeSelecionada = null;
            return;
        }

        if (painelDescricao != null) painelDescricao.SetActive(true);

        // --- MUDANÇA AQUI ---
        // Pega o upgrade CORRETO para o personagem ATIVO
        RastroUpgrade upgradeCorreto = habilidadeSelecionada.GetUpgradeParaPersonagem(personagemSendoUprado);

        if (upgradeCorreto != null)
        {
            textoDescricaoHabilidade.text = $"<b>{upgradeCorreto.upgradeName}</b>\n\n{upgradeCorreto.description}";
        }
        else
        {
            // Caso o botão não tenha um upgrade configurado para este personagem
            textoDescricaoHabilidade.text = $"Habilidade não configurada para {personagemSendoUprado.name}.";
        }
        // -----------------

        bool podeComprar = ChecarCondicoes(habilidadeSelecionada);

        // Só pode confirmar se puder comprar E se o upgrade estiver configurado
        botaoConfirmarCompra.interactable = podeComprar && (upgradeCorreto != null);
    }

    /// <summary>
    /// Chamado pelo 'botaoConfirmarCompra' da UI.
    /// (Sem mudanças)
    /// </summary>
    public void ConfirmarCompraHabilidade()
    {
        if (habilidadeSelecionada == null) return;

        bool sucesso = TentarDesbloquear(habilidadeSelecionada); // <-- Esta função foi atualizada abaixo

        if (painelDescricao != null) painelDescricao.SetActive(false);
        habilidadeSelecionada = null;
    }


    /// <summary>
    /// Função separada que APENAS checa se o upgrade pode ser comprado.
    /// (Sem mudanças)
    /// </summary>
    private bool ChecarCondicoes(BotaoHabilidade botao)
    {
        if (botao == null) return false;
        if (habilidadesDesbloqueadas.Contains(botao.idHabilidade)) return false;
        if (pontosDisponiveis < botao.custo) return false;
        if (!string.IsNullOrEmpty(botao.preRequisitoHabilidade) && !habilidadesDesbloqueadas.Contains(botao.preRequisitoHabilidade)) return false;
        if (pontosGastosGlobal < botao.pontosGlobaisNecessarios) return false;
        int pontosAtuaisNesteCaminho = pontosPorCaminho.ContainsKey(botao.idCaminho) ? pontosPorCaminho[botao.idCaminho] : 0;
        if (pontosAtuaisNesteCaminho < botao.pontosNoCaminhoNecessarios) return false;

        return true;
    }

    /// <summary>
    /// Função principal que executa a lógica de compra.
    /// </summary>
    private bool TentarDesbloquear(BotaoHabilidade botao)
    {
        if (!ChecarCondicoes(botao))
        {
            return false;
        }

        // --- MUDANÇA AQUI ---
        // Pega o upgrade correto ANTES de aplicar
        RastroUpgrade upgradeCorreto = botao.GetUpgradeParaPersonagem(personagemSendoUprado);

        if (upgradeCorreto == null)
        {
            Debug.LogError($"Tentativa de comprar {botao.idHabilidade} falhou: Upgrade não mapeado para {personagemSendoUprado.name}!");
            return false;
        }
        // -----------------

        // --- SUCESSO! (Lógica de pontos igual a antes) ---
        pontosDisponiveis -= botao.custo;
        pontosGastosGlobal += 1;
        pontosPorCaminho[botao.idCaminho] = pontosPorCaminho[botao.idCaminho] + 1;
        habilidadesDesbloqueadas.Add(botao.idHabilidade);

        // --- APLICA O UPGRADE CORRETO ---
        AplicarUpgrade(upgradeCorreto);
        Debug.Log("Upgrade " + upgradeCorreto.upgradeName + " aplicado!");

        // TODO: Salvar o 'habilidadesDesbloqueadas'

        AtualizarUI();
        AtualizarEstadoBotoes();
        return true;
    }

    /// <summary>
    /// Modifica o ScriptableObject 'personagemSendoUprado' com os status do upgrade.
    /// (Sem mudanças)
    /// </summary>
    private void AplicarUpgrade(RastroUpgrade upgrade)
    {
        if (personagemSendoUprado == null || upgrade == null) return;

        foreach (var modifier in upgrade.modifiers)
        {
            switch (modifier.statToModify)
            {
                case CharacterStatType.MaxHealth:
                    if (modifier.modType == ModificationType.Additive)
                        personagemSendoUprado.maxHealth += modifier.value;
                    else
                        personagemSendoUprado.maxHealth *= (1 + modifier.value);
                    break;

                case CharacterStatType.Damage:
                    if (modifier.modType == ModificationType.Additive)
                        personagemSendoUprado.damage += modifier.value;
                    else
                        personagemSendoUprado.damage *= (1 + modifier.value);
                    break;

                case CharacterStatType.MoveSpeed:
                    if (modifier.modType == ModificationType.Additive)
                        personagemSendoUprado.moveSpeed += modifier.value;
                    else
                        personagemSendoUprado.moveSpeed *= (1 + modifier.value);
                    break;

                    // (Adicione os outros 'case' para os stats restantes...)
            }
        }
    }

    /// <summary>
    /// Reaplica upgrades salvos ao carregar a cena.
    /// </summary>
    private void ReaplicarUpgradesSalvos()
    {
        Debug.Log("Reaplicando " + habilidadesDesbloqueadas.Count + " upgrades salvos...");
        foreach (var botao in todosBotoesDaArvore)
        {
            if (botao != null && habilidadesDesbloqueadas.Contains(botao.idHabilidade))
            {
                // --- MUDANÇA AQUI ---
                // Pega o upgrade correto para o personagem atual
                RastroUpgrade upgradeCorreto = botao.GetUpgradeParaPersonagem(personagemSendoUprado);

                if (upgradeCorreto != null)
                {
                    AplicarUpgrade(upgradeCorreto);
                }
                // -----------------
            }
        }
    }


    // --- Funções Auxiliares (sem mudanças) ---

    public void AtualizarUI()
    {
        if (textoPontosDisponiveis != null)
        {
            textoPontosDisponiveis.text = "Pontos: " + pontosDisponiveis;
        }
    }

    public void AtualizarEstadoBotoes()
    {
        foreach (var botao in todosBotoesDaArvore)
        {
            if (botao != null)
            {
                botao.VerificarEstado(this);
            }
        }
    }
}