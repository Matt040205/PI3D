using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommanderAbilityController : MonoBehaviour
{
    [Header("Dados do Personagem")]
    public CharacterBase characterData;

    [Header("Referências de Componentes")]
    public PlayerShooting shootingSystem;
    public MeleeCombatSystem meleeSystem;

    // Componentes que serão pegos automaticamente no Start
    private PlayerHealthSystem healthSystem;
    private PlayerCombatManager combatManager;

    // Dicionário para gerenciar os tempos de recarga de cada habilidade
    private Dictionary<Ability, float> abilityCooldowns = new Dictionary<Ability, float>();

    void Start()
    {
        // Pega as referências dos componentes que estão no mesmo GameObject
        healthSystem = GetComponent<PlayerHealthSystem>();
        combatManager = GetComponent<PlayerCombatManager>();

        // Inicializa o dicionário de cooldowns, garantindo que as habilidades possam ser usadas no início
        if (characterData.ability1 != null) abilityCooldowns[characterData.ability1] = 0f;
        if (characterData.ability2 != null) abilityCooldowns[characterData.ability2] = 0f;
        if (characterData.ultimate != null) abilityCooldowns[characterData.ultimate] = 0f;
    }

    void Update()
    {
        // Ouve os inputs do jogador para ativar as habilidades
        HandleAbilityInputs();
    }

    private void HandleAbilityInputs()
    {
        // Input para Habilidade 1 (Ex: Tecla Q)
        if (Input.GetKeyDown(KeyCode.Q) && characterData.ability1 != null)
        {
            TryActivateAbility(characterData.ability1);
        }

        // Input para Habilidade 2 (Ex: Tecla E)
        if (Input.GetKeyDown(KeyCode.E) && characterData.ability2 != null)
        {
            TryActivateAbility(characterData.ability2);
        }

        // Input para Ultimate (Ex: Tecla X)
        if (Input.GetKeyDown(KeyCode.X) && characterData.ultimate != null)
        {
            TryActivateAbility(characterData.ultimate);
        }
    }

    private void TryActivateAbility(Ability ability)
    {
        // Verifica se a habilidade existe no dicionário e se o tempo de recarga já passou
        if (abilityCooldowns.ContainsKey(ability) && Time.time >= abilityCooldowns[ability])
        {
            // Ativa a habilidade e recebe 'true' ou 'false' como resposta
            bool shouldGoOnCooldown = ability.Activate(this.gameObject);

            // Se a habilidade retornar 'true', ela entra em recarga
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

    #region Funções Chamadas pelas Habilidades

    // Esta função é chamada pela "receita" da habilidade de cura
    public void StartHealOverTime(float totalHeal, float duration)
    {
        StartCoroutine(HealCoroutine(totalHeal, duration));
    }

    // Esta função é chamada pela "receita" da habilidade Ultimate
    public void StartUltimate(float duration, float reductionPercent)
    {
        StartCoroutine(UltimateCoroutine(duration, reductionPercent));
    }

    #endregion

    #region Coroutines (Lógicas de Habilidades com Duração)

    private IEnumerator HealCoroutine(float totalHeal, float duration)
    {
        float healPerSecond = totalHeal / duration;
        float timeLeft = duration;

        while (timeLeft > 0)
        {
            if (healthSystem != null)
            {
                healthSystem.Heal(healPerSecond * Time.deltaTime);
            }
            timeLeft -= Time.deltaTime;
            yield return null; // Espera até o próximo frame
        }
    }

    private IEnumerator UltimateCoroutine(float duration, float reductionPercent)
    {
        Debug.Log("ULTIMATE ATIVADA: Dança das Nove Caudas!");

        // 1. Desativa o gerenciador de combate para assumirmos o controle manual
        if (combatManager != null) combatManager.enabled = false;

        // 2. Troca para o modo de combate corpo a corpo
        if (shootingSystem != null) shootingSystem.enabled = false;
        if (meleeSystem != null) meleeSystem.enabled = true;

        // 3. Reduz o cooldown das outras habilidades
        ReduceCooldown(characterData.ability1, reductionPercent);
        ReduceCooldown(characterData.ability2, reductionPercent);

        // 4. Concede imunidade (se você tiver um sistema para isso)
        // Ex: GetComponent<StatusController>().isImmune = true;

        // 5. Espera a duração da ultimate acabar
        yield return new WaitForSeconds(duration);

        // 6. Reverte tudo para o estado normal
        Debug.Log("Ultimate finalizada.");
        // Ex: GetComponent<StatusController>().isImmune = false;

        // 7. Reativa o gerenciador de combate para ele reassumir o controle
        if (combatManager != null)
        {
            combatManager.enabled = true;
            // Forçamos uma atualização para garantir que o estado de combate correto seja ativado
            if (characterData.combatType == CombatType.Ranged)
            {
                if (shootingSystem != null) shootingSystem.enabled = true;
                if (meleeSystem != null) meleeSystem.enabled = false;
            }
            else
            {
                if (shootingSystem != null) shootingSystem.enabled = false;
                if (meleeSystem != null) meleeSystem.enabled = true;
            }
        }
    }

    #endregion

    #region Métodos de Ajuda

    // Função auxiliar para reduzir o tempo de recarga de uma habilidade
    private void ReduceCooldown(Ability ability, float percent)
    {
        if (ability != null && abilityCooldowns.ContainsKey(ability))
        {
            float remainingTime = abilityCooldowns[ability] - Time.time;
            if (remainingTime > 0)
            {
                // Reduz o tempo restante pela porcentagem definida
                abilityCooldowns[ability] -= remainingTime * percent;
            }
        }
    }

    #endregion
}