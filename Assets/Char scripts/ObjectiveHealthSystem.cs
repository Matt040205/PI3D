using UnityEngine;
using System; // Necessário para usar o 'Action'

public class ObjectiveHealthSystem : MonoBehaviour
{
    [Header("Configurações de Vida")]
    public float maxHealth = 1000f;
    public float currentHealth;

    // Evento para notificar a UI quando a vida mudar (igual ao do jogador)
    public event Action OnHealthChanged;

    void Start()
    {
        currentHealth = maxHealth;
        NotifyHealthChanged(); // Garante que a UI comece com o valor correto
    }

    public void TakeDamage(float damage)
    {
        // Se a vida já for zero, não faz nada
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); // Garante que a vida não fique negativa

        Debug.Log("Objetivo sofreu " + damage + " de dano. Vida restante: " + currentHealth);
        NotifyHealthChanged(); // Avisa a UI para se atualizar

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.LogError("GAME OVER! O objetivo foi destruído!");
        // --- LÓGICA DE FIM DE JOGO ---
        // Aqui você pode adicionar a lógica para parar o jogo,
        // mostrar uma tela de derrota, etc.
        // Por enquanto, vamos apenas parar o tempo:
        Time.timeScale = 0; // Pausa o jogo
    }

    private void NotifyHealthChanged()
    {
        // Dispara o evento, se alguém estiver "ouvindo"
        OnHealthChanged?.Invoke();
    }
}