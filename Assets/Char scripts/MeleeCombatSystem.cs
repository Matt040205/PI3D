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

    private float nextAttackTime;

    void Update()
    {
        // Atualiza cooldown
        if (isAttacking)
        {
            attackCooldown -= Time.deltaTime;
            if (attackCooldown <= 0)
            {
                isAttacking = false;
            }
        }

        // Controle de ataque
        if (Input.GetButtonDown("Fire1") && Time.time >= nextAttackTime)
        {
            PerformMeleeAttack();
        }
    }

    void PerformMeleeAttack()
    {
        isAttacking = true;
        attackCooldown = 1f / characterData.attackSpeed;
        nextAttackTime = Time.time + attackCooldown;

        // Detecta alvos no arco de ataque
        Collider[] hitTargets = Physics.OverlapSphere(attackPoint.position, attackRange, hitLayers);

        foreach (Collider target in hitTargets)
        {
            // Verifica se está dentro do ângulo de ataque
            Vector3 directionToTarget = (target.transform.position - attackPoint.position).normalized;
            float angleToTarget = Vector3.Angle(attackPoint.forward, directionToTarget);

           if (angleToTarget < attackAngle / 2)
            {
                // Aplica dano
                EnemyHealthSystem enHealth = target.GetComponent<EnemyHealthSystem>();
                if (enHealth != null)
                {
                    enHealth.TakeDamage(characterData.damage);
                }
            }

        }

        // Animação/efeitos visuais aqui
    }

    // Visualização do Gizmo para debug
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        // Desenha o arco de ataque
        Vector3 leftBound = Quaternion.Euler(0, -attackAngle / 2, 0) * attackPoint.forward * attackRange;
        Vector3 rightBound = Quaternion.Euler(0, attackAngle / 2, 0) * attackPoint.forward * attackRange;

        Gizmos.DrawLine(attackPoint.position, attackPoint.position + leftBound);
        Gizmos.DrawLine(attackPoint.position, attackPoint.position + rightBound);
        Gizmos.DrawLine(attackPoint.position + leftBound, attackPoint.position + rightBound);
    }
}