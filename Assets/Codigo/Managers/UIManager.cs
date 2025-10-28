using UnityEngine;
using UnityEngine.UI;
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

    [Header("HUD da Base")]
    public ObjectiveHealthSystem objectiveHealthSystem;
    public TextMeshProUGUI objectiveHealthText;
    public Image objectiveHealthBar;

    private float gameTime = 0f;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        ShowHUD();

        if (objectiveHealthSystem != null)
        {
            objectiveHealthSystem.OnHealthChanged += UpdateObjectiveHealthUI;
            UpdateObjectiveHealthUI();
        }
        else
        {
            Debug.LogError("UIManager: A referência do ObjectiveHealthSystem não foi definida no Inspector!");
        }
    }

    void Update()
    {
        gameTime += Time.deltaTime;
        UpdateTimerDisplay(gameTime);
    }

    private void OnDestroy()
    {
        if (objectiveHealthSystem != null)
        {
            objectiveHealthSystem.OnHealthChanged -= UpdateObjectiveHealthUI;
        }
    }

    public void UpdateObjectiveHealthUI()
    {
        if (objectiveHealthSystem == null) return;

        float currentHealth = objectiveHealthSystem.currentHealth;
        float maxHealth = objectiveHealthSystem.maxHealth;

        if (objectiveHealthText != null)
        {
            objectiveHealthText.text = $"{currentHealth:F0} / {maxHealth:F0}";
        }

        if (objectiveHealthBar != null)
        {
            if (maxHealth > 0)
            {
                objectiveHealthBar.fillAmount = currentHealth / maxHealth;
            }
            else
            {
                objectiveHealthBar.fillAmount = 0;
            }
        }
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