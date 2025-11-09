using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
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

    public void ChangeScene(string nomeDaCena)
    {
        if (!string.IsNullOrEmpty(nomeDaCena))
        {
            Time.timeScale = 1f;
            PauseControl.isPaused = false;
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