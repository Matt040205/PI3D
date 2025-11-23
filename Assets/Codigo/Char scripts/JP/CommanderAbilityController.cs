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
        // Criamos uma lista das chaves para evitar erros de modificação durante o loop, 
        // embora estejamos modificando valores, é uma boa prática.
        List<Ability> keys = new List<Ability>(abilityCooldowns.Keys);
        foreach (Ability ability in keys)
        {
            if (abilityCooldowns[ability] > 0)
            {
                abilityCooldowns[ability] -= Time.deltaTime;
                // Garante que não fique negativo
                if (abilityCooldowns[ability] < 0) abilityCooldowns[ability] = 0;
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
        if (ability == null || !abilityCooldowns.ContainsKey(ability))
            return;

        // Se o cooldown for maior que 0, não pode usar
        if (abilityCooldowns[ability] > 0)
            return;

        bool shouldStartCooldown = ability.Activate(gameObject);

        if (shouldStartCooldown)
        {
            abilityCooldowns[ability] = ability.cooldown;

            if (anim != null)
            {
                if (ability == characterData.ability1)
                    anim.SetTrigger("Dash");
                else if (ability == characterData.ability2)
                    anim.SetTrigger("Meditar");
            }
        }
        else
        {
            // Se a habilidade retornou false (ex: dash resetou ao matar), zera o cooldown
            abilityCooldowns[ability] = 0;
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

    // --- Funções Auxiliares (AGORA IMPLEMENTADAS) ---

    // Chamado pelo Dash quando mata alguém
    public void ResetCooldown(Ability ability)
    {
        if (ability != null && abilityCooldowns.ContainsKey(ability))
        {
            abilityCooldowns[ability] = 0f;
        }
    }

    // Chamado pela UI para desenhar a bolinha escura
    public float GetRemainingCooldownPercent(Ability ability)
    {
        if (ability == null || !abilityCooldowns.ContainsKey(ability) || ability.cooldown <= 0)
            return 0f;

        // Retorna um valor entre 0 e 1 (ex: 0.5 = metade do tempo passou)
        return abilityCooldowns[ability] / ability.cooldown;
    }

    // Pode ser usado por itens que diminuem cooldown
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
}