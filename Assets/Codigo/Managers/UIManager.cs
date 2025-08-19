using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Painéis de UI")]
    public GameObject hudPanel;
    public GameObject pausePanel;
    public GameObject buildPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Garante que o HUD esteja ativo e os outros desativados no início
        ShowHUD();
    }

    // Gerencia a visibilidade do HUD
    public void ShowHUD()
    {
        if (hudPanel != null) hudPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (buildPanel != null) buildPanel.SetActive(false);
    }

    // Gerencia a visibilidade do Menu de Pausa
    public void ShowPauseMenu(bool show)
    {
        if (pausePanel != null) pausePanel.SetActive(show);

        if (show)
        {
            // Se o menu de pausa está ativo, desativa o HUD
            if (hudPanel != null) hudPanel.SetActive(false);
        }
        else
        {
            // Se o menu de pausa foi desativado, retorna ao HUD ou modo de construção
            // Verifica se o BuildManager está no modo de construção
            if (FindObjectOfType<BuildManager>() != null && FindObjectOfType<BuildManager>().isBuildingMode)
            {
                ShowBuildUI(true); // Se estava construindo, reativa a UI de construção
            }
            else
            {
                ShowHUD(); // Caso contrário, volta para o HUD
            }
        }
    }

    // Gerencia a visibilidade da UI de Construção
    public void ShowBuildUI(bool show)
    {
        if (buildPanel != null) buildPanel.SetActive(show);

        if (show)
        {
            // Se a UI de construção está ativa, desativa o HUD
            if (hudPanel != null) hudPanel.SetActive(false);
        }
        else
        {
            // Se a UI de construção foi desativada, volta para o HUD
            ShowHUD();
        }
    }
}