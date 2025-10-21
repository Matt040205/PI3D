// Arquivo: Rastros.cs
// Esta script gerencia o estado geral da árvore de habilidades.

using UnityEngine;
using System.Collections.Generic; // Necessário para Dictionaries e HashSets
using TMPro;

public class Rastros : MonoBehaviour
{
    [Header("Pontos do Jogador")]
    public int pontosDisponiveis = 10; // Pontos que o jogador TEM para gastar
    public int pontosGastosGlobal = 0; // A "variável" global que você mencionou (total de habilidades compradas)

    // Dicionário para rastrear quantos pontos foram gastos em CADA caminho
    // Ex: {"Caminho1", 2}, {"Caminho2", 1}
    public Dictionary<string, int> pontosPorCaminho = new Dictionary<string, int>();

    // HashSet para rastrear QUAIS habilidades específicas foram desbloqueadas
    // Armazena o "idHabilidade" de cada botão comprado
    public HashSet<string> habilidadesDesbloqueadas = new HashSet<string>();

    [Header("Referências da UI")]
    // Arraste aqui todos os GameObjects dos botões da árvore
    public List<BotaoHabilidade> todosBotoesDaArvore;

    // (Opcional) Arraste um componente de Texto para mostrar os pontos
    public TextMeshProUGUI textoPontosDisponiveis;

    void Start()
    {
        // Inicializa os caminhos no dicionário para evitar erros
        // Adicione os IDs de todos os seus caminhos aqui
        // O "Central" é o seu botão inicial
        if (!pontosPorCaminho.ContainsKey("Central")) pontosPorCaminho.Add("Central", 0);
        if (!pontosPorCaminho.ContainsKey("Caminho1")) pontosPorCaminho.Add("Caminho1", 0);
        if (!pontosPorCaminho.ContainsKey("Caminho2")) pontosPorCaminho.Add("Caminho2", 0);
        if (!pontosPorCaminho.ContainsKey("Caminho3")) pontosPorCaminho.Add("Caminho3", 0);
        if (!pontosPorCaminho.ContainsKey("Caminho4")) pontosPorCaminho.Add("Caminho4", 0);

        // Atualiza a UI e o estado visual de todos os botões no início
        AtualizarUI();
        AtualizarEstadoBotoes();
    }

    /// <summary>
    /// Função principal, chamada pelo BotaoHabilidade quando clicado.
    /// </summary>
    public bool TentarDesbloquear(BotaoHabilidade botao)
    {
        // 1. Já foi desbloqueada?
        if (habilidadesDesbloqueadas.Contains(botao.idHabilidade))
        {
            Debug.Log("Habilidade " + botao.idHabilidade + " já foi desbloqueada.");
            return false;
        }

        // 2. Tem pontos disponíveis para gastar?
        if (pontosDisponiveis < botao.custo)
        {
            Debug.Log("Pontos insuficientes. Necessário: " + botao.custo + ", Disponível: " + pontosDisponiveis);
            return false;
        }

        // 3. Tem o pré-requisito (o botão anterior)?
        // Verifica se o pré-requisito está vazio (botão central) OU se já foi desbloqueado
        if (!string.IsNullOrEmpty(botao.preRequisitoHabilidade) && !habilidadesDesbloqueadas.Contains(botao.preRequisitoHabilidade))
        {
            Debug.Log("Pré-requisito " + botao.preRequisitoHabilidade + " não foi desbloqueado.");
            return false;
        }

        // 4. Tem os pontos GLOBAIS necessários?
        // (Sua lógica de "precisa de um valor 1" para os botões do Tier 1)
        if (pontosGastosGlobal < botao.pontosGlobaisNecessarios)
        {
            Debug.Log("Pontos globais gastos insuficientes. Necessário: " + botao.pontosGlobaisNecessarios + ", Atual: " + pontosGastosGlobal);
            return false;
        }

        // 5. Tem os pontos DE CAMINHO necessários?
        // (Sua lógica de "pontos necessários" no caminho específico)
        int pontosAtuaisNesteCaminho = pontosPorCaminho.ContainsKey(botao.idCaminho) ? pontosPorCaminho[botao.idCaminho] : 0;

        if (pontosAtuaisNesteCaminho < botao.pontosNoCaminhoNecessarios)
        {
            Debug.Log("Pontos no caminho " + botao.idCaminho + " insuficientes. Necessário: " + botao.pontosNoCaminhoNecessarios + ", Atual: " + pontosAtuaisNesteCaminho);
            return false;
        }

        // --- SUCESSO! Desbloqueia a habilidade ---

        // 1. Gasta o ponto
        pontosDisponiveis -= botao.custo;

        // 2. "adiciona 1 a uma variável" (global)
        pontosGastosGlobal += 1;

        // 3. "adiciona 1... num caminho diferente" (específico do caminho)
        pontosPorCaminho[botao.idCaminho] = pontosAtuaisNesteCaminho + 1;

        // 4. Marca a habilidade como desbloqueada
        habilidadesDesbloqueadas.Add(botao.idHabilidade);

        // 5. (TODO) Adicione aqui a lógica para APLICAR O EFEITO da habilidade
        // Ex: GetComponent<PlayerStatus>().AumentarDano(10);
        Debug.Log("Habilidade " + botao.idHabilidade + " DESBLOQUEADA!");

        // 6. Atualiza a UI de pontos e o estado visual de todos os botões
        AtualizarUI();
        AtualizarEstadoBotoes();
        return true;
    }

    // --- Funções Auxiliares ---

    public void AtualizarUI()
    {
        if (textoPontosDisponiveis != null)
        {
            textoPontosDisponiveis.text = "Pontos: " + pontosDisponiveis;
        }
    }

    // Passa por todos os botões e atualiza sua aparência (bloqueado, disponível, comprado)
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

    // TODO: Adicionar funções de Save/Load para salvar os
    // 'habilidadesDesbloqueadas', 'pontosDisponiveis', 'pontosGastosGlobal' e 'pontosPorCaminho'
}