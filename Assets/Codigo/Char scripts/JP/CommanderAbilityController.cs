using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using FMODUnity;

public class CommanderAbilityController : MonoBehaviour
{
    // --- Configuração ---
    public CharacterBase characterData;
    private Animator anim;
    private PlayerHealthSystem playerHealth;

    // --- Habilidades ---
    public Dictionary<Ability, float> abilityCooldowns = new Dictionary<Ability, float>();

    // --- Ultimate ---
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
        // --- Pegar Componentes ---
        playerHealth = GetComponent<PlayerHealthSystem>();
        anim = GetComponentInChildren<Animator>();

        // --- Eventos ---
        if (playerHealth != null)
        {
            playerHealth.OnDamageDealt += HandleDamageDealt;
        }

        // --- Inicializar Habilidades ---
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
        // --- Carregar Ultimate Passivamente ---
        if (currentUltimateCharge < ultimateChargeThreshold && characterData != null && characterData.ultimateChargePerSecond > 0)
        {
            currentUltimateCharge += characterData.ultimateChargePerSecond * Time.deltaTime;
            currentUltimateCharge = Mathf.Min(currentUltimateCharge, ultimateChargeThreshold);
        }

        // --- Contar Cooldowns ---
        List<Ability> keys = new List<Ability>(abilityCooldowns.Keys);
        foreach (Ability ability in keys)
        {
            if (abilityCooldowns[ability] > 0)
            {
                abilityCooldowns[ability] -= Time.deltaTime;
            }
        }

        // --- Inputs de Habilidade ---
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ActivateAbility(characterData.ability1); // Dash
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            ActivateAbility(characterData.ability2); // Meditar
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            ActivateUltimate(); // Modo Katana
        }
    }

    public void ActivateAbility(Ability ability)
    {
        if (ability == null || !abilityCooldowns.ContainsKey(ability) || abilityCooldowns[ability] > 0)
            return;

        bool shouldStartCooldown = ability.Activate(gameObject);

        if (shouldStartCooldown)
        {
            abilityCooldowns[ability] = ability.cooldown;

            if (ability == characterData.ability1)
                anim.SetTrigger("Dash");
            else if (ability == characterData.ability2)
                anim.SetTrigger("Meditar");
        }
        else
        {
            abilityCooldowns[ability] = 0;
        }
    }

    public void ActivateUltimate()
    {
        // A lógica do timer foi removida.
        // Apenas checamos a carga e ativamos a habilidade (que deve criar o NineTailsDanceLogic).
        if (characterData.ultimate == null || CurrentUltimateCharge < 1f)
            return;

        // Assumindo que "Activate" é o que adiciona o script "NineTailsDanceLogic"
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

    // --- Funções Auxiliares (Sem mudanças) ---
    public void ResetCooldown(Ability ability) { /*...*/ }
    public float GetRemainingCooldownPercent(Ability ability) { /*...*/ return 0; }
    public void ReduceAllAbilityCooldowns(float reductionAmount) { /*...*/ }
}