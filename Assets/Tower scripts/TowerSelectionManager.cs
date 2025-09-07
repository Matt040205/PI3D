// TowerSelectionManager.cs (VERSÃO DE DEPURAÇÃO)
using UnityEngine;

public class TowerSelectionManager : MonoBehaviour
{
    public static TowerSelectionManager Instance;

    [Header("Referências da UI")]
    public UpgradePanelUI upgradePanel;

    private TowerController selectedTower;
    private Camera mainCamera; // Adicionado para otimização

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        mainCamera = Camera.main; // Guarda a referência da câmera no início
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("--- Clique do mouse detectado ---");

            if (mainCamera == null)
            {
                Debug.LogError("Câmera principal não encontrada! O Raycast não vai funcionar.");
                return;
            }

            if (upgradePanel == null)
            {
                Debug.LogError("Referência do UpgradePanel não está definida no TowerSelectionManager!");
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                Debug.Log($"Raycast atingiu o objeto: '{hit.collider.name}' na layer '{LayerMask.LayerToName(hit.collider.gameObject.layer)}'");

                TowerController tower = hit.collider.GetComponent<TowerController>();

                if (tower != null)
                {
                    Debug.Log($"<color=green>Objeto '{hit.collider.name}' é uma torre! Chamando SelectTower...</color>");
                    SelectTower(tower);
                }
                else
                {
                    Debug.Log("<color=orange>Objeto atingido não é uma torre. Deselecionando.</color>");
                    DeselectTower();
                }
            }
            else
            {
                Debug.Log("<color=red>Clique não atingiu nenhum objeto com Collider. Deselecionando.</color>");
                DeselectTower();
            }
        }
    }

    void SelectTower(TowerController tower)
    {
        Debug.Log($"Lógica SelectTower iniciada. Torre clicada: {tower.name}, Torre já selecionada: {(selectedTower != null ? selectedTower.name : "Nenhuma")}");

        if (tower == selectedTower && upgradePanel.IsPanelVisible())
        {
            Debug.Log("Clicou na mesma torre com o painel visível. Escondendo o painel.");
            DeselectTower();
        }
        else
        {
            Debug.Log("Selecionando nova torre ou mostrando painel para a torre atual.");
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