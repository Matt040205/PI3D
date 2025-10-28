using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommanderAbilityController : MonoBehaviour
{
    [Header("Dados do Personagem")]
    public CharacterBase characterData;

    [Header("Ultimate Charge")]
    [Tooltip("A taxa de carregamento passivo da Ultimate por segundo (ex: 0.01f = 1% por segundo).")]
    public float ultimateChargeRatePerSecond = 0.01f;

    [Tooltip("O carregamento atual da Ultimate (0.0f = 0% a 1.0f = 100%).")]
    private float currentUltimateCharge = 0f;

    public float CurrentUltimateCharge { get { return currentUltimateCharge; } }
    public bool IsUltimateReady { get { return currentUltimateCharge >= 1f; } }

    private Dictionary<Ability, float> abilityCooldowns = new Dictionary<Ability, float>();

    void Start()
    {
        if (characterData.ability1 != null) abilityCooldowns[characterData.ability1] = 0f;
        if (characterData.ability2 != null) abilityCooldowns[characterData.ability2] = 0f;

        if (characterData.ultimate != null)
        {
            if (!abilityCooldowns.ContainsKey(characterData.ultimate))
            {
                abilityCooldowns[characterData.ultimate] = 0f;
            }
            currentUltimateCharge = 0f;
        }
    }

    void Update()
    {
        HandleAbilityInputs();

        if (characterData.ultimate != null && currentUltimateCharge < 1f)
        {
            currentUltimateCharge += ultimateChargeRatePerSecond * Time.deltaTime;
            currentUltimateCharge = Mathf.Clamp01(currentUltimateCharge);
        }
    }

    private void HandleAbilityInputs()
    {
        if (Input.GetKeyDown(KeyCode.Q) && characterData.ability1 != null)
        {
            TryActivateAbility(characterData.ability1);
        }

        if (Input.GetKeyDown(KeyCode.E) && characterData.ability2 != null)
        {
            TryActivateAbility(characterData.ability2);
        }

        if (Input.GetKeyDown(KeyCode.X) && characterData.ultimate != null)
        {
            TryActivateAbility(characterData.ultimate);
        }
    }

    private void TryActivateAbility(Ability ability)
    {
        if (ability == characterData.ultimate)
        {
            if (IsUltimateReady)
            {
                bool shouldGoOnCooldown = ability.Activate(this.gameObject);
                if (shouldGoOnCooldown)
                {
                    currentUltimateCharge = 0f;
                    Debug.Log("Ultimate ativada com sucesso! Carga reiniciada.");
                }
            }
            else
            {
                Debug.Log($"Ultimate {ability.name} carregando: {CurrentUltimateCharge:P2}");
            }
            return;
        }

        if (abilityCooldowns.ContainsKey(ability) && Time.time >= abilityCooldowns[ability])
        {
            bool shouldGoOnCooldown = ability.Activate(this.gameObject);

            if (shouldGoOnCooldown)
            {
                abilityCooldowns[ability] = Time.time + ability.cooldown;
            }
        }
        else
        {
            Debug.Log("Habilidade " + ability.name + " em recarga!");
        }
    }

    public void AddUltimateCharge(float amount)
    {
        if (characterData.ultimate == null) return;
        currentUltimateCharge += amount;
        currentUltimateCharge = Mathf.Clamp01(currentUltimateCharge);
    }

    public float GetRemainingCooldownPercent(Ability ability)
    {
        if (ability == null || ability == characterData.ultimate || ability.cooldown <= 0) return 0f;

        if (abilityCooldowns.ContainsKey(ability))
        {
            float remainingTime = abilityCooldowns[ability] - Time.time;
            return Mathf.Clamp01(remainingTime / ability.cooldown);
        }
        return 0f;
    }

    public void ReduceAllAbilityCooldowns(float percent)
    {
        List<Ability> abilitiesOnCooldown = new List<Ability>(abilityCooldowns.Keys);

        foreach (var ability in abilitiesOnCooldown)
        {
            if (ability == characterData.ultimate) continue;

            float remainingTime = abilityCooldowns[ability] - Time.time;
            if (remainingTime > 0)
            {
                abilityCooldowns[ability] -= remainingTime * percent;
            }
        }
        Debug.Log("Cooldowns reduzidos em " + (percent * 100) + "%! A Ultimate usa um sistema de carga separado.");
    }
}