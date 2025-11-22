using UnityEngine;

public class PlayerCombatManager : MonoBehaviour
{
    [Header("Dados e Lógica")]
    public CharacterBase characterData;
    public PlayerShooting shootingSystem;
    public MeleeCombatSystem meleeSystem;
    public PlayerHealthSystem healthSystem;

    [Header("Visuals (Modelos 3D)")]
    // Arraste o GameObject da Katana (que está na mão) para cá
    public GameObject meleeWeaponModel;
    // Arraste o GameObject da SMG (que está na mão) para cá
    public GameObject rangedWeaponModel;

    void Start()
    {
        healthSystem.characterData = characterData;
        UpdateCombatState(); // Chama a função uma vez ao iniciar
    }

    void Update()
    {
        // Verifica se houve mudança no tipo de combate para atualizar
        // DICA: O ideal é não fazer isso todo frame no Update por performance,
        // mas para seu protótipo atual, vamos checar se o estado atual difere do desejado.

        if (characterData.combatType == CombatType.Ranged && (!shootingSystem.enabled || !rangedWeaponModel.activeSelf))
        {
            UpdateCombatState();
        }
        else if (characterData.combatType == CombatType.Melee && (!meleeSystem.enabled || !meleeWeaponModel.activeSelf))
        {
            UpdateCombatState();
        }
    }

    // Criei um método separado para organizar a troca
    void UpdateCombatState()
    {
        if (characterData.combatType == CombatType.Ranged)
        {
            // Lógica
            shootingSystem.enabled = true;
            meleeSystem.enabled = false;
            shootingSystem.characterData = characterData;

            // Visual
            if (meleeWeaponModel != null) meleeWeaponModel.SetActive(false); // Esconde a Katana
            if (rangedWeaponModel != null) rangedWeaponModel.SetActive(true);  // Mostra a SMG
        }
        else // Melee
        {
            // Lógica
            shootingSystem.enabled = false;
            meleeSystem.enabled = true;
            meleeSystem.characterData = characterData;

            // Visual
            if (meleeWeaponModel != null) meleeWeaponModel.SetActive(true);  // Mostra a Katana
            if (rangedWeaponModel != null) rangedWeaponModel.SetActive(false); // Esconde a SMG
        }
    }
}