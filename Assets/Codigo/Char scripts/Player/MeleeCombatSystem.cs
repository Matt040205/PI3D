using UnityEngine;
using FMODUnity;

public class MeleeCombatSystem : MonoBehaviour
{
    [Header("Configurações")]
    public CharacterBase characterData;
    public Transform attackPoint;
    public float attackRange = 2f;
    public float attackAngle = 90f;
    public LayerMask hitLayers;

    [Header("Dano (Overrides da Ultimate)")]
    public float damageCombo1 = 13f;
    public float damageCombo2 = 15f;
    public float damageCombo3 = 22f;

    [Header("FMOD (Combo da Ultimate)")]
    [EventRef]
    public string eventoEspada1 = "event:/SFX/Espada";
    [EventRef]
    public string eventoEspada2 = "event:/SFX/Espada_1";
    [EventRef]
    public string eventoEspada3 = "event:/SFX/Espada_2";
    public float comboResetTime = 1.5f;

    [Header("Estado")]
    public bool isAttacking;
    public float attackCooldown;

    [Header("Overrides da Ultimate (Não mexer)")]
    public float? overrideAttackSpeed = null;
    public float? overrideAttackAngle = null;

    private float nextAttackTime;
    private int comboCounter = 0;
    private float lastAttackTimestamp = 0f;

    void OnEnable()
    {
        comboCounter = 0;
    }

    void Update()
    {
        if (isAttacking)
        {
            attackCooldown -= Time.deltaTime;
            if (attackCooldown <= 0)
            {
                isAttacking = false;
            }
        }

        if (Time.time - lastAttackTimestamp > comboResetTime && comboCounter != 0)
        {
            comboCounter = 0;
        }

        if (Input.GetButtonDown("Fire1") && Time.time >= nextAttackTime)
        {
            PerformMeleeAttack();
        }
    }

    void PerformMeleeAttack()
    {
        float currentSpeed = overrideAttackSpeed ?? characterData.attackSpeed;
        float currentAngle = overrideAttackAngle ?? this.attackAngle;

        isAttacking = true;
        attackCooldown = 1f / currentSpeed;
        nextAttackTime = Time.time + attackCooldown;
        lastAttackTimestamp = Time.time;

        float damageToApply = characterData.damage;

        switch (comboCounter)
        {
            case 0:
                if (!string.IsNullOrEmpty(eventoEspada1))
                    RuntimeManager.PlayOneShot(eventoEspada1, transform.position);
                damageToApply = damageCombo1;
                comboCounter = 1;
                break;
            case 1:
                if (!string.IsNullOrEmpty(eventoEspada2))
                    RuntimeManager.PlayOneShot(eventoEspada2, transform.position);
                damageToApply = damageCombo2;
                comboCounter = 2;
                break;
            case 2:
                if (!string.IsNullOrEmpty(eventoEspada3))
                    RuntimeManager.PlayOneShot(eventoEspada3, transform.position);
                damageToApply = damageCombo3;
                comboCounter = 0;
                break;
        }

        Collider[] hitTargets = Physics.OverlapSphere(attackPoint.position, attackRange, hitLayers);

        foreach (Collider target in hitTargets)
        {
            Vector3 directionToTarget = (target.transform.position - attackPoint.position).normalized;
            float angleToTarget = Vector3.Angle(attackPoint.forward, directionToTarget);

            if (angleToTarget < currentAngle / 2)
            {
                EnemyHealthSystem enHealth = target.GetComponent<EnemyHealthSystem>();
                if (enHealth != null)
                {
                    enHealth.TakeDamage(damageToApply);
                    Debug.Log($"<color=cyan>ATAQUE MELEE (Combo {comboCounter}):</color> Causou {damageToApply} de dano em {target.name}");
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        float currentAngle = overrideAttackAngle ?? this.attackAngle;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        Vector3 leftBound = Quaternion.Euler(0, -currentAngle / 2, 0) * attackPoint.forward * attackRange;
        Vector3 rightBound = Quaternion.Euler(0, currentAngle / 2, 0) * attackPoint.forward * attackRange;

        Gizmos.DrawLine(attackPoint.position, attackPoint.position + leftBound);
        Gizmos.DrawLine(attackPoint.position, attackPoint.position + rightBound);
        Gizmos.DrawLine(attackPoint.position + leftBound, attackPoint.position + rightBound);
    }
}