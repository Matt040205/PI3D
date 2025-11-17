using UnityEngine;
using System.Collections;

public class NineTailsDanceLogic : MonoBehaviour
{
    [Header("Configurações da Ultimate")]
    public float ultimateAttackSpeed = 5f; // Agora vai funcionar!
    public float ultimateAttackRange = 3f;
    public float ultimateAttackAngle = 360f;

    private PlayerCombatManager combatManager;
    private PlayerShooting shootingSystem;
    private MeleeCombatSystem meleeSystem;

    // !! MUDANÇA !! - Adicionada referência ao Animator
    private Animator anim;

    private float originalAttackRange;

    public void StartEffect(float duration)
    {
        combatManager = GetComponent<PlayerCombatManager>();
        shootingSystem = GetComponent<PlayerShooting>();
        meleeSystem = GetComponent<MeleeCombatSystem>();

        // !! MUDANÇA !! - Pegar o Animator
        anim = GetComponentInChildren<Animator>();

        if (combatManager != null && shootingSystem != null && meleeSystem != null && anim != null)
        {
            StartCoroutine(UltimateCoroutine(duration));
        }
        else
        {
            Debug.LogError("Não foi possível encontrar todos os componentes necessários para a Ultimate (inclusive o Animator). Abortando.");
            Destroy(this);
        }
    }

    private IEnumerator UltimateCoroutine(float duration)
    {
        Debug.Log("ULTIMATE ATIVADA: Dança das Nove Caudas!");

        combatManager.enabled = false;
        shootingSystem.enabled = false;
        meleeSystem.enabled = true;

        // !! MUDANÇA !! - Diz ao Animator para sacar a espada
        anim.SetBool("KatanaArmed", true);

        originalAttackRange = meleeSystem.attackRange;
        meleeSystem.attackRange = ultimateAttackRange;
        meleeSystem.overrideAttackAngle = ultimateAttackAngle;

        // Isto agora vai controlar a VELOCIDADE da animação
        meleeSystem.overrideAttackSpeed = ultimateAttackSpeed;

        yield return new WaitForSeconds(duration);

        Debug.Log("Ultimate finalizada.");

        // !! MUDANÇA !! - Diz ao Animator para guardar a espada
        anim.SetBool("KatanaArmed", false);

        if (meleeSystem != null)
        {
            meleeSystem.attackRange = originalAttackRange;
            meleeSystem.overrideAttackAngle = null;
            meleeSystem.overrideAttackSpeed = null; // Reseta a velocidade
        }

        combatManager.enabled = true;
        // O shootingSystem é re-ativado pelo combatManager? Se não, adicione:
        // shootingSystem.enabled = true; 

        Destroy(this);
    }
}