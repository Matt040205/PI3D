using UnityEngine;
using System.Collections;

public class IntegrationManager : MonoBehaviour
{
    public static IntegrationManager Instance;

    [Header("Referências dos Sistemas")]
    public CharacterStatsBridge statsBridge;
    public UpgradeManager upgradeManager;
    public AbilityManager abilityManager;
    public PlayerHUD playerHUD;

    [Header("Configuração")]
    public bool enableDebugLogs = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Garante que todos os componentes necessários existam
        if (upgradeManager == null)
            upgradeManager = gameObject.AddComponent<UpgradeManager>();

        if (abilityManager == null)
            abilityManager = gameObject.AddComponent<AbilityManager>();
    }

    void Start()
    {
        // Configura os managers com as referências apropriadas
        if (enableDebugLogs)
            upgradeManager.debugLogs = true;

        // Busca automaticamente componentes se não foram atribuídos
        if (statsBridge == null)
            statsBridge = FindObjectOfType<CharacterStatsBridge>();

        if (playerHUD == null)
            playerHUD = FindObjectOfType<PlayerHUD>();

        LogSystemStatus();
    }

    void LogSystemStatus()
    {
        if (!enableDebugLogs) return;

        Debug.Log("=== STATUS DO SISTEMA DE INTEGRAÇÃO ===");
        Debug.Log($"StatsBridge: {statsBridge != null}");
        Debug.Log($"UpgradeManager: {upgradeManager != null}");
        Debug.Log($"AbilityManager: {abilityManager != null}");
        Debug.Log($"PlayerHUD: {playerHUD != null}");

        if (statsBridge != null && statsBridge.characterData != null)
        {
            Debug.Log($"Personagem: {statsBridge.characterData.name}");
            Debug.Log($"Vida Base: {statsBridge.characterData.maxHealth}");
        }
    }

    // Método para aplicar upgrades via UI ou eventos
    public void ApplyUpgradeToPlayer(UpgradePathSO path, int levelIndex)
    {
        if (statsBridge == null)
        {
            Debug.LogError("StatsBridge não encontrado!");
            return;
        }

        upgradeManager.ApplyUpgrade(path, levelIndex, statsBridge.currentStats);
        statsBridge.OnStatsUpdated(); // Força atualização dos componentes

        if (enableDebugLogs)
            Debug.Log($"Upgrade aplicado: {path.pathName} nível {levelIndex}");
    }

    // Método para desbloquear habilidade
    public void UnlockAbility(AbilitySO ability)
    {
        if (abilityManager == null)
        {
            Debug.LogError("AbilityManager não encontrado!");
            return;
        }

        abilityManager.UnlockAbility(ability);

        if (enableDebugLogs)
            Debug.Log($"Habilidade desbloqueada: {ability.abilityName}");
    }
}