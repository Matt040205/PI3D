using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Configurações de Ataque")]
    public float damage = 25f;
    public float fireRate = 0.5f;
    public float range = 100f;

    private float nextTimeToFire = 0f;

    [Header("Câmera e UI")]
    // Referência à câmera principal, de onde o tiro sairá
    public Camera mainCamera;

    // GameObject que representa o ponto na mira (opcional)
    public GameObject crosshairDot;

    void Start()
    {
        // Se a câmera não for atribuída no Inspector, pega a principal
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Garante que o ponto da mira esteja ativo se for atribuído
        if (crosshairDot != null)
        {
            crosshairDot.SetActive(true);
        }
    }

    void Update()
    {
        // Verifica se o jogador clicou para atirar e se o tempo de recarga já passou
        if (Input.GetMouseButton(0) && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        RaycastHit hit;

        // Cria um raio a partir do centro da tela, na direção da câmera
        // Este é o método HitScan
        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, range))
        {
            Debug.Log("Acertou: " + hit.transform.name);

            // Tenta obter o componente EnemyController do objeto atingido
            EnemyController enemy = hit.transform.GetComponent<EnemyController>();

            // Se o objeto atingido tiver o script EnemyController, damos dano a ele
            if (enemy != null)
            {
                // Chama a função TakeDamage do script do inimigo
                enemy.TakeDamage(damage, transform);
            }
        }
    }
}