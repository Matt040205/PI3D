using UnityEngine;

public class GameSetupManager : MonoBehaviour
{
    [Header("Configuração de Spawn")]
    public Transform spawnPoint;

    void Start()
    {
       // Debug.Log("--- DEBUG: GameSetupManager.Start() foi chamado. ---");
        SetupGame();
    }

    private void SetupGame()
    {
        if (GameDataManager.Instance == null)
        {
            Debug.LogError("DEBUG FALHA: GameDataManager não existe!");
            return;
        }
        //Debug.Log("DEBUG: GameDataManager encontrado.");

        CharacterBase commanderData = GameDataManager.Instance.equipeSelecionada[0];
        if (commanderData != null && commanderData.commanderPrefab != null && spawnPoint != null)
        {
            Instantiate(commanderData.commanderPrefab, spawnPoint.position, spawnPoint.rotation);
           // Debug.Log("DEBUG: Comandante instanciado.");
        }

        if (BuildManager.Instance != null)
        {
          //  Debug.Log("DEBUG: BuildManager encontrado. Chamando SetAvailableTowers...");
            BuildManager.Instance.SetAvailableTowers(GameDataManager.Instance.equipeSelecionada);
        }
        else
        {
            Debug.LogError("DEBUG FALHA: BuildManager.Instance é NULO!");
        }
    }
}