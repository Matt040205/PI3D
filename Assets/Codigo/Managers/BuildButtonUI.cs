using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildButtonUI : MonoBehaviour
{
    public GameObject buildButtonPrefab;

    [Header("Contêineres das Lojas")]
    public Transform towerButtonContainer;
    public Transform trapButtonContainer;

    [Header("Configuração do Prefab")]
    [Tooltip("O nome EXATO do GameObject 'Filho' que contém a imagem do ícone")]
    public string iconChildObjectName = "Icon"; // Coloque o nome do seu objeto filho aqui

    public void ClearTowerButtons()
    {
        if (towerButtonContainer == null) { Debug.LogError("DEBUG FALHA: 'Tower Button Container' está NULO!"); return; }

        foreach (Transform child in towerButtonContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void ClearTrapButtons()
    {
        if (trapButtonContainer == null) { Debug.LogError("DEBUG FALHA: 'Trap Button Container' está NULO!"); return; }

        foreach (Transform child in trapButtonContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void CreateTowerBuildButtons(List<CharacterBase> availableTowers)
    {
        if (towerButtonContainer == null) { Debug.LogError("DEBUG FALHA: 'Tower Button Container' está NULO!"); return; }
        if (buildButtonPrefab == null) { Debug.LogError("DEBUG FALHA: 'Build Button Prefab' está NULO!"); return; }

        foreach (CharacterBase towerData in availableTowers)
        {
            if (towerData == null) continue;
            GameObject buttonGO = Instantiate(buildButtonPrefab, towerButtonContainer);

            Button button = buttonGO.GetComponent<Button>();

            Image iconImage = null;
            Transform iconTransform = buttonGO.transform.Find(iconChildObjectName);

            if (iconTransform != null)
            {
                iconImage = iconTransform.GetComponent<Image>();
            }

            if (iconImage != null && towerData.characterIcon != null)
            {
                iconImage.sprite = towerData.characterIcon;
                iconImage.enabled = true;
            }
            else
            {
                Debug.LogWarning($"Não foi possível encontrar o 'Image' no objeto filho chamado '{iconChildObjectName}' no prefab {buttonGO.name}", buttonGO);
            }

            if (button != null)
            {
                button.onClick.AddListener(() => {
                    BuildManager.Instance.SelectTowerToBuild(towerData);
                });
            }
        }
    }

    public void CreateTrapBuildButtons(List<TrapDataSO> availableTraps)
    {
        if (trapButtonContainer == null) { Debug.LogError("DEBUG FALHA: 'Trap Button Container' está NULO!"); return; }
        if (buildButtonPrefab == null) { Debug.LogError("DEBUG FALHA: 'Build Button Prefab' está NULO!"); return; }

        foreach (TrapDataSO trapData in availableTraps)
        {
            if (trapData == null) continue;
            GameObject buttonGO = Instantiate(buildButtonPrefab, trapButtonContainer);

            Button button = buttonGO.GetComponent<Button>();

            Image iconImage = null;
            Transform iconTransform = buttonGO.transform.Find(iconChildObjectName);

            if (iconTransform != null)
            {
                iconImage = iconTransform.GetComponent<Image>();
            }

            if (iconImage != null && trapData.icon != null)
            {
                iconImage.sprite = trapData.icon;
                iconImage.enabled = true;
            }
            else
            {
                Debug.LogWarning($"Não foi possível encontrar o 'Image' no objeto filho chamado '{iconChildObjectName}' no prefab {buttonGO.name}", buttonGO);
            }

            if (button != null)
            {
                button.onClick.AddListener(() => {
                    BuildManager.Instance.SelectTrapToBuild(trapData);
                });
            }
        }
    }
}