using UnityEngine;
using System;
using System.Collections; // Adicione este namespace para usar Coroutines

public class PlayerHealthSystem : MonoBehaviour
{
    public CharacterBase characterData;
    public float currentHealth;
    public bool isRegenerating;

    private float timeSinceLastDamage;

    public Transform respawnPoint;

    // Evento para notificar mudanças na sade
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

            // Notifica a mudana durante a regenerao
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
        // Garante que o ponto de respawn est atribuído
        if (respawnPoint != null)
        {
            // Tenta pegar o CharacterController
            CharacterController controller = GetComponent<CharacterController>();

            // Tenta pegar o script de movimento
            PlayerMovement movementScript = GetComponent<PlayerMovement>();

            // Desativa temporariamente o script e o controlador para permitir o teletransporte
            if (controller != null) controller.enabled = false;
            if (movementScript != null) movementScript.enabled = false;

            // Teletransporta o jogador para o ponto de respawn
            transform.position = respawnPoint.position;

            // Inicia uma coroutine para reativar o movimento e o controlador
            StartCoroutine(ReactivatePlayer(controller, movementScript));
        }
        else
        {
            Debug.LogWarning("O respawnPoint não foi atribuído! O jogador não pode ser teletransportado.");
        }

        // Restaura a sade do jogador
        currentHealth = characterData.maxHealth;
        NotifyHealthChanged();
    }

    private IEnumerator ReactivatePlayer(CharacterController controller, PlayerMovement movementScript)
    {
        // Espera um frame para garantir que a posição foi atualizada
        yield return null;

        // Reativa o controlador e o script de movimento
        if (controller != null) controller.enabled = true;
        if (movementScript != null) movementScript.enabled = true;
    }

    void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke();
    }
}