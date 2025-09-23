// TowerSelectionManager.cs
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TowerSelectionManager : MonoBehaviour
{
    public static TowerSelectionManager Instance;

    [Header("Referências da UI")]
    public UpgradePanelUI upgradePanel; // Referência à script do painel de upgrade

    [Header("Configuração da Seleção")]
    public LayerMask towerLayerMask;

    private TowerController selectedTower;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("<color=yellow>Clique ignorado pois foi feito sobre a UI.</color>");
                return;
            }

            Camera currentCamera = Camera.main;
            if (currentCamera == null)
            {
                Debug.LogError("Câmera principal não encontrada!");
                return;
            }

            Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f, towerLayerMask))
            {
                Debug.Log($"<color=cyan>Raycast atingiu o objeto '{hit.collider.name}' na camada '{LayerMask.LayerToName(hit.collider.gameObject.layer)}'</color>");

                TowerController tower = hit.collider.GetComponent<TowerController>();

                if (tower != null)
                {
                    Debug.Log($"<color=green>Objeto '{hit.collider.name}' é uma torre! Chamando SelectTower...</color>");
                    SelectTower(tower);
                }
            }
            else
            {
                Debug.Log("<color=red>Clique não atingiu nenhuma torre. Deselecionando.</color>");
                DeselectTower();
            }
        }
    }

    void SelectTower(TowerController tower)
    {
        if (tower == selectedTower && upgradePanel.IsPanelVisible())
        {
            DeselectTower();
        }
        else
        {
            selectedTower = tower;
            // AQUI ESTÁ A LÓGICA: SÓ MOSTRAR O PAINEL DE UPGRADE SE O CLIQUE FOR EM UMA TORRE.
            if (upgradePanel != null)
            {
                upgradePanel.ShowPanel(selectedTower);
            }
        }
    }

    public void DeselectTower()
    {
        if (selectedTower != null || (upgradePanel != null && upgradePanel.IsPanelVisible()))
        {
            Debug.Log("Deselecionando torre e escondendo painel.");
            selectedTower = null;
            // AQUI ESTÁ A LÓGICA: ESCONDER O PAINEL DE UPGRADE.
            if (upgradePanel != null)
            {
                upgradePanel.HidePanel();
            }
        }
    }
}