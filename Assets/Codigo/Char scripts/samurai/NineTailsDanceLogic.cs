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

    private float originalAttackRange;

    public void StartEffect(float duration)
    {
        combatManager = GetComponent<PlayerCombatManager>();
        shootingSystem = GetComponent<PlayerShooting>();
        meleeSystem = GetComponent<MeleeCombatSystem>();

        if (combatManager != null && shootingSystem != null && meleeSystem != null)
        {
            StartCoroutine(UltimateCoroutine(duration));
        }
        else
        {
            Debug.LogError("Não foi possível encontrar todos os componentes necessários para a Ultimate. Abortando.");
            Destroy(this);
        }
    }

    private IEnumerator UltimateCoroutine(float duration)
    {
        Debug.Log("ULTIMATE ATIVADA: Dança das Nove Caudas!");

        combatManager.enabled = false;

        shootingSystem.enabled = false;
        meleeSystem.enabled = true;

        originalAttackRange = meleeSystem.attackRange;

        meleeSystem.attackRange = ultimateAttackRange;
        meleeSystem.overrideAttackAngle = ultimateAttackAngle;
        meleeSystem.overrideAttackSpeed = ultimateAttackSpeed;

        yield return new WaitForSeconds(duration);

        Debug.Log("Ultimate finalizada.");

        if (meleeSystem != null)
        {
            meleeSystem.attackRange = originalAttackRange;
            meleeSystem.overrideAttackAngle = null;
            meleeSystem.overrideAttackSpeed = null;
        }

        combatManager.enabled = true;

        Destroy(this);
    }
}