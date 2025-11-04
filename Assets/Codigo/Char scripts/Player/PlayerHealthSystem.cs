using UnityEngine;
using System;
using System.Collections;

public class PlayerHealthSystem : MonoBehaviour
{
    public CharacterBase characterData;
    public float currentHealth;
    public bool isRegenerating;

    private float timeSinceLastDamage;
    private Transform respawnPoint;

    [Header("Configuração de Respawn")]
    [Tooltip("Nome/Tag do Transform do Respawn Point na cena atual. Ex: 'RespawnPoint'")]
    public string respawnPointNameOrTag = "RespawnPoint";

    public event Action OnHealthChanged;
    public event Action<float> OnDamageDealt;

    void Start()
    {
        currentHealth = characterData.maxHealth;
        NotifyHealthChanged();
        FindRespawnPoint();
    }

    void FindRespawnPoint()
    {
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
            Debug.LogWarning($"Respawn Point '{respawnPointNameOrTag}' NÃO foi encontrado na cena!");
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

    public void TriggerDamageDealt(float damageAmount)
    {
        OnDamageDealt?.Invoke(damageAmount);
    }

    void Die()
    {
        if (respawnPoint == null)
        {
            FindRespawnPoint();
        }

        if (respawnPoint != null)
        {
            CharacterController controller = GetComponent<CharacterController>();
            PlayerMovement movementScript = GetComponent<PlayerMovement>();

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
        NotifyHealthChanged();
    }

    private IEnumerator ReactivatePlayer(CharacterController controller, PlayerMovement movementScript)
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