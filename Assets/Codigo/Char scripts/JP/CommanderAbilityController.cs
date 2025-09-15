using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// AGORA ESTE SCRIPT É GENÉRICO. Ele funcionará para QUALQUER personagem
// sem precisar de modificações para novas habilidades.
public class CommanderAbilityController : MonoBehaviour
{
    [Header("Dados do Personagem")]
    public CharacterBase characterData;

    // Não precisamos mais de referências diretas a sistemas de combate aqui
    // para a lógica de habilidades.

    private Dictionary<Ability, float> abilityCooldowns = new Dictionary<Ability, float>();

    void Start()
    {
        if (characterData.ability1 != null) abilityCooldowns[characterData.ability1] = 0f;
        if (characterData.ability2 != null) abilityCooldowns[characterData.ability2] = 0f;
        if (characterData.ultimate != null) abilityCooldowns[characterData.ultimate] = 0f;
    }

    void Update()
    {
        HandleAbilityInputs();
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
    public void ReduceAllAbilityCooldowns(float percent)
    {
        // Cria uma lista temporária para evitar problemas ao modificar o dicionário enquanto itera
        List<Ability> abilitiesOnCooldown = new List<Ability>(abilityCooldowns.Keys);

        foreach (var ability in abilitiesOnCooldown)
        {
            float remainingTime = abilityCooldowns[ability] - Time.time;
            if (remainingTime > 0)
            {
                // Reduz o tempo restante pela porcentagem definida
                abilityCooldowns[ability] -= remainingTime * percent;
            }
        }
        Debug.Log("Cooldowns reduzidos em " + (percent * 100) + "%!");
    }

}