using UnityEngine;
using System;

public class PlayerHealthSystem : MonoBehaviour
{
    public CharacterBase characterData;
    public float currentHealth;
    public bool isRegenerating;

    private float timeSinceLastDamage;

    // Evento para notificar mudanças na saúde
    public event Action OnHealthChanged;

    void Start()
    {
        currentHealth = characterData.maxHealth;
        NotifyHealthChanged(); // Notifica o valor inicial
    }

    void Update()
    {
        HandleRegeneration();
    }

    void HandleRegeneration()
    {
        if (currentHealth >= characterData.maxHealth)
        {
            isRegenerating = false;
            return;
        }

        timeSinceLastDamage += Time.deltaTime;

        if (timeSinceLastDamage >= 3f)
        {
            isRegenerating = true;
            currentHealth += characterData.maxHealth * 0.01f * Time.deltaTime;
            currentHealth = Mathf.Min(currentHealth, characterData.maxHealth);

            // Notifica a mudança durante a regeneração
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
        currentHealth = Mathf.Min(currentHealth + amount, characterData.maxHealth);
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
}