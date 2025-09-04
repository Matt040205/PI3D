using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject hudPanel;
    public GameObject pausePanel;
    public GameObject buildPanel;
    public BuildButtonUI buildButtonUI;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        ShowHUD();
    }

    public void UpdateBuildUI(List<CharacterBase> availableTowers)
    {
        Debug.Log("--- DEBUG: UIManager.UpdateBuildUI() foi chamado. ---");
        if (buildButtonUI != null)
        {
            Debug.Log("DEBUG: buildButtonUI encontrado. Chamando CreateBuildButtons...");
            buildButtonUI.CreateBuildButtons(availableTowers);
        }
        else
        {
            Debug.LogError("DEBUG FALHA: A variável 'buildButtonUI' no UIManager está NULA!");
        }
    }

    // O resto do seu script...
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
        }
        else
        {
            if (BuildManager.Instance != null && BuildManager.Instance.isBuildingMode)
            {
                ShowBuildUI(true);
            }
            else
            {
                ShowHUD();
            }
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
}