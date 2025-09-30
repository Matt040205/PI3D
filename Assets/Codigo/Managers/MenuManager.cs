using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // A variável nomeDaCena não é mais necessária aqui, pois será passada diretamente pelo botão.
    // Removi: public string nomeDaCena;

    [Header("Menus")]
    public GameObject menuPanel;
    public GameObject optionsPanel;
    public GameObject creditosPanel;
    public GameObject sonsPanel;
    public GameObject PausePanel;
    public GameObject HUDPanel;

    void Start()
    {
        if (menuPanel != null) menuPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditosPanel != null) creditosPanel.SetActive(false);
        if (sonsPanel != null) sonsPanel.SetActive(false);
    }

    /// <summary>
    /// Carrega uma nova cena. O nome da cena é passado como parâmetro no evento OnClick do botão.
    /// </summary>
    /// <param name="nomeDaCena">O nome exato da cena a ser carregada (configurado no Inspector do botão).</param>
    public void ChangeScene(string nomeDaCena)
    {
        if (!string.IsNullOrEmpty(nomeDaCena))
        {
            // 1. Garante que o tempo volte ao normal.
            Time.timeScale = 1f;

            // 2. [NOVA LINHA] Garante que o jogo não se considere mais "pausado".
            PauseControl.isPaused = false;

            // 3. [BOA PRÁTICA] Garante que o cursor esteja visível para os menus.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            SceneManager.LoadScene(nomeDaCena);
        }
        else
        {
            Debug.LogError("O nome da cena não foi definido no Inspetor do botão que chamou ChangeScene!");
        }
    }

    public void Resume()
    {
        if (PausePanel != null)
        {
            PausePanel.SetActive(false);
            HUDPanel.SetActive(true);
        }

        Time.timeScale = 1f;

        PauseControl.isPaused = false;

        // --- MUDANÇA AQUI ---
        // Acessa a variável estática diretamente
        if (BuildManager.isBuildingMode)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void Options()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void Voltar()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(true);
    }

    public void Creditos()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditosPanel != null) creditosPanel.SetActive(true);
    }

    public void Sons()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (sonsPanel != null) sonsPanel.SetActive(true);
    }

    public void VoltarSons()
    {
        if (sonsPanel != null) sonsPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void VoltarCreditos()
    {
        if (creditosPanel != null) creditosPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Saiu do jogo.");
    }
}