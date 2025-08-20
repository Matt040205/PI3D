using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;

public class BuildManager : MonoBehaviour
{
    [Header("Câmeras")]
    public CinemachineCamera buildCamera;

    [Header("Lista de Construções")]
    public List<GameObject> buildablePrefabs;
    private int selectedPrefabIndex = -1;

    [Header("Visual do Ghost")]
    private GameObject currentBuildGhost;
    public Material validPlacementMaterial;
    public Material invalidPlacementMaterial;

    public bool isBuildingMode = false;

    private const int PriorityBuild = 20;
    private const int PriorityInactive = 0;


    void Start()
    {
        buildCamera.Priority.Value = PriorityInactive;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
        if (state)
        {
            buildCamera.Priority.Value = PriorityBuild;
        }
        else
        {
            buildCamera.Priority.Value = PriorityInactive;
            ClearSelection();
        }

        UIManager.Instance.ShowBuildUI(state);
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;
    }

    void HandleBuildGhost()
    {
        if (selectedPrefabIndex == -1)
        {
            if (currentBuildGhost != null) Destroy(currentBuildGhost);
            return;
        }

        GameObject selectedPrefab = buildablePrefabs[selectedPrefabIndex];

        if (currentBuildGhost == null)
        {
            currentBuildGhost = Instantiate(selectedPrefab);
            Collider ghostCollider = currentBuildGhost.GetComponent<Collider>();
            if (ghostCollider != null) ghostCollider.isTrigger = true;
        }

        MeshRenderer ghostRenderer = currentBuildGhost.GetComponent<MeshRenderer>();
        if (ghostRenderer == null) return;

        // CORRIGIDO: O Raycast agora parte da câmera principal da cena (Camera.main)
        // Isso garante que estamos usando a câmera "real" que está renderizando o jogo.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GridPlacement gridPlacer = selectedPrefab.GetComponent<GridPlacement>();
            currentBuildGhost.transform.position = hit.point;
            gridPlacer.SnapToGrid();

            RaycastHit groundHit;
            if (Physics.Raycast(currentBuildGhost.transform.position, Vector3.down, out groundHit, Mathf.Infinity))
            {
                float objectHeight = selectedPrefab.GetComponent<GridPlacement>().GetObjectHeight();
                currentBuildGhost.transform.position = new Vector3(
                    currentBuildGhost.transform.position.x,
                    groundHit.point.y + (objectHeight / 2f),
                    currentBuildGhost.transform.position.z
                );
            }

            int buildingCost = selectedPrefab.GetComponent<GridPlacement>().cost;

            if (CurrencyManager.Instance.HasEnoughCurrency(buildingCost, CurrencyType.Geodites))
            {
                ghostRenderer.material = validPlacementMaterial;
            }
            else
            {
                ghostRenderer.material = invalidPlacementMaterial;
            }
        }
    }

    void PlaceBuilding()
    {
        if (selectedPrefabIndex == -1 || currentBuildGhost == null) return;

        GameObject selectedPrefab = buildablePrefabs[selectedPrefabIndex];
        int buildingCost = selectedPrefab.GetComponent<GridPlacement>().cost;

        if (CurrencyManager.Instance.HasEnoughCurrency(buildingCost, CurrencyType.Geodites))
        {
            CurrencyManager.Instance.SpendCurrency(buildingCost, CurrencyType.Geodites);
            Instantiate(selectedPrefab, currentBuildGhost.transform.position, Quaternion.identity);
            ClearSelection();
        }
        else
        {
            Debug.Log("Não há Geoditas suficientes!");
        }
    }

    public void SelectBuilding(int prefabIndex)
    {
        ClearSelection();

        if (prefabIndex >= 0 && prefabIndex < buildablePrefabs.Count)
        {
            selectedPrefabIndex = prefabIndex;
        }
    }

    void ClearSelection()
    {
        if (currentBuildGhost != null)
        {
            Destroy(currentBuildGhost);
        }
        currentBuildGhost = null;
        selectedPrefabIndex = -1;
    }
}