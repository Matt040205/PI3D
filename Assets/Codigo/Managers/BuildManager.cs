// Cole esta versão no seu BuildManager
using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;
    public CinemachineCamera buildCamera;
    private List<CharacterBase> availableTowers = new List<CharacterBase>();
    private CharacterBase selectedTowerData;
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
        Debug.Log("--- DEBUG: BuildManager.SetAvailableTowers() foi chamado. ---");
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
            Debug.Log("DEBUG: UIManager encontrado. Chamando UpdateBuildUI com " + availableTowers.Count + " torres.");
            UIManager.Instance.UpdateBuildUI(availableTowers);
        }
        else
        {
            Debug.LogError("DEBUG FALHA: UIManager.Instance é NULO!");
        }
    }

    // O resto do script (Update, PlaceBuilding, etc.) continua igual...
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
        if (selectedTowerData == null) { if (currentBuildGhost != null) Destroy(currentBuildGhost); return; }
        GameObject selectedPrefab = selectedTowerData.commanderPrefab;
        if (selectedPrefab == null) return;
        if (currentBuildGhost == null)
        {
            currentBuildGhost = Instantiate(selectedPrefab);
            Collider ghostCollider = currentBuildGhost.GetComponent<Collider>();
            if (ghostCollider != null) ghostCollider.enabled = false;
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool isOverValidSurface = Physics.Raycast(ray, out hit) && hit.transform.CompareTag("Local");
        currentBuildGhost.transform.position = hit.point;
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
        GameObject selectedPrefab = selectedTowerData.commanderPrefab;
        int buildingCost = selectedTowerData.cost;
        var ghostRenderer = currentBuildGhost.GetComponentInChildren<MeshRenderer>();
        if (ghostRenderer != null && ghostRenderer.material.name.Contains(validPlacementMaterial.name))
        {
            if (CurrencyManager.Instance.HasEnoughCurrency(buildingCost, CurrencyType.Geodites))
            {
                Instantiate(selectedPrefab, currentBuildGhost.transform.position, Quaternion.identity);
                CurrencyManager.Instance.SpendCurrency(buildingCost, CurrencyType.Geodites);
                ClearSelection();
            }
        }
    }

    void ClearSelection()
    {
        if (currentBuildGhost != null) { Destroy(currentBuildGhost); }
        currentBuildGhost = null;
        selectedTowerData = null;
    }
}