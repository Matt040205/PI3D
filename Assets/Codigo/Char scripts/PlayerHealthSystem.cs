using UnityEngine;
using System;
using System.Collections;

public class PlayerHealthSystem : MonoBehaviour
{
    public CharacterBase characterData;
    public float currentHealth;
    public bool isRegenerating;

    private float timeSinceLastDamage;

    // Removemos 'public' para que ele não seja configurado no Inspector
    // Ele será encontrado dinamicamente
    private Transform respawnPoint;

    // VARIÁVEL PARA O NOME/TAG DO OBJETO DE RESPAWN NA CENA
    [Header("Configuração de Respawn")]
    [Tooltip("Nome/Tag do Transform do Respawn Point na cena atual. Ex: 'RespawnPoint'")]
    public string respawnPointNameOrTag = "RespawnPoint"; // Você pode mudar isso no Inspector

    // Evento para notificar mudanças na saúde
    public event Action OnHealthChanged;

    void Start()
    {
        currentHealth = characterData.maxHealth;
        NotifyHealthChanged(); // Notifica o valor inicial

        // --- MUDANÇA PRINCIPAL AQUI: LOCALIZANDO O RESPAWN POINT ---
        FindRespawnPoint();
    }

    /// <summary>
    /// Procura o Respawn Point na cena atual usando o nome ou tag configurado.
    /// </summary>
    void FindRespawnPoint()
    {
        // 1. Tenta encontrar pelo nome/tag definido no Inspector
        GameObject respawnObject = GameObject.FindWithTag(respawnPointNameOrTag);

        if (respawnObject == null)
        {
            respawnObject = GameObject.Find(respawnPointNameOrTag);
        }

        if (respawnObject != null)
        {
            respawnPoint = respawnObject.transform;
            Debug.Log($"Respawn Point '{respawnPointNameOrTag}' encontrado com sucesso!", respawnObject);
        }
        else
        {
            // Isso é um aviso, não um erro fatal, para o caso de querer usar a mesma cena
            // sem um ponto de respawn explícito (embora seja melhor ter um).
            Debug.LogWarning($"Respawn Point '{respawnPointNameOrTag}' NÃO foi encontrado na cena! Certifique-se de que ele existe e tem a tag/nome correta.");
        }
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
        // Garante que o ponto de respawn foi encontrado ao iniciar a cena
        if (respawnPoint == null)
        {
            // Tenta encontrar novamente, para o caso de ter sido carregado dinamicamente
            FindRespawnPoint();
        }

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
            Debug.LogError("O Respawn Point não foi encontrado! O jogador não pode ser teletransportado.");
        }

        // Restaura a saúde do jogador
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