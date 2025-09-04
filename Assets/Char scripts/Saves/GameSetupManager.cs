// GameSetupManager.cs (Vers�o Final e Simplificada)
using UnityEngine;

public class GameSetupManager : MonoBehaviour
{
    [Header("Configura��o de Spawn")]
    [Tooltip("Um objeto vazio na cena que marca a posi��o e rota��o inicial do jogador.")]
    public Transform spawnPoint;

    void Awake()
    {
        // Garante que a l�gica rode antes de qualquer outra coisa no jogo
        SpawnCommander();
    }

    private void SpawnCommander()
    {
        if (GameDataManager.Instance == null)
        {
            Debug.LogError("GameDataManager n�o encontrado! A cena foi iniciada diretamente sem passar pelo menu?");
            // Opcional: Adicione uma l�gica para carregar um comandante padr�o para testes
            return;
        }

        CharacterBase commanderData = GameDataManager.Instance.equipeSelecionada[0];

        if (commanderData == null)
        {
            Debug.LogError("Nenhum comandante selecionado no GameDataManager!");
            // Opcional: Voltar para o menu ou carregar um comandante padr�o
            return;
        }

        if (commanderData.commanderPrefab == null)
        {
            Debug.LogError($"O comandante '{commanderData.name}' n�o tem um prefab associado no seu CharacterBase!");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("Spawn Point n�o foi definido no GameSetupManager!");
            return;
        }

        // A M�GICA ACONTECE AQUI:
        // Instancia o prefab completo do comandante na posi��o e rota��o do spawnPoint.
        Instantiate(commanderData.commanderPrefab, spawnPoint.position, spawnPoint.rotation);

        Debug.Log($"Comandante '{commanderData.name}' instanciado na cena!");

        // Futuramente, � aqui que voc� passaria as torres para o BuildManager
        // BuildManager.Instance.SetAvailableTowers(GameDataManager.Instance.equipeSelecionada);
    }
}