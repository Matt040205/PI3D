using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class CommanderAbilityController : MonoBehaviour
{
    public CharacterBase characterData;
    private Animator anim;
    private PlayerHealthSystem playerHealth;

    public Dictionary<Ability, float> abilityCooldowns = new Dictionary<Ability, float>();

    public float ultimateChargeThreshold = 100f;
    public float currentUltimateCharge = 0f;

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
        anim = GetComponentInChildren<Animator>();

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
                if (abilityCooldowns[ability] < 0) abilityCooldowns[ability] = 0;
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
        if (ability == null || !abilityCooldowns.ContainsKey(ability))
            return;

        if (abilityCooldowns[ability] > 0)
            return;

        bool started = ability.Activate(gameObject);

        if (started)
        {
            if (anim != null)
            {
                if (ability == characterData.ability1)
                    anim.SetTrigger("Dash");
                else if (ability == characterData.ability2)
                    anim.SetTrigger("Meditar");
            }
        }
    }

    public void SetAbilityUsage(Ability ability, bool wasUsed)
    {
        if (ability == null || !abilityCooldowns.ContainsKey(ability)) return;

        if (wasUsed)
        {
            abilityCooldowns[ability] = ability.cooldown;
        }
        else
        {
            abilityCooldowns[ability] = 0f;
        }
    }

    public void ActivateUltimate()
    {
        if (characterData.ultimate == null || CurrentUltimateCharge < 1f)
            return;

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

    public void RefundCooldown(string keyword)
    {
        if (characterData.ability1 != null && characterData.ability1.name.Contains(keyword))
        {
            abilityCooldowns[characterData.ability1] = 0f;
        }
        else if (characterData.ability2 != null && characterData.ability2.name.Contains(keyword))
        {
            abilityCooldowns[characterData.ability2] = 0f;
        }
    }

    public float GetRemainingCooldownPercent(Ability ability)
    {
        if (ability == null || !abilityCooldowns.ContainsKey(ability) || ability.cooldown <= 0)
            return 0f;

        return abilityCooldowns[ability] / ability.cooldown;
    }

    public void ReduceAllAbilityCooldowns(float reductionAmount)
    {
        List<Ability> keys = new List<Ability>(abilityCooldowns.Keys);
        foreach (Ability ability in keys)
        {
            if (abilityCooldowns[ability] > 0)
            {
                abilityCooldowns[ability] = Mathf.Max(0, abilityCooldowns[ability] - reductionAmount);
            }
        }
    }

    public void ResetCooldown()
    {
        if (characterData.ability1 != null && abilityCooldowns.ContainsKey(characterData.ability1))
        {
            abilityCooldowns[characterData.ability1] = 0f;
        }
        if (characterData.ability2 != null && abilityCooldowns.ContainsKey(characterData.ability2))
        {
            abilityCooldowns[characterData.ability2] = 0f;
        }
    }

    public void ResetCooldown(Ability ability)
    {
        if (ability != null && abilityCooldowns.ContainsKey(ability))
        {
            abilityCooldowns[ability] = 0f;
        }
    }

    public void ResetCooldown(string keyword)
    {
        RefundCooldown(keyword);
    }
}