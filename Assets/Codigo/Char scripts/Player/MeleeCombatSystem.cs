using UnityEngine;

public class MeleeCombatSystem : MonoBehaviour
{
    [Header("Configurações")]
    public CharacterBase characterData;
    public Transform attackPoint;
    public float attackRange = 2f;
    public float attackAngle = 90f;
    public LayerMask hitLayers;

    [Header("Estado")]
    public bool isAttacking;
    public float attackCooldown;

    [Header("Overrides da Ultimate (Não mexer)")]
    public float? overrideAttackSpeed = null;
    public float? overrideAttackAngle = null;

    private float nextAttackTime;

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
                    enHealth.TakeDamage(characterData.damage);
                    Debug.Log($"<color=cyan>ATAQUE MELEE:</color> Causou {characterData.damage} de dano em {target.name}");
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