// BuildManager.cs (Com depuração e nova lógica de altura)
using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    [Header("Câmeras")]
    public CinemachineCamera buildCamera;

    private List<CharacterBase> availableTowers = new List<CharacterBase>();
    private CharacterBase selectedTowerData;

    [Header("Visual do Ghost")]
    private GameObject currentBuildGhost;
    public Material validPlacementMaterial;
    public Material invalidPlacementMaterial;

    public bool isBuildingMode = false;

    private const int PriorityBuild = 20;
    private const int PriorityInactive = 0;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        buildCamera.Priority.Value = PriorityInactive;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetAvailableTowers(CharacterBase[] selectedTeam)
    {
        availableTowers.Clear();
        for (int i = 1; i < selectedTeam.Length; i++)
        {
            if (selectedTeam[i] != null)
            {
                availableTowers.Add(selectedTeam[i]);
            }
        }
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateBuildUI(availableTowers);
        }
    }

    public void SelectTowerToBuild(CharacterBase towerData)
    {
        ClearSelection();
        selectedTowerData = towerData;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isBuildingMode = !isBuildingMode;
            ToggleBuildMode(isBuildingMode);
        }

        if (isBuildingMode)
        {
            HandleBuildGhost();
            if (Input.GetMouseButtonDown(0))
            {
                PlaceBuilding();
            }
        }
    }

    void ToggleBuildMode(bool state)
    {
        buildCamera.Priority.Value = state ? PriorityBuild : PriorityInactive;
        if (!state)
        {
            ClearSelection();
        }
        UIManager.Instance.ShowBuildUI(state);
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;
    }

    void HandleBuildGhost()
    {
        if (selectedTowerData == null)
        {
            if (currentBuildGhost != null) Destroy(currentBuildGhost);
            return;
        }

        GameObject selectedPrefab = selectedTowerData.towerPrefab;
        if (selectedPrefab == null)
        {
            if (currentBuildGhost != null) Destroy(currentBuildGhost);
            return;
        }

        if (currentBuildGhost == null)
        {
            currentBuildGhost = Instantiate(selectedPrefab);
            var towerController = currentBuildGhost.GetComponentInChildren<TowerController>();
            if (towerController) towerController.enabled = false;

            Collider[] colliders = currentBuildGhost.GetComponentsInChildren<Collider>();
            foreach (var col in colliders) col.enabled = false;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        bool isOverValidSurface = Physics.Raycast(ray, out hit) && hit.transform.CompareTag("Local");

        if (isOverValidSurface)
        {
            // --- NOVA LÓGICA DE ALTURA ---
            // Pega o collider do ghost para calcular a altura
            Collider ghostCollider = currentBuildGhost.GetComponentInChildren<Collider>();
            if (ghostCollider != null)
            {
                // A altura a ser adicionada é metade da altura total do collider
                float yOffset = ghostCollider.bounds.extents.y;
                currentBuildGhost.transform.position = hit.point + new Vector3(0, yOffset, 0);
            }
            else
            {
                // Se não houver collider, posiciona no ponto de impacto
                currentBuildGhost.transform.position = hit.point;
            }
        }
        else if (Physics.Raycast(ray, out hit))
        {
            currentBuildGhost.transform.position = hit.point;
        }

        int buildingCost = selectedTowerData.cost;
        bool hasEnoughCurrency = CurrencyManager.Instance.HasEnoughCurrency(buildingCost, CurrencyType.Geodites);

        var ghostRenderer = currentBuildGhost.GetComponentInChildren<MeshRenderer>();
        if (ghostRenderer != null)
        {
            ghostRenderer.material = (isOverValidSurface && hasEnoughCurrency) ? validPlacementMaterial : invalidPlacementMaterial;
        }
    }

    void PlaceBuilding()
    {
        if (selectedTowerData == null || currentBuildGhost == null) return;

        var ghostRenderer = currentBuildGhost.GetComponentInChildren<MeshRenderer>();
        if (ghostRenderer != null && ghostRenderer.material.name.Contains(validPlacementMaterial.name))
        {
            GameObject prefabToBuild = selectedTowerData.towerPrefab;
            int buildingCost = selectedTowerData.cost;

            if (CurrencyManager.Instance.HasEnoughCurrency(buildingCost, CurrencyType.Geodites))
            {
                // Usa a mesma lógica de altura para a torre final
                Vector3 finalPosition = currentBuildGhost.transform.position;
                Instantiate(prefabToBuild, finalPosition, Quaternion.identity);

                CurrencyManager.Instance.SpendCurrency(buildingCost, CurrencyType.Geodites);
                ClearSelection();
            }
        }
    }

    void ClearSelection()
    {
        if (currentBuildGhost != null)
        {
            Destroy(currentBuildGhost);
        }
        currentBuildGhost = null;
        selectedTowerData = null;
    }
}