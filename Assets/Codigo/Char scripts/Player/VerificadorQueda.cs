using UnityEngine;

// Garante que este script esteja no mesmo objeto que o CharacterController
[RequireComponent(typeof(CharacterController))]
public class VerificadorQueda : MonoBehaviour
{
    [Header("Configuração de Queda")]
    [Tooltip("A altura Y em que o jogador será teleportado de volta.")]
    public float limiteY = -30f;

    [Header("Referências")]
    private Transform spawnPoint;
    private CharacterController controller;

    void Start()
    {
        // Pega o CharacterController do jogador
        controller = GetComponent<CharacterController>();

        // Encontra o GameSetupManager na cena para descobrir onde fica o SpawnPoint
        GameSetupManager setupManager = FindObjectOfType<GameSetupManager>();

        if (setupManager != null && setupManager.spawnPoint != null)
        {
            spawnPoint = setupManager.spawnPoint;
        }
        else
        {
            Debug.LogError("VerificadorQueda: Não foi possível encontrar o SpawnPoint! O teleporte de queda não funcionará.");
        }
    }

    void Update()
    {
        // Verifica a posição Y em todos os frames
        if (transform.position.y < limiteY)
        {
            TeleportarParaSpawn();
        }
    }

    void TeleportarParaSpawn()
    {
        if (spawnPoint == null)
        {
            Debug.LogError("VerificadorQueda: SpawnPoint é nulo. Não é possível teleportar.");
            return;
        }

        Debug.Log("Jogador caiu do mapa! Teleportando para o SpawnPoint.");

        // 1. Desativa o CharacterController (MUITO IMPORTANTE para o teleporte)
        controller.enabled = false;

        // 2. Move o transform do jogador diretamente para a posição e rotação do spawn
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        // 3. Reativa o CharacterController
        // Precisamos esperar um frame para o motor de física registar a mudança
        // Mas para este caso, reativar imediatamente costuma ser seguro.
        controller.enabled = true;
    }
}
