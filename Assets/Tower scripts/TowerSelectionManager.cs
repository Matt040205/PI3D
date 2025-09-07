// TowerSelectionManager.cs (VERS�O DE DEPURA��O)
using UnityEngine;

public class TowerSelectionManager : MonoBehaviour
{
    public static TowerSelectionManager Instance;

    [Header("Refer�ncias da UI")]
    public UpgradePanelUI upgradePanel;

    private TowerController selectedTower;
    private Camera mainCamera; // Adicionado para otimiza��o

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        mainCamera = Camera.main; // Guarda a refer�ncia da c�mera no in�cio
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("--- Clique do mouse detectado ---");

            if (mainCamera == null)
            {
                Debug.LogError("C�mera principal n�o encontrada! O Raycast n�o vai funcionar.");
                return;
            }

            if (upgradePanel == null)
            {
                Debug.LogError("Refer�ncia do UpgradePanel n�o est� definida no TowerSelectionManager!");
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
                    Debug.Log($"<color=green>Objeto '{hit.collider.name}' � uma torre! Chamando SelectTower...</color>");
                    SelectTower(tower);
                }
                else
                {
                    Debug.Log("<color=orange>Objeto atingido n�o � uma torre. Deselecionando.</color>");
                    DeselectTower();
                }
            }
            else
            {
                Debug.Log("<color=red>Clique n�o atingiu nenhum objeto com Collider. Deselecionando.</color>");
                DeselectTower();
            }
        }
    }

    void SelectTower(TowerController tower)
    {
        Debug.Log($"L�gica SelectTower iniciada. Torre clicada: {tower.name}, Torre j� selecionada: {(selectedTower != null ? selectedTower.name : "Nenhuma")}");

        if (tower == selectedTower && upgradePanel.IsPanelVisible())
        {
            Debug.Log("Clicou na mesma torre com o painel vis�vel. Escondendo o painel.");
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