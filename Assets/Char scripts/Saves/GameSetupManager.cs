// GameSetupManager.cs (Versão Final e Simplificada)
using UnityEngine;

public class GameSetupManager : MonoBehaviour
{
    [Header("Configuração de Spawn")]
    [Tooltip("Um objeto vazio na cena que marca a posição e rotação inicial do jogador.")]
    public Transform spawnPoint;

    void Awake()
    {
        // Garante que a lógica rode antes de qualquer outra coisa no jogo
        SpawnCommander();
    }

    private void SpawnCommander()
    {
        if (GameDataManager.Instance == null)
        {
            Debug.LogError("GameDataManager não encontrado! A cena foi iniciada diretamente sem passar pelo menu?");
            // Opcional: Adicione uma lógica para carregar um comandante padrão para testes
            return;
        }

        CharacterBase commanderData = GameDataManager.Instance.equipeSelecionada[0];

        if (commanderData == null)
        {
            Debug.LogError("Nenhum comandante selecionado no GameDataManager!");
            // Opcional: Voltar para o menu ou carregar um comandante padrão
            return;
        }

        if (commanderData.commanderPrefab == null)
        {
            Debug.LogError($"O comandante '{commanderData.name}' não tem um prefab associado no seu CharacterBase!");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("Spawn Point não foi definido no GameSetupManager!");
            return;
        }

        // A MÁGICA ACONTECE AQUI:
        // Instancia o prefab completo do comandante na posição e rotação do spawnPoint.
        Instantiate(commanderData.commanderPrefab, spawnPoint.position, spawnPoint.rotation);

        Debug.Log($"Comandante '{commanderData.name}' instanciado na cena!");

        // Futuramente, é aqui que você passaria as torres para o BuildManager
        // BuildManager.Instance.SetAvailableTowers(GameDataManager.Instance.equipeSelecionada);
    }
}