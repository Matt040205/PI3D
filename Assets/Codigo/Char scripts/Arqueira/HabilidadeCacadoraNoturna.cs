using UnityEngine;

[CreateAssetMenu(fileName = "Caçadora Noturna", menuName = "ExoBeasts/Habilidades/Caçadora Noturna")]
public class HabilidadeCacadoraNoturna : Ability
{
    [Header("Configurações da Habilidade")]
    public float damage = 500f;
    public float range = 100f;
    public float width = 3f;

    [Tooltip("Arraste o prefab da lógica da habilidade aqui.")]
    public CacadoraNoturnaLogic logicPrefab;

    public override bool Activate(GameObject quemUsou)
    {
        if (logicPrefab == null)
        {
            Debug.LogError("O prefab da lógica da habilidade está NULO no ScriptableObject 'Caçadora Noturna'!");
            return true;
        }

        PlayerShooting shootingScript = quemUsou.GetComponent<PlayerShooting>();
        PlayerMovement movementScript = quemUsou.GetComponent<PlayerMovement>();
        CameraController cameraController = Object.FindObjectOfType<CameraController>();
        Camera mainCamera = Camera.main;
        Transform modelPivot = (movementScript != null) ? movementScript.GetModelPivot() : quemUsou.transform;
        Transform firePoint = (shootingScript != null && shootingScript.firePoint != null) ? shootingScript.firePoint : quemUsou.transform;

        Vector3 shotDirection;
        Vector3 spawnPosition = firePoint.position;

        if (cameraController != null && cameraController.isAiming && shootingScript != null && mainCamera != null)
        {
            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, shootingScript.maxDistance, shootingScript.hitLayers))
            {
                shotDirection = (hit.point - spawnPosition).normalized;
            }
            else
            {
                shotDirection = ray.direction;
            }
        }
        else
        {
            shotDirection = modelPivot.forward;
        }

        Quaternion spawnRotation = Quaternion.LookRotation(shotDirection);

        CacadoraNoturnaLogic logic = Instantiate(logicPrefab, spawnPosition, spawnRotation);
        logic.StartUltimateEffect(quemUsou, damage, range, width);

        Debug.Log("Caçadora Noturna ativado.");
        return true;
    }
}