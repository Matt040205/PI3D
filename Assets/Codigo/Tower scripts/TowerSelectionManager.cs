using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TowerSelectionManager : MonoBehaviour
{
    public static TowerSelectionManager Instance;

    [Header("Referências da UI")]
    public UpgradePanelUI upgradePanel;

    [Header("Configuração da Seleção")]
    [Tooltip("A layer (camada) que as suas torres usam. O Raycast vai procurar por esta camada.")]
    public LayerMask towerLayerMask;

    private TowerController selectedTower;
    private TowerController towerCurrentlyHighlighted; // A torre que está com o rato por cima
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
        // Se o jogo estiver pausado ou a câmera não existir, não faz nada
        if (Time.timeScale == 0 || mainCamera == null) return;

        HandleHoverHighlighting();
        HandleSelectionClick();
    }

    private void HandleHoverHighlighting()
    {
        // 1. Se o rato estiver sobre a UI (botões, etc.), desliga o highlight e sai
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

        // 2. Dispara o Raycast do Rato
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        TowerController towerHit = null;

        if (Physics.Raycast(ray, out hit, 1000f, towerLayerMask))
        {
            // Tenta encontrar o TowerController no objeto que o raio acertou
            towerHit = hit.collider.GetComponent<TowerController>();

            // Se não encontrou, talvez o colisor seja um filho (como o círculo)
            if (towerHit == null)
            {
                towerHit = hit.collider.GetComponentInParent<TowerController>();
            }
        }

        // 3. Compara o que o raio atingiu (towerHit) com o que estava em highlight
        if (towerHit != towerCurrentlyHighlighted)
        {
            // Se o rato mudou de alvo, desliga o highlight antigo
            if (towerCurrentlyHighlighted != null)
            {
                Debug.Log($"[Highlight] Rato saiu da torre ({towerCurrentlyHighlighted.name}).");
                towerCurrentlyHighlighted.GetComponent<TowerSelectionCircle>()?.Unhighlight();
            }

            // E liga o novo (se houver um)
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
            // Se o rato estiver sobre a UI, ignora o clique
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("[Clique] Clique ignorado (sobre a UI).");
                return;
            }

            // Se o clique foi no mundo E uma torre está em highlight:
            if (towerCurrentlyHighlighted != null)
            {
                Debug.Log($"[Clique] Clicou na torre em highlight: {towerCurrentlyHighlighted.name}");
                SelectTower(towerCurrentlyHighlighted);
            }
            else // Se o clique foi no mundo e NENHUMA torre estava em highlight:
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