using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;
using static TrapDataSO;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    [Header("Câmeras")]
    public CinemachineCamera buildCamera;
    public GameObject upgradePanel;

    [Header("Listas de Construíveis")]
    private List<CharacterBase> availableTowers = new List<CharacterBase>();
    public List<TrapDataSO> availableTraps = new List<TrapDataSO>();

    private GameObject selectedBuildablePrefab;
    private int selectedBuildableCost;
    private object selectedBuildableData;

    [Header("Visual do Ghost")]
    private GameObject currentBuildGhost;
    public Material validPlacementMaterial;
    public Material invalidPlacementMaterial;

    public static bool isBuildingMode = false;
    public float gridSize = 1f;

    [Header("Configuração de Altura")]
    public float globalHeightOffset = 0.5f;

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

    // ESTA É A CORREÇÃO CRUCIAL (PROBLEMA 1)
    // O loop 'for (int i = 1; ...)' começa em 1, ignorando o índice 0 (o jogador).
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
        UpdateBuildUI();
    }

    public void UpdateBuildUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateBuildUI(availableTowers, availableTraps);
        }
    }

    public void SelectTowerToBuild(CharacterBase towerData)
    {
        ClearSelection();
        selectedBuildablePrefab = towerData.towerPrefab;
        selectedBuildableCost = towerData.cost;
        selectedBuildableData = towerData;
    }

    public void SelectTrapToBuild(TrapDataSO trapData)
    {
        ClearSelection();
        selectedBuildablePrefab = trapData.prefab;
        selectedBuildableCost = trapData.geoditeCost;
        selectedBuildableData = trapData;
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

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowBuildUI(state);
        }

        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;
    }

    void HandleBuildGhost()
    {
        if (selectedBuildablePrefab == null)
        {
            if (currentBuildGhost != null) Destroy(currentBuildGhost);
            return;
        }

        GameObject selectedPrefab = selectedBuildablePrefab;
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

        bool isOverValidSurface = false;

        if (Physics.Raycast(ray, out hit))
        {
            if (selectedBuildableData is TrapDataSO trapData)
            {
                if (trapData.placementType == TrapPlacementType.OnPath)
                {
                    isOverValidSurface = hit.transform.CompareTag("Path");
                }
                else
                {
                    isOverValidSurface = hit.transform.CompareTag("Local");
                }
            }
            else if (selectedBuildableData is CharacterBase)
            {
                isOverValidSurface = hit.transform.CompareTag("Local");
            }

            if (isOverValidSurface)
            {
                float calculatedHeight = CalculateRequiredHeight(hit.point, selectedBuildablePrefab);
                currentBuildGhost.transform.position = new Vector3(hit.point.x, hit.point.y + calculatedHeight, hit.point.z);
            }
            else
            {
                currentBuildGhost.transform.position = hit.point + Vector3.up * globalHeightOffset;
            }
        }
        else
        {
            if (currentBuildGhost != null)
                currentBuildGhost.transform.position = ray.GetPoint(20f);
        }

        int buildingCost = selectedBuildableCost;
        bool hasEnoughCurrency = CurrencyManager.Instance.HasEnoughCurrency(buildingCost, CurrencyType.Geodites);

        var ghostRenderer = currentBuildGhost.GetComponentInChildren<MeshRenderer>();
        if (ghostRenderer != null)
        {
            ghostRenderer.material = (isOverValidSurface && hasEnoughCurrency) ? validPlacementMaterial : invalidPlacementMaterial;
        }
    }

    private float CalculateRequiredHeight(Vector3 hitPoint, GameObject prefab)
    {
        Collider col = prefab.GetComponentInChildren<Collider>();
        if (col != null)
        {
            float bottomDistance = col.bounds.extents.y;
            Ray downRay = new Ray(hitPoint + Vector3.up * 10f, Vector3.down);
            RaycastHit downHit;

            if (Physics.Raycast(downRay, out downHit, 20f))
            {
                return downHit.point.y - hitPoint.y + bottomDistance + globalHeightOffset;
            }
        }
        return globalHeightOffset;
    }

    void PlaceBuilding()
    {
        if (selectedBuildablePrefab == null || currentBuildGhost == null) return;

        var ghostRenderer = currentBuildGhost.GetComponentInChildren<MeshRenderer>();

        if (ghostRenderer != null && ghostRenderer.material.name.StartsWith(validPlacementMaterial.name))
        {
            GameObject prefabToBuild = selectedBuildablePrefab;
            int buildingCost = selectedBuildableCost;

            if (CurrencyManager.Instance.HasEnoughCurrency(buildingCost, CurrencyType.Geodites))
            {
                Vector3 finalPosition = currentBuildGhost.transform.position;

                finalPosition.x = Mathf.Round(finalPosition.x / gridSize) * gridSize;
                finalPosition.z = Mathf.Round(finalPosition.z / gridSize) * gridSize;

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
        selectedBuildablePrefab = null;
        selectedBuildableCost = 0;
        selectedBuildableData = null;

        TowerSelectionManager.Instance.DeselectTower();
    }
}