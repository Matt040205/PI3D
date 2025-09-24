using UnityEngine;

public class PlayerCombatManager : MonoBehaviour
{
    [Header("Referências")]
    public CharacterBase characterData;
    public PlayerShooting shootingSystem;
    public MeleeCombatSystem meleeSystem;
    public PlayerHealthSystem healthSystem;

    void Start()
    {
        if (characterData.combatType == CombatType.Ranged)
        {
            shootingSystem.enabled = true;
            meleeSystem.enabled = false;
            shootingSystem.characterData = characterData;
        }
        else // Melee
        {
            shootingSystem.enabled = false;
            meleeSystem.enabled = true;
            meleeSystem.characterData = characterData;
        }

        healthSystem.characterData = characterData;
    }

    void Update()
    {
        if (characterData.combatType == CombatType.Ranged && !shootingSystem.enabled)
        {
            shootingSystem.enabled = true;
            meleeSystem.enabled = false;
        }
        else if (characterData.combatType == CombatType.Melee && !meleeSystem.enabled)
        {
            shootingSystem.enabled = false;
            meleeSystem.enabled = true;
        }
    }
}