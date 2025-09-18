using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Configuração da Cena")]
    // Variável pública para você arrastar ou digitar o nome da cena no Inspetor
    public string nomeDaCena;

    [Header("Menus")]
    // Referências para os GameObjects de cada tela do menu
    public GameObject menuPanel;
    public GameObject optionsPanel;
    public GameObject creditosPanel;
    public GameObject sonsPanel;

    void Start()
    {
        // Garante que apenas o painel principal do menu esteja ativo no início
        if (menuPanel != null) menuPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditosPanel != null) creditosPanel.SetActive(false);
        if (sonsPanel != null) sonsPanel.SetActive(false);
    }

    // --- Funções de Navegação ---

    // Inicia o jogo, carregando a cena definida na variável 'nomeDaCena'
    public void ChangeScene()
    {
        // Verifica se um nome de cena foi definido no inspetor para evitar erros
        if (!string.IsNullOrEmpty(nomeDaCena))
        {
            SceneManager.LoadScene(nomeDaCena);
        }
        else
        {
            // Se nenhum nome de cena foi definido, exibe um erro no console do Unity
            Debug.LogError("O nome da cena não foi definido no Inspetor do MenuManager!");
        }
    }

    public void Resume()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    // Navega para o menu de Opções
    public void Options()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    // Volta do menu de Opções para o Menu Principal
    public void Voltar()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(true);
    }

    // Navega para a tela de Créditos
    public void Creditos()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditosPanel != null) creditosPanel.SetActive(true);
    }

    // Navega para a tela de Sons
    public void Sons()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (sonsPanel != null) sonsPanel.SetActive(true);
    }

    // Volta da tela de Sons para o menu de Opções
    public void VoltarSons()
    {
        if (sonsPanel != null) sonsPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    // Volta da tela de Créditos para o menu de Opções
    public void VoltarCreditos()
    {
        if (creditosPanel != null) creditosPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void QuitGame()
    {
        // Fecha a aplicação
        Application.Quit();

        // Linha apenas para o Editor do Unity,
        // para saber que a função foi chamada
        Debug.Log("Saiu do jogo.");
    }
}