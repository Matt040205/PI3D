using UnityEngine;

public class PauseControl : MonoBehaviour
{
    [Header("Gerenciador de UI")]
    public UIManager uiManager;

    // --- NOVA VARIÁVEL GLOBAL ---
    public static bool isPaused = false;

    // Remove Start para evitar conflito com UIManager.Start()
    void Start() { /* Vazio */ }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        if (uiManager != null) uiManager.ShowPauseMenu(true);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // --- ATUALIZAÇÃO AQUI ---
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (uiManager != null) uiManager.ShowPauseMenu(false);

        Time.timeScale = 1f;

        if (FindObjectOfType<BuildManager>() != null && FindObjectOfType<BuildManager>().isBuildingMode)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // --- ATUALIZAÇÃO AQUI ---
        isPaused = false;
    }
}