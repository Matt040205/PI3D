// Crie um novo arquivo: BotaoHabilidade.cs
// Esta script deve ser colocada em CADA botão da sua árvore.

using UnityEngine;
using UnityEngine.UI; // Necessário para Button e Image

[RequireComponent(typeof(Button))] // Garante que o GameObject tenha um componente Button
public class BotaoHabilidade : MonoBehaviour
{
    [Header("Identificação da Habilidade")]
    public string idHabilidade; // ID único, ex: "Central", "Caminho1_Nivel1"
    public string idCaminho;      // ID do caminho, ex: "Central", "Caminho1"

    [Header("Requisitos para Desbloquear")]
    public int custo = 1; // Custo em pontos de habilidade

    // O ID da habilidade ANTERIOR necessária (deixe em branco se for o central)
    public string preRequisitoHabilidade;

    // Quantos pontos GLOBAIS gastos são necessários (o "valor 1" para os T1)
    public int pontosGlobaisNecessarios = 0;

    // Quantos pontos já gastos NESTE CAMINHO são necessários
    public int pontosNoCaminhoNecessarios = 0;

    [Header("Referências")]
    // Arraste o GameObject que tem a script "Rastros.cs" aqui
    public Rastros managerRastros;
    private Button meuBotao;

    // (Opcional) Arraste a imagem do botão para mudar a cor
    public Image iconeHabilidade;

    [Header("Cores de Estado")]
    public Color corBloqueado = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Cinza
    public Color corDisponivel = Color.white; // Branco
    public Color corDesbloqueado = Color.green; // Verde

    void Start()
    {
        meuBotao = GetComponent<Button>();

        // Adiciona um "listener" ao botão para chamar nossa função quando clicado
        meuBotao.onClick.AddListener(OnBotaoClicado);

        if (managerRastros == null)
        {
            // Tenta encontrar o manager na cena se não foi arrastado
            managerRastros = FindObjectOfType<Rastros>();
        }

        // Adiciona este botão à lista do manager (se ainda não estiver)
        if (managerRastros != null && !managerRastros.todosBotoesDaArvore.Contains(this))
        {
            managerRastros.todosBotoesDaArvore.Add(this);
        }
    }

    void OnEnable()
    {
        // Garante que o botão atualize quando o painel for ativado
        if (managerRastros != null)
        {
            VerificarEstado(managerRastros);
        }
    }

    // Chamado quando o botão da UI é clicado
    void OnBotaoClicado()
    {
        // Tenta desbloquear a habilidade através do manager
        managerRastros.TentarDesbloquear(this);

        // O managerRastros.TentarDesbloquear já vai chamar a atualização
        // de todos os botões (incluindo este) se for bem-sucedido.
    }

    /// <summary>
    /// Verifica e atualiza a aparência do botão com base no estado da árvore.
    /// Esta função é chamada pelo manager (Rastros.cs).
    /// </summary>
    public void VerificarEstado(Rastros manager)
    {
        if (manager.habilidadesDesbloqueadas.Contains(idHabilidade))
        {
            // --- 1. HABILIDADE JÁ DESBLOQUEADA ---
            if (iconeHabilidade != null) iconeHabilidade.color = corDesbloqueado;
            meuBotao.interactable = false; // Já comprou, não pode clicar de novo
        }
        else
        {
            // --- 2. HABILIDADE BLOQUEADA ---
            // Verifica se PODE ser comprada (está disponível)
            bool disponivel = true;

            // Checa todas as condições
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
                // --- 3. DISPONÍVEL PARA COMPRA ---
                if (iconeHabilidade != null) iconeHabilidade.color = corDisponivel;
                meuBotao.interactable = true;
            }
            else
            {
                // --- 4. BLOQUEADA (AINDA NÃO DISPONÍVEL) ---
                if (iconeHabilidade != null) iconeHabilidade.color = corBloqueado;
                meuBotao.interactable = false;
            }
        }
    }
}