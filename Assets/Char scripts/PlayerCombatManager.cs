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
        // Configura o sistema apropriado baseado no tipo de combate
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

        // Configura o sistema de saúde
       healthSystem.characterData = characterData;
    }

    void Update()
    {
        // Atualiza dinamicamente se o tipo mudar (para debug/testes)
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