using UnityEngine;

public class PauseControl : MonoBehaviour
{
    [Header("Menu de Pausa")]
    // Referência para o GameObject do Canvas de pausa
    public GameObject pauseMenuCanvas;

    private bool isPaused = false;

    void Start()
    {
        // Garante que o menu de pausa esteja inativo no início
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);
        }

        // Garante que o jogo comece com o cursor oculto e travado
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Oculta o cursor e trava-o ao clicar com o botão esquerdo do mouse
        if (Input.GetMouseButtonDown(0) && !isPaused)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Verifica se a tecla "ESC" foi pressionada
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                // Se o jogo está pausado, despausa-o
                ResumeGame();
            }
            else
            {
                // Se o jogo não está pausado, pausa-o
                PauseGame();
            }
        }
    }

    // Método para pausar o jogo
    public void PauseGame()
    {
        if (pauseMenuCanvas != null)
        {
            // Ativa o Canvas do menu de pausa
            pauseMenuCanvas.SetActive(true);
        }

        // Pausa o tempo do jogo
        Time.timeScale = 0f;

        // Mostra e destrava o cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        isPaused = true;
    }

    // Método para despausar o jogo
    public void ResumeGame()
    {
        if (pauseMenuCanvas != null)
        {
            // Desativa o Canvas do menu de pausa
            pauseMenuCanvas.SetActive(false);
        }

        // Restaura o tempo normal do jogo
        Time.timeScale = 1f;

        // Esconde e trava o cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isPaused = false;
    }
}