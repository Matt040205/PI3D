using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class ObjectiveHealthSystem : MonoBehaviour
{
    [Header("Configurações de Vida")]
    public float maxHealth = 100f;
    public float currentHealth;

    public event Action OnHealthChanged;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        NotifyHealthChanged();
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0 || isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        NotifyHealthChanged();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        Time.timeScale = 1f;
        SceneManager.LoadScene("Lose");
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke();
    }
}