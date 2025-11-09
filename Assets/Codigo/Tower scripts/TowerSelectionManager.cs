using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TowerSelectionManager : MonoBehaviour
{
    public static TowerSelectionManager Instance;

    [Header("Referências da UI")]
    public UpgradePanelUI upgradePanel;

    [Header("Configuração da Seleção")]
    public LayerMask towerLayerMask;

    private TowerController selectedTower;
    private TowerController towerCurrentlyHighlighted;
    private Camera mainCamera;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("[TowerSelectionManager] Câmera principal não encontrada!");
            this.enabled = false;
        }
    }

    void Update()
    {
        if (Time.timeScale == 0 || mainCamera == null) return;

        HandleHoverHighlighting();
        HandleSelectionClick();
    }

    private void HandleHoverHighlighting()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            if (towerCurrentlyHighlighted != null)
            {
                Debug.Log($"[Highlight] Rato saiu da torre ({towerCurrentlyHighlighted.name}) para a UI.");
                towerCurrentlyHighlighted.GetComponent<TowerSelectionCircle>()?.Unhighlight();
                towerCurrentlyHighlighted = null;
            }
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        TowerController towerHit = null;

        if (Physics.Raycast(ray, out hit, 1000f, towerLayerMask))
        {
            towerHit = hit.collider.GetComponent<TowerController>();

            if (towerHit == null)
            {
                towerHit = hit.collider.GetComponentInParent<TowerController>();
            }
        }

        if (towerHit != towerCurrentlyHighlighted)
        {
            if (towerCurrentlyHighlighted != null)
            {
                Debug.Log($"[Highlight] Rato saiu da torre ({towerCurrentlyHighlighted.name}).");
                towerCurrentlyHighlighted.GetComponent<TowerSelectionCircle>()?.Unhighlight();
            }

            if (towerHit != null)
            {
                Debug.Log($"[Highlight] Rato entrou na torre ({towerHit.name}).");
                towerHit.GetComponent<TowerSelectionCircle>()?.Highlight();
            }

            towerCurrentlyHighlighted = towerHit;
        }
    }

    private void HandleSelectionClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("[Clique] Clique ignorado (sobre a UI).");
                return;
            }

            if (towerCurrentlyHighlighted != null)
            {
                Debug.Log($"[Clique] Clicou na torre em highlight: {towerCurrentlyHighlighted.name}");
                SelectTower(towerCurrentlyHighlighted);
            }
            else
            {
                Debug.Log("[Clique] Clicou no chão. Deselecionando.");
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
            if (upgradePanel != null)
            {
                upgradePanel.ShowPanel(selectedTower);
            }

            if (BuildManager.Instance != null)
            {
                BuildManager.Instance.ClearSelection();
            }
        }
    }

    public void DeselectTower()
    {
        if (selectedTower != null || (upgradePanel != null && upgradePanel.IsPanelVisible()))
        {
            selectedTower = null;
            if (upgradePanel != null)
            {
                upgradePanel.HidePanel();
            }
        }
    }
}