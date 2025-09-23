using UnityEngine;

public class PauseControl : MonoBehaviour
{
    [Header("Gerenciador de UI")]
    public UIManager uiManager;

    public static bool isPaused = false;

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

        isPaused = true;
    }

    public void ResumeGame()
    {
        if (uiManager != null) uiManager.ShowPauseMenu(false);

        Time.timeScale = 1f;

        // --- MUDANÇA AQUI ---
        // Acessando isBuildingMode com o nome da classe, não com uma instância
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

        isPaused = false;
    }
}