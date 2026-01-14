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
            Destroy(this);
        }
    }

    private IEnumerator UltimateCoroutine(float duration)
    {
        if (combatManager.characterData != null)
        {
            previousCombatType = combatManager.characterData.combatType;
            combatManager.characterData.combatType = CombatType.Melee;
        }

        if (combatManager.meleeWeaponModel != null) combatManager.meleeWeaponModel.SetActive(true);
        if (combatManager.rangedWeaponModel != null) combatManager.rangedWeaponModel.SetActive(false);

        shootingSystem.enabled = false;
        meleeSystem.enabled = true;

        anim.SetBool("KatanaArmed", true);

        if (meleeSystem != null && meleeSystem.swordStats != null)
        {
            originalAttackRange = meleeSystem.swordStats.attackRange;
            meleeSystem.swordStats.attackRange = ultimateAttackRange;

            meleeSystem.overrideAttackAngle = ultimateAttackAngle;
            meleeSystem.overrideAttackSpeed = ultimateAttackSpeed;
        }

        yield return new WaitForSeconds(duration);

        anim.SetBool("KatanaArmed", false);

        if (meleeSystem != null && meleeSystem.swordStats != null)
        {
            meleeSystem.swordStats.attackRange = originalAttackRange;
            meleeSystem.overrideAttackAngle = null;
            meleeSystem.overrideAttackSpeed = null;
        }

        if (combatManager.characterData != null)
        {
            combatManager.characterData.combatType = previousCombatType;
        }

        if (previousCombatType == CombatType.Ranged)
        {
            if (combatManager.meleeWeaponModel != null) combatManager.meleeWeaponModel.SetActive(false);
            if (combatManager.rangedWeaponModel != null) combatManager.rangedWeaponModel.SetActive(true);
        }

        Destroy(this);
    }
}