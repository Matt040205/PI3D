using UnityEngine;
using System.Collections;

public class CharacterStatsBridge : MonoBehaviour
{
    [Header("Dados Base")]
    public CharacterBase characterData;

    [Header("Stats em Runtime")]
    public CharacterStats currentStats;

    [Header("Referências para Atualização")]
    public PlayerHealthSystem healthSystem;
    public PlayerShooting shootingSystem;
    public PlayerMovement movementSystem;
    public MeleeCombatSystem meleeSystem;

    [Header("Configuração")]
    public bool applyInitialStatsOnStart = true;

    // Eventos para notificar mudanças (renomeado para evitar conflito)
    public System.Action OnStatsInitialized;
    public System.Action OnStatsChanged; // Renomeado de OnStatsUpdated

    void Start()
    {
        // Busca automaticamente componentes se não configurados
        if (healthSystem == null) healthSystem = GetComponent<PlayerHealthSystem>();
        if (shootingSystem == null) shootingSystem = GetComponent<PlayerShooting>();
        if (movementSystem == null) movementSystem = GetComponent<PlayerMovement>();
        if (meleeSystem == null) meleeSystem = GetComponent<MeleeCombatSystem>();

        if (applyInitialStatsOnStart)
            InitializeStats();
    }

    public void InitializeStats()
    {
        if (characterData == null)
        {
            Debug.LogError("CharacterData não atribuído!");
            return;
        }

        // Cria os stats iniciais a partir do CharacterBase
        currentStats = new CharacterStats(characterData, transform);

        // Aplica os stats iniciais a todos os sistemas
        UpdateAllSystems();

        OnStatsInitialized?.Invoke();

        Debug.Log($"Stats inicializados para: {characterData.name}");
    }

    // Chamado quando upgrades são aplicados
    public void OnStatsUpdated()
    {
        UpdateAllSystems();
        OnStatsChanged?.Invoke(); // Usando o nome do evento renomeado
    }

    void UpdateAllSystems()
    {
        if (healthSystem != null)
            healthSystem.OnStatsUpdated(currentStats);

        if (shootingSystem != null)
            shootingSystem.OnStatsUpdated(currentStats);

        if (movementSystem != null)
            movementSystem.OnStatsUpdated(currentStats);

        if (meleeSystem != null)
            meleeSystem.OnStatsUpdated(currentStats);
    }

    // Métodos para acesso rápido aos valores
    public float GetHealthPercent()
    {
        if (healthSystem == null) return 0;
        return healthSystem.currentHealth / currentStats.maxHealth;
    }

    public bool IsHealthLow()
    {
        if (healthSystem == null) return false;
        return healthSystem.currentHealth < currentStats.maxHealth * 0.3f;
    }
}