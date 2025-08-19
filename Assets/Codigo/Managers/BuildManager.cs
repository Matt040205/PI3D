using UnityEngine;
using System.Collections.Generic;

public class BuildManager : MonoBehaviour
{
    [Header("C�meras")]
    public Camera thirdPersonCamera;
    public Camera buildCamera;

    [Header("Lista de Constru��es")]
    public List<GameObject> buildablePrefabs;
    private int selectedPrefabIndex = -1;

    [Header("Visual do Ghost")]
    private GameObject currentBuildGhost;
    public Material validPlacementMaterial;
    public Material invalidPlacementMaterial;

    public bool isBuildingMode = false;

    void Start()
    {
        buildCamera.gameObject.SetActive(false);
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
        thirdPersonCamera.gameObject.SetActive(!state);
        buildCamera.gameObject.SetActive(state);

        UIManager.Instance.ShowBuildUI(state);

        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;

        if (!state)
        {
            ClearSelection();
        }
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

        Ray ray = buildCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Primeiro Raycast: da c�mera para o mouse
            GridPlacement gridPlacer = selectedPrefab.GetComponent<GridPlacement>();

            // Alinha a posi��o do ghost em X e Z usando o ponto de colis�o
            Vector3 snappedPosition = hit.point;

            currentBuildGhost.transform.position = snappedPosition;
            gridPlacer.SnapToGrid(); // Alinha X e Z � grade

            // Segundo Raycast: do ghost para baixo para encontrar o ch�o
            RaycastHit groundHit;
            if (Physics.Raycast(currentBuildGhost.transform.position, Vector3.down, out groundHit, Mathf.Infinity))
            {
                // Define a posi��o Y do ghost na altura exata do ch�o
                currentBuildGhost.transform.position = new Vector3(currentBuildGhost.transform.position.x, groundHit.point.y, currentBuildGhost.transform.position.z);
            }

            int buildingCost = selectedPrefab.GetComponent<GridPlacement>().cost;
            if (CurrencyManager.Instance.HasEnoughMoney(buildingCost))
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

        if (CurrencyManager.Instance.HasEnoughMoney(buildingCost))
        {
            CurrencyManager.Instance.SpendMoney(buildingCost);

            Instantiate(selectedPrefab, currentBuildGhost.transform.position, Quaternion.identity);

            ClearSelection();
        }
        else
        {
            Debug.Log("N�o h� dinheiro suficiente!");
        }
    }

    public void SelectBuilding(int prefabIndex)
    {
        ClearSelection();

        if (prefabIndex >= 0 && prefabIndex < buildablePrefabs.Count)
        {
            selectedPrefabIndex = prefabIndex;
            Debug.Log("Selecionado: " + buildablePrefabs[selectedPrefabIndex].name);
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