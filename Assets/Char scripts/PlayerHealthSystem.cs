using UnityEngine;
using System;

public class PlayerHealthSystem : MonoBehaviour
{
    // Mantém referência ao CharacterBase original para icons e descrições
    public CharacterBase characterData;

    // Agora usa currentHealth baseado nos stats atualizados
    [HideInInspector] public float currentHealth;
    public bool isRegenerating;

    private float timeSinceLastDamage;
    private CharacterStatsBridge statsBridge;

    // Evento para notificar mudanças na saúde
    public event Action OnHealthChanged;

    void Start()
    {
        statsBridge = GetComponent<CharacterStatsBridge>();

        if (statsBridge != null && statsBridge.currentStats != null)
        {
            currentHealth = statsBridge.currentStats.maxHealth;
        }
        else if (characterData != null)
        {
            // Fallback para o sistema antigo
            currentHealth = characterData.maxHealth;
        }

        NotifyHealthChanged();
    }

    void Update()
    {
        HandleRegeneration();
    }

    void HandleRegeneration()
    {
        if (statsBridge == null || statsBridge.currentStats == null) return;

        if (currentHealth >= statsBridge.currentStats.maxHealth)
        {
            isRegenerating = false;
            return;
        }

        timeSinceLastDamage += Time.deltaTime;

        if (timeSinceLastDamage >= 3f)
        {
            isRegenerating = true;
            float regenRate = statsBridge.currentStats.maxHealth * 0.01f * Time.deltaTime;
            currentHealth = Mathf.Min(currentHealth + regenRate, statsBridge.currentStats.maxHealth);

            NotifyHealthChanged();
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        timeSinceLastDamage = 0f;
        isRegenerating = false;

        NotifyHealthChanged();

        if (currentHealth <= 0) Die();
    }

    public void Heal(float amount)
    {
        if (statsBridge != null && statsBridge.currentStats != null)
        {
            currentHealth = Mathf.Min(currentHealth + amount, statsBridge.currentStats.maxHealth);
        }
        else if (characterData != null)
        {
            currentHealth = Mathf.Min(currentHealth + amount, characterData.maxHealth);
        }

        NotifyHealthChanged();
    }

    void Die()
    {
        Debug.Log("Player morreu!");
        // Lógica de morte
    }

    void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke();
    }

    // Método chamado quando os stats são atualizados
    public void OnStatsUpdated(CharacterStats stats)
    {
        // Mantém a porcentagem de vida ao atualizar a vida máxima
        float healthPercent = currentHealth / (statsBridge.currentStats?.maxHealth ?? characterData.maxHealth);
        currentHealth = stats.maxHealth * healthPercent;

        NotifyHealthChanged();

        Debug.Log($"Sistema de saúde atualizado. Vida: {currentHealth}/{stats.maxHealth}");
    }
}