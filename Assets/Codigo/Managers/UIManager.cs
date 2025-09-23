using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject hudPanel;
    public GameObject pausePanel;
    public GameObject buildPanel;
    public BuildButtonUI buildButtonUI;

    [Header("Elementos de HUD")]
    public TextMeshProUGUI timerText;

    // Adicione esta variável para controlar o tempo.
    private float gameTime = 0f;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        ShowHUD();
    }

    // Adicione a função Update() para atualizar o temporizador a cada frame.
    void Update()
    {
        gameTime += Time.deltaTime;
        UpdateTimerDisplay(gameTime);
    }

    public void UpdateBuildUI(List<CharacterBase> availableTowers)
    {
        if (buildButtonUI != null)
        {
            buildButtonUI.CreateBuildButtons(availableTowers);
        }
        else
        {
            Debug.LogError("DEBUG FALHA: A variável 'buildButtonUI' no UIManager está NULA!");
        }
    }

    public void ShowHUD()
    {
        if (hudPanel != null) hudPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (buildPanel != null) buildPanel.SetActive(false);
    }

    public void ShowPauseMenu(bool show)
    {
        if (pausePanel != null) pausePanel.SetActive(show);
        if (show)
        {
            if (hudPanel != null) hudPanel.SetActive(false);
            // Ao pausar, defina a escala de tempo para 0.
            Time.timeScale = 0f;
        }
        else
        {
            if (BuildManager.isBuildingMode)
            {
                ShowBuildUI(true);
            }
            else
            {
                ShowHUD();
            }
            // Ao despausar, retorne a escala de tempo para 1.
            Time.timeScale = 1f;
        }
    }

    public void ShowBuildUI(bool show)
    {
        if (buildPanel != null) buildPanel.SetActive(show);
        if (show)
        {
            if (hudPanel != null) hudPanel.SetActive(false);
        }
        else
        {
            ShowHUD();
        }
    }

    public void UpdateTimerDisplay(float timeInSeconds)
    {
        if (timerText == null)
        {
            Debug.LogError("DEBUG FALHA: A variável 'timerText' no UIManager está NULA! Certifique-se de atribuir o TextMeshProUGUI no inspetor da Unity.");
            return;
        }

        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}