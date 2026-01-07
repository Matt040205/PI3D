using UnityEngine;
using System;
using System.Collections;

public class PlayerHealthSystem : MonoBehaviour
{
    public CharacterBase characterData;
    public float currentHealth;
    public bool isRegenerating;

    [Header("Status de Buffs (Visualização)")]
    public float damageMultiplier = 1f; // Use isso no script de ataque
    public float speedMultiplier = 1f;  // Use isso no script de movimento
    public bool isBuffed = false;

    private float timeSinceLastDamage;
    private Transform respawnPoint;
    private Coroutine buffCoroutine;

    [Header("Configuração de Respawn")]
    [Tooltip("Nome/Tag do Transform do Respawn Point na cena atual.")]
    public string respawnPointNameOrTag = "RespawnPoint";

    // --- EVENTOS (Restaurados) ---
    public event Action OnHealthChanged;
    public event Action<float> OnDamageDealt;

    void Start()
    {
        currentHealth = characterData.maxHealth;
        NotifyHealthChanged();
        FindRespawnPoint();
    }

    void Update()
    {
        HandleRegeneration();
    }

    // --- SISTEMA DE BUFFS (Torre Nível 5) ---
    public void ApplyBuffs(float newDamageMult, float newSpeedMult, float duration)
    {
        if (buffCoroutine != null) StopCoroutine(buffCoroutine);

        damageMultiplier = newDamageMult;
        speedMultiplier = newSpeedMult;
        isBuffed = true;

        buffCoroutine = StartCoroutine(RemoveBuffsAfterTime(duration));
    }

    private IEnumerator RemoveBuffsAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);

        damageMultiplier = 1f;
        speedMultiplier = 1f;
        isBuffed = false;
        buffCoroutine = null;
    }

    // --- FUNÇÃO QUE ESTAVA FALTANDO (CORREÇÃO DO ERRO) ---
    public void TriggerDamageDealt(float damageAmount)
    {
        OnDamageDealt?.Invoke(damageAmount);
    }
    // -----------------------------------------------------

    void FindRespawnPoint()
    {
        GameObject respawnObject = GameObject.FindWithTag(respawnPointNameOrTag);
        if (respawnObject == null) respawnObject = GameObject.Find(respawnPointNameOrTag);

        if (respawnObject != null)
        {
            respawnPoint = respawnObject.transform;
        }
        else
        {
            Debug.LogWarning($"Respawn Point '{respawnPointNameOrTag}' NÃO foi encontrado na cena!");
        }
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
        if (respawnPoint == null) FindRespawnPoint();

        if (respawnPoint != null)
        {
            CharacterController controller = GetComponent<CharacterController>();
            // Tenta pegar o PlayerMovement (se existir com esse nome)
            MonoBehaviour movementScript = GetComponent("PlayerMovement") as MonoBehaviour;

            if (controller != null) controller.enabled = false;
            if (movementScript != null) movementScript.enabled = false;

            transform.position = respawnPoint.position;

            StartCoroutine(ReactivatePlayer(controller, movementScript));
        }
        else
        {
            Debug.LogError("O Respawn Point não foi encontrado! O jogador não pode ser teletransportado.");
        }

        currentHealth = characterData.maxHealth;

        // Reseta buffs ao morrer
        damageMultiplier = 1f;
        speedMultiplier = 1f;

        NotifyHealthChanged();
    }

    private IEnumerator ReactivatePlayer(CharacterController controller, MonoBehaviour movementScript)
    {
        yield return null;

        if (controller != null) controller.enabled = true;
        if (movementScript != null) movementScript.enabled = true;
    }

    void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke();
    }
}