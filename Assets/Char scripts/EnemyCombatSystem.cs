using UnityEngine;

public class EnemyCombatSystem : MonoBehaviour
{
    [Header("Configura��es de Combate")]
    public float attackRange = 2f;
    public float attackCooldown = 1f;

    [Header("Refer�ncias")]
    public Transform attackPoint;
    public LayerMask playerLayer;

    // Componentes
    private EnemyController enemyController;
    private EnemyDataSO enemyData;
    private Transform player;

    // Estado
    private bool canAttack = true;
    private bool isAttacking = false;
    private float currentAttackCooldown;

    void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Start()
    {
        // Pega os dados do inimigo do EnemyController
        if (enemyController != null)
        {
            enemyData = enemyController.enemyData;
        }
    }

    void Update()
    {
        if (enemyController == null || enemyController.IsDead) return;

        // Atualiza o cooldown do ataque
        if (!canAttack)
        {
            currentAttackCooldown -= Time.deltaTime;
            if (currentAttackCooldown <= 0)
            {
                canAttack = true;
            }
        }
    }

    public void TryAttack()
    {
        if (canAttack && IsPlayerInAttackRange())
        {
            StartAttack();
        }
    }

    bool IsPlayerInAttackRange()
    {
        if (player == null) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer <= attackRange;
    }

    void StartAttack()
    {
        isAttacking = true;
        canAttack = false;
        currentAttackCooldown = attackCooldown;

        // Inicia a anima��o de ataque (se tiver)
        // animator.SetTrigger("Attack");

        // Aplica o dano (ajuste o timing conforme sua anima��o)
        Invoke("ApplyDamage", 0.5f); // Ajuste este tempo para coincidir com a anima��o
    }

    void ApplyDamage()
    {
        if (!isAttacking) return;

        // Detecta se o jogador est� dentro do alcance no momento do ataque
        Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, attackRange, playerLayer);

        foreach (Collider playerCollider in hitPlayers)
        {
            PlayerHealthSystem playerHealth = playerCollider.GetComponent<PlayerHealthSystem>();
            if (playerHealth != null)
            {
                // Calcula dano baseado nos dados do inimigo
                float finalDamage = enemyData.baseATQ + (enemyController.nivel * enemyData.atqPerLevel);

                playerHealth.TakeDamage(finalDamage);
                Debug.Log("Inimigo causou " + finalDamage + " de dano ao jogador");
            }
        }

        isAttacking = false;
    }

    // Visualiza��o do alcance de ataque no editor
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}