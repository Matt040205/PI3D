using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class AbilityManager : MonoBehaviour
{
    [Header("Habilidades Desbloqueadas")]
    public List<AbilitySO> unlockedAbilities = new List<AbilitySO>();

    [Header("Configuração")]
    public KeyCode[] abilityKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };

    // Eventos para UI
    public event Action<AbilitySO> OnAbilityUnlocked;
    public event Action<AbilitySO> OnAbilityUsed;
    public event Action<AbilitySO> OnAbilityCooldownStart;

    private Dictionary<AbilitySO, float> abilityCooldowns = new Dictionary<AbilitySO, float>();
    private CharacterStatsBridge statsBridge;

    void Start()
    {
        statsBridge = GetComponent<CharacterStatsBridge>();
    }

    void Update()
    {
        UpdateCooldowns();
        HandleAbilityInput();
    }

    void UpdateCooldowns()
    {
        List<AbilitySO> abilities = new List<AbilitySO>(abilityCooldowns.Keys);
        foreach (AbilitySO ability in abilities)
        {
            if (abilityCooldowns[ability] > 0)
            {
                abilityCooldowns[ability] -= Time.deltaTime;

                if (abilityCooldowns[ability] <= 0)
                {
                    abilityCooldowns[ability] = 0;
                    // Cooldown completo, poderia disparar evento aqui
                }
            }
        }
    }

    void HandleAbilityInput()
    {
        for (int i = 0; i < abilityKeys.Length; i++)
        {
            if (Input.GetKeyDown(abilityKeys[i]))
            {
                if (i < unlockedAbilities.Count)
                {
                    TryUseAbility(unlockedAbilities[i]);
                }
            }
        }
    }

    public void TryUseAbility(AbilitySO ability)
    {
        if (ability == null) return;

        // Verifica se a habilidade está em cooldown
        if (abilityCooldowns.ContainsKey(ability) && abilityCooldowns[ability] > 0)
            return;

        // Verifica se é uma habilidade ativa ou ultimate
        if (ability.abilityType != AbilityType.Active &&
            ability.abilityType != AbilityType.Ultimate)
            return;

        UseAbility(ability);
    }

    public void UseAbility(AbilitySO ability)
    {
        // Aplica os efeitos da habilidade
        ApplyAbilityEffects(ability);

        // Configura cooldown
        abilityCooldowns[ability] = ability.cooldown;

        // Instancia VFX/SFX
        if (ability.effectPrefab != null)
            Instantiate(ability.effectPrefab, transform.position, Quaternion.identity);

        if (ability.vfxOnCast != null)
            Instantiate(ability.vfxOnCast, transform.position, Quaternion.identity);

        if (ability.sfx != null)
            AudioSource.PlayClipAtPoint(ability.sfx, transform.position);

        // Dispara eventos
        OnAbilityUsed?.Invoke(ability);
        OnAbilityCooldownStart?.Invoke(ability);

        Debug.Log($"Habilidade usada: {ability.abilityName}");
    }

    void ApplyAbilityEffects(AbilitySO ability)
    {
        // Implemente efeitos específicos de cada habilidade aqui
        // Esta é uma implementação genérica - você precisará expandir para cada habilidade

        switch (ability.abilityType)
        {
            case AbilityType.Passive:
                // Habilidades passivas são aplicadas automaticamente quando desbloqueadas
                break;

            case AbilityType.Active:
                // Exemplo: cura
                if (ability.abilityName.Contains("Cura") || ability.abilityName.Contains("Heal"))
                {
                    PlayerHealthSystem health = GetComponent<PlayerHealthSystem>();
                    if (health != null)
                        health.Heal(statsBridge.currentStats.maxHealth * 0.3f);
                }
                // Exemplo: aumento de velocidade
                else if (ability.abilityName.Contains("Velocidade") || ability.abilityName.Contains("Speed"))
                {
                    StartCoroutine(ApplyTemporarySpeedBoost(ability));
                }
                break;

            case AbilityType.Ultimate:
                // Implemente efeitos de ultimate aqui
                break;
        }
    }

    IEnumerator ApplyTemporarySpeedBoost(AbilitySO ability)
    {
        float originalSpeed = statsBridge.currentStats.moveSpeed;
        statsBridge.currentStats.moveSpeed *= 1.5f; // 50% de aumento
        statsBridge.OnStatsUpdated();

        yield return new WaitForSeconds(5f); // Duração do efeito

        statsBridge.currentStats.moveSpeed = originalSpeed;
        statsBridge.OnStatsUpdated();
    }

    public void UnlockAbility(AbilitySO ability)
    {
        if (!unlockedAbilities.Contains(ability))
        {
            unlockedAbilities.Add(ability);

            // Habilidades passivas são aplicadas imediatamente
            if (ability.abilityType == AbilityType.Passive)
            {
                ApplyAbilityEffects(ability);
            }

            // Dispara evento
            OnAbilityUnlocked?.Invoke(ability);

            Debug.Log($"Habilidade desbloqueada: {ability.abilityName}");
        }
    }

    public float GetCooldown(AbilitySO ability)
    {
        if (abilityCooldowns.ContainsKey(ability))
            return abilityCooldowns[ability];
        return 0f;
    }

    public bool IsAbilityReady(AbilitySO ability)
    {
        return GetCooldown(ability) <= 0;
    }
}