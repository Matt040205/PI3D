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

    [Header("Mapeamento de Ícones")]
    [Tooltip("Arraste o ScriptableObject 'Banco de Icones de Status' aqui.")]
    public StatIconDatabase iconDatabase;

    private CharacterBase personagemSendoUprado;
    private BotaoHabilidade habilidadeSelecionada;

    void Start()
    {
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

        AtualizarIconesBotoes();

        ReaplicarUpgradesSalvos();

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

    void AtualizarIconesBotoes()
    {
        if (iconDatabase == null)
        {
            Debug.LogWarning("Nenhum Banco de Ícones (StatIconDatabase) foi assignado no script Rastros.");
            return;
        }

        foreach (var botao in todosBotoesDaArvore)
        {
            if (botao == null || botao.iconeHabilidade == null) continue;

            RastroUpgrade upgrade = botao.GetUpgradeParaPersonagem(personagemSendoUprado);

            if (upgrade != null && upgrade.modifiers.Count > 0)
            {
                CharacterStatType statPrincipal = upgrade.modifiers[0].statToModify;
                Sprite icon = iconDatabase.GetIconForStat(statPrincipal);

                if (icon != null)
                {
                    botao.iconeHabilidade.sprite = icon;
                }
                else
                {
                    Debug.LogWarning($"Ícone para o status {statPrincipal} não encontrado no Database (Botão: {botao.idHabilidade}).");
                }
            }
            else
            {
                botao.iconeHabilidade.enabled = false;
            }
        }
    }

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

        RastroUpgrade upgradeCorreto = habilidadeSelecionada.GetUpgradeParaPersonagem(personagemSendoUprado);

        if (upgradeCorreto != null)
        {
            textoDescricaoHabilidade.text = $"<b>{upgradeCorreto.upgradeName}</b>\n\n{upgradeCorreto.description}";
        }
        else
        {
            textoDescricaoHabilidade.text = $"Habilidade não configurada para {personagemSendoUprado.name}.";
        }

        bool podeComprar = ChecarCondicoes(habilidadeSelecionada);

        botaoConfirmarCompra.interactable = podeComprar && (upgradeCorreto != null);
    }

    public void ConfirmarCompraHabilidade()
    {
        if (habilidadeSelecionada == null) return;

        bool sucesso = TentarDesbloquear(habilidadeSelecionada);

        if (painelDescricao != null) painelDescricao.SetActive(false);
        habilidadeSelecionada = null;
    }

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

    private bool TentarDesbloquear(BotaoHabilidade botao)
    {
        if (!ChecarCondicoes(botao))
        {
            return false;
        }

        RastroUpgrade upgradeCorreto = botao.GetUpgradeParaPersonagem(personagemSendoUprado);

        if (upgradeCorreto == null)
        {
            Debug.LogError($"Tentativa de comprar {botao.idHabilidade} falhou: Upgrade não mapeado para {personagemSendoUprado.name}!");
            return false;
        }

        pontosDisponiveis -= botao.custo;
        pontosGastosGlobal += 1;
        pontosPorCaminho[botao.idCaminho] = pontosPorCaminho[botao.idCaminho] + 1;
        habilidadesDesbloqueadas.Add(botao.idHabilidade);

        AplicarUpgrade(upgradeCorreto);
        Debug.Log("Upgrade " + upgradeCorreto.upgradeName + " aplicado!");

        AtualizarUI();
        AtualizarEstadoBotoes();
        return true;
    }

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
            }
        }
    }

    private void ReaplicarUpgradesSalvos()
    {
        Debug.Log("Reaplicando " + habilidadesDesbloqueadas.Count + " upgrades salvos...");
        foreach (var botao in todosBotoesDaArvore)
        {
            if (botao != null && habilidadesDesbloqueadas.Contains(botao.idHabilidade))
            {
                RastroUpgrade upgradeCorreto = botao.GetUpgradeParaPersonagem(personagemSendoUprado);

                if (upgradeCorreto != null)
                {
                    AplicarUpgrade(upgradeCorreto);
                }
            }
        }
    }

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