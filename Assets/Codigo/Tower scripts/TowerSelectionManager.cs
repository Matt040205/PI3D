// TowerSelectionManager.cs (Nova Versão com LayerMask)
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TowerSelectionManager : MonoBehaviour
{
    public static TowerSelectionManager Instance;

    [Header("Referências da UI")]
    public UpgradePanelUI upgradePanel;

    [Header("Configuração da Seleção")]
    public LayerMask towerLayerMask; // NOVO: Campo para definir a camada das torres

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
                // Este log ajuda a confirmar se a UI está bloqueando o clique.
                // Se esta mensagem aparece, o problema é um painel da UI com "Raycast Target" ativado.
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

            // MUDANÇA PRINCIPAL AQUI:
            // Usamos uma distância maior (1000f) e a towerLayerMask para garantir
            // que o raio só vai procurar por colisões na camada "Towers".
            if (Physics.Raycast(ray, out hit, 1000f, towerLayerMask))
            {
                Debug.Log($"<color=cyan>Raycast atingiu o objeto '{hit.collider.name}' na camada '{LayerMask.LayerToName(hit.collider.gameObject.layer)}'</color>");

                TowerController tower = hit.collider.GetComponent<TowerController>();

                if (tower != null)
                {
                    Debug.Log($"<color=green>Objeto '{hit.collider.name}' é uma torre! Chamando SelectTower...</color>");
                    SelectTower(tower);
                }
                // Não precisamos mais de um 'else' aqui, pois o raycast só vai atingir torres.
            }
            else
            {
                // Se o raycast não atingiu NADA na camada de torres, então deselecionamos.
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
            upgradePanel.ShowPanel(selectedTower);
        }
    }

    void DeselectTower()
    {
        if (selectedTower != null || upgradePanel.IsPanelVisible())
        {
            Debug.Log("Deselecionando torre e escondendo painel.");
            selectedTower = null;
            upgradePanel.HidePanel();
        }
    }
}