using UnityEngine;

[CreateAssetMenu(fileName = "Bomba de Spray", menuName = "ExoBeasts/Personagens/Polvo/Habilidade/Bomba de Spray")]
public class HabilidadeBombaSpray : Ability
{
    [Header("Configurações da Bomba")]
    public float throwForce = 15f;
    public float explosionRadius = 6f;
    public float cloudDuration = 4f;

    [Tooltip("Arraste o prefab da LATA aqui")]
    public BombaSprayProjectile projectilePrefab;

    [Tooltip("Layers que o raio da mira pode acertar (geralmente Default, Ground, Enemy)")]
    public LayerMask aimLayers = ~0; // ~0 significa "Tudo"

    public override bool Activate(GameObject quemUsou)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("Faltou o prefab da Bomba no ScriptableObject!");
            return true;
        }

        // 1. Tenta achar o ponto de saída de tiro do PlayerShooting para a granada não sair de dentro do corpo
        Vector3 spawnPos = quemUsou.transform.position + Vector3.up * 1.5f; // Padrão (altura do peito)
        PlayerShooting shootingScript = quemUsou.GetComponent<PlayerShooting>();

        if (shootingScript != null && shootingScript.firePoint != null)
        {
            spawnPos = shootingScript.firePoint.position;
        }

        // 2. Calcula a direção da mira usando a Câmera (Igual ao PlayerShooting)
        Vector3 throwDirection = GetAimDirection(quemUsou, spawnPos);

        // 3. Cria a granada olhando para o alvo
        BombaSprayProjectile bomba = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(throwDirection));

        // 4. Lança
        bomba.Launch(throwDirection * throwForce, explosionRadius, cloudDuration);

        return true;
    }

    private Vector3 GetAimDirection(GameObject quemUsou, Vector3 originPoint)
    {
        Camera cam = Camera.main;
        if (cam == null) return quemUsou.transform.forward;

        // Raio no centro da tela (0.5, 0.5)
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint;

        // Se bater em algo em até 100 metros, mira lá. Se não, mira no horizonte.
        if (Physics.Raycast(ray, out hit, 100f, aimLayers))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100f);
        }

        // Calcula vetor direção (Destino - Origem) normalizado
        return (targetPoint - originPoint).normalized;
    }
}