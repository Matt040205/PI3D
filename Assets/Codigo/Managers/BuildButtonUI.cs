using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildButtonUI : MonoBehaviour
{
    public GameObject buildButtonPrefab;
    public Transform buttonContainer;

    public void CreateBuildButtons(List<CharacterBase> availableTowers)
    {
       // Debug.Log("--- DEBUG: BuildButtonUI.CreateBuildButtons() foi chamado. ---");

        if (buttonContainer == null) { Debug.LogError("DEBUG FALHA: 'Button Container' está NULO!"); return; }
        if (buildButtonPrefab == null) { Debug.LogError("DEBUG FALHA: 'Build Button Prefab' está NULO!"); return; }

        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        //Debug.Log("DEBUG: Criando " + availableTowers.Count + " botões...");
        foreach (CharacterBase towerData in availableTowers)
        {
            if (towerData == null) continue;
            GameObject buttonGO = Instantiate(buildButtonPrefab, buttonContainer);
           // Debug.Log("DEBUG: Botão criado para " + towerData.name);

            // O resto do script...
            Image buttonImage = buttonGO.GetComponent<Image>();
            Button button = buttonGO.GetComponent<Button>();
            if (buttonImage != null && towerData.characterIcon != null)
            {
                buttonImage.sprite = towerData.characterIcon;
            }
            if (button != null)
            {
                button.onClick.AddListener(() => {
                    BuildManager.Instance.SelectTowerToBuild(towerData);
                });
            }
        }
    }
}