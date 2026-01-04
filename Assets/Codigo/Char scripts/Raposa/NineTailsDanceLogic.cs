using UnityEngine;
using System.Collections;

public class NineTailsDanceLogic : MonoBehaviour
{
    [Header("Configurações da Ultimate")]
    public float ultimateAttackSpeed = 5f;
    public float ultimateAttackRange = 3f;
    public float ultimateAttackAngle = 360f;

    private PlayerCombatManager combatManager;
    private PlayerShooting shootingSystem;
    private MeleeCombatSystem meleeSystem;
    private Animator anim;

    // Para guardar o estado anterior e restaurar depois
    private float originalAttackRange;
    private CombatType previousCombatType;

    public void StartEffect(float duration)
    {
        combatManager = GetComponent<PlayerCombatManager>();
        shootingSystem = GetComponent<PlayerShooting>();
        meleeSystem = GetComponent<MeleeCombatSystem>();
        anim = GetComponentInChildren<Animator>();

        if (combatManager != null && shootingSystem != null && meleeSystem != null && anim != null)
        {
            StartCoroutine(UltimateCoroutine(duration));
        }
        else
        {
            Debug.LogError("Faltam componentes para a Ultimate. Verifique se PlayerCombatManager tem as referências.");
            Destroy(this);
        }
    }

    private IEnumerator UltimateCoroutine(float duration)
    {
        Debug.Log("ULTIMATE ATIVADA: Dança das Nove Caudas!");

        // 1. Salvar o estado atual (para saber se voltamos pra SMG ou Katana depois)
        if (combatManager.characterData != null)
        {
            previousCombatType = combatManager.characterData.combatType;
            // Força a mudança de DADO para Melee
            combatManager.characterData.combatType = CombatType.Melee;
        }

        // 2. NÃO desative o combatManager. Deixe ele rodar para manter a lógica funcionando.
        // combatManager.enabled = false; <--- REMOVIDO

        // 3. Forçar a troca visual IMEDIATA (Garantia)
        // Acessamos as variáveis públicas que criamos no PlayerCombatManager
        if (combatManager.meleeWeaponModel != null) combatManager.meleeWeaponModel.SetActive(true);
        if (combatManager.rangedWeaponModel != null) combatManager.rangedWeaponModel.SetActive(false);

        // Desativa tiro e Ativa melee manualmente (redundância de segurança)
        shootingSystem.enabled = false;
        meleeSystem.enabled = true;

        // Animação
        anim.SetBool("KatanaArmed", true);

        // Configurações de "Buff" da Ultimate
        originalAttackRange = meleeSystem.attackRange;
        meleeSystem.attackRange = ultimateAttackRange;
        meleeSystem.overrideAttackAngle = ultimateAttackAngle;
        meleeSystem.overrideAttackSpeed = ultimateAttackSpeed;

        // --- ESPERA O TEMPO DA ULTIMATE ---
        yield return new WaitForSeconds(duration);

        Debug.Log("Ultimate finalizada.");

        // 4. Restaura o estado original
        anim.SetBool("KatanaArmed", false);

        if (meleeSystem != null)
        {
            meleeSystem.attackRange = originalAttackRange;
            meleeSystem.overrideAttackAngle = null;
            meleeSystem.overrideAttackSpeed = null;
        }

        // Restaura o tipo de combate (Se era Ranged antes, volta a ser Ranged)
        if (combatManager.characterData != null)
        {
            combatManager.characterData.combatType = previousCombatType;
        }

        // O PlayerCombatManager vai perceber a mudança no Update e trocar o visual de volta automaticamente.
        // Mas para garantir que não fique 1 frame sem arma, forçamos a atualização visual se voltarmos para Ranged:
        if (previousCombatType == CombatType.Ranged)
        {
            if (combatManager.meleeWeaponModel != null) combatManager.meleeWeaponModel.SetActive(false);
            if (combatManager.rangedWeaponModel != null) combatManager.rangedWeaponModel.SetActive(true);
        }

        Destroy(this);
    }
}