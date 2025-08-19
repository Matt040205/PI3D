using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Menus")]
    // Refer�ncias para os GameObjects de cada tela do menu
    public GameObject menuPanel;
    public GameObject optionsPanel;
    public GameObject creditosPanel;
    public GameObject sonsPanel;

    void Start()
    {
        // Garante que apenas o painel principal do menu esteja ativo no in�cio
        if (menuPanel != null) menuPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditosPanel != null) creditosPanel.SetActive(false);
        if (sonsPanel != null) sonsPanel.SetActive(false);
    }

    // --- Fun��es de Navega��o ---

    // Inicia o jogo, carregando a cena "EscolherPersonagem"
    public void StartGame()
    {
        SceneManager.LoadScene("EscolherPersonagem");
    }

    // Navega para o menu de Op��es
    public void Options()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    // Volta do menu de Op��es para o Menu Principal
    public void Voltar()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(true);
    }

    // Navega para a tela de Cr�ditos
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

    // Volta da tela de Sons para o menu de Op��es
    public void VoltarSons()
    {
        if (sonsPanel != null) sonsPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    // Volta da tela de Cr�ditos para o menu de Op��es
    public void VoltarCreditos()
    {
        if (creditosPanel != null) creditosPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void QuitGame()
    {
        // Fecha a aplica��o
        Application.Quit();

        // Linha apenas para o Editor do Unity,
        // para saber que a fun��o foi chamada
        Debug.Log("Saiu do jogo.");
    }
}