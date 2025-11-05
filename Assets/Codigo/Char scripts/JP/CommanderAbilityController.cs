using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CommanderAbilityController : MonoBehaviour
{
    public CharacterBase characterData;
    public Dictionary<Ability, float> abilityCooldowns = new Dictionary<Ability, float>();

    public float ultimateChargeThreshold = 100f;
    public float currentUltimateCharge = 0f;

    private PlayerHealthSystem playerHealth;

    public float CurrentUltimateCharge
    {
        get
        {
            if (ultimateChargeThreshold <= 0) return 0;
            return Mathf.Clamp01(currentUltimateCharge / ultimateChargeThreshold);
        }
    }

    void Start()
    {
        playerHealth = GetComponent<PlayerHealthSystem>();
        if (playerHealth != null)
        {
            playerHealth.OnDamageDealt += HandleDamageDealt;
        }

        if (characterData.ability1 != null)
        {
            abilityCooldowns[characterData.ability1] = 0;
            characterData.ability1.Initialize();
        }
        if (characterData.ability2 != null)
        {
            abilityCooldowns[characterData.ability2] = 0;
            characterData.ability2.Initialize();
        }
        if (characterData.ultimate != null)
        {
            abilityCooldowns[characterData.ultimate] = 0;
            characterData.ultimate.Initialize();
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDamageDealt -= HandleDamageDealt;
        }
    }

    void Update()
    {
        if (currentUltimateCharge < ultimateChargeThreshold && characterData != null && characterData.ultimateChargePerSecond > 0)
        {
            currentUltimateCharge += characterData.ultimateChargePerSecond * Time.deltaTime;
            currentUltimateCharge = Mathf.Min(currentUltimateCharge, ultimateChargeThreshold);
        }

        List<Ability> keys = new List<Ability>(abilityCooldowns.Keys);
        foreach (Ability ability in keys)
        {
            if (abilityCooldowns[ability] > 0)
            {
                abilityCooldowns[ability] -= Time.deltaTime;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ActivateAbility(characterData.ability1);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            ActivateAbility(characterData.ability2);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            ActivateUltimate();
        }
    }

    public void ActivateAbility(Ability ability)
    {
        if (ability == null || !abilityCooldowns.ContainsKey(ability) || abilityCooldowns[ability] > 0)
        {
            return;
        }

        bool shouldStartCooldown = ability.Activate(gameObject);

        if (shouldStartCooldown)
        {
            abilityCooldowns[ability] = ability.cooldown;
        }
        else
        {
            abilityCooldowns[ability] = 0;
        }
    }

    public float GetRemainingCooldownPercent(Ability ability)
    {
        if (ability == null || !abilityCooldowns.ContainsKey(ability) || ability.cooldown <= 0)
            return 0;

        return abilityCooldowns[ability] / ability.cooldown;
    }

    public void ActivateUltimate()
    {
        if (characterData.ultimate == null || CurrentUltimateCharge < 1f)
        {
            return;
        }

        bool shouldStartCooldown = characterData.ultimate.Activate(gameObject);
        if (shouldStartCooldown)
        {
            abilityCooldowns[characterData.ultimate] = characterData.ultimate.cooldown;
            currentUltimateCharge = 0f;
        }
    }

    private void HandleDamageDealt(float damage)
    {
        if (characterData != null && characterData.ultimateChargePerDamage > 0)
        {
            currentUltimateCharge += damage * characterData.ultimateChargePerDamage;
            currentUltimateCharge = Mathf.Min(currentUltimateCharge, ultimateChargeThreshold);
        }
    }

    public void ReduceAllAbilityCooldowns(float reductionAmount)
    {
        if (characterData.ability1 != null && abilityCooldowns.ContainsKey(characterData.ability1))
        {
            abilityCooldowns[characterData.ability1] = Mathf.Max(0, abilityCooldowns[characterData.ability1] - reductionAmount);
        }
        if (characterData.ability2 != null && abilityCooldowns.ContainsKey(characterData.ability2))
        {
            abilityCooldowns[characterData.ability2] = Mathf.Max(0, abilityCooldowns[characterData.ability2] - reductionAmount);
        }
    }
}