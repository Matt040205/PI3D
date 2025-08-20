using UnityEngine;
using System.Collections.Generic;

// O enum não precisa de mudanças
public enum AITargetPriority
{
    Player,
    Objective
}

public class EnemyController : MonoBehaviour
{
    [Header("Dados do Inimigo")]
    public EnemyDataSO enemyData;

    [Header("Status Atual")]
    public int nivel = 1;

    [Header("Pontos de Patrulha")]
    public List<Transform> patrolPoints;

    [Header("Inteligência Artificial")]
    public AITargetPriority mainPriority = AITargetPriority.Objective;
    public float selfDefenseRadius = 5f;

    // Componentes
    private EnemyHealthSystem healthSystem;
    private EnemyCombatSystem combatSystem;
    private Rigidbody rb;

    // Status calculados
    private float currentDamage;
    private float currentMoveSpeed;

    // Variáveis de comportamento
    private int currentPointIndex = 0;
    private Transform target; // O alvo será quase sempre o jogador, ou null.

    [Header("Configurações")]
    public float chaseDistance = 50f;
    public float attackDistance = 2f;

    // Referência para o jogador
    private Transform playerTransform;

    // Propriedades
    public bool IsDead { get { return healthSystem.isDead; } }
    public Transform Target { get { return target; } }

    void Awake()
    {
        healthSystem = GetComponent<EnemyHealthSystem>();
        combatSystem = GetComponent<EnemyCombatSystem>();
        rb = GetComponent<Rigidbody>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.useGravity = true;
        }
    }

    void OnEnable()
    {
        InitializeEnemy();
    }

    void FixedUpdate()
    {
        if (IsDead) return;

        // 1. Decide se deve focar no jogador
        DecideTarget();

        // 2. Age com base na decisão
        if (target != null)
        {
            // Se o alvo for o jogador, persiga-o.
            ChaseTarget();
        }
        else
        {
            // Se não houver alvo, siga o caminho de patrulha.
            Patrol();
        }
    }

    // MODIFICADO: Lógica de decisão simplificada
    private void DecideTarget()
    {
        if (playerTransform == null)
        {
            target = null;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Se a prioridade é o jogador, use a distância de perseguição normal
        if (mainPriority == AITargetPriority.Player && distanceToPlayer <= chaseDistance)
        {
            target = playerTransform;
            return;
        }

        // Se a prioridade é o objetivo, o inimigo só ataca o jogador por autodefesa (se ele chegar perto)
        if (mainPriority == AITargetPriority.Objective && distanceToPlayer <= selfDefenseRadius)
        {
            target = playerTransform;
            return;
        }

        // Em todos os outros casos, o inimigo não tem um alvo (foco na patrulha)
        target = null;
    }

    // MODIFICADO: Lógica de Patrulha agora é o comportamento principal
    private void Patrol()
    {
        // Se não houver pontos de patrulha ou se já chegou ao final, para.
        if (patrolPoints == null || patrolPoints.Count == 0 || currentPointIndex >= patrolPoints.Count)
        {
            // Opcional: Adicionar lógica para quando o inimigo chega à base.
            // Ex: causar dano à base, se autodestruir, etc.
            return;
        }

        // Define o próximo ponto do caminho como destino
        Transform currentDestination = patrolPoints[currentPointIndex];
        MoveTowardsPosition(currentDestination.position);

        // Verifica se chegou perto o suficiente do ponto de destino
        float distanceToPoint = Vector3.Distance(transform.position, currentDestination.position);
        if (distanceToPoint < 0.5f) // Aumentei a tolerância para evitar que ele fique preso
        {
            // Avança para o próximo ponto do caminho
            currentPointIndex++;
        }
    }

    private void ChaseTarget()
    {
        if (target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // Se estiver dentro da distância de ataque, para de se mover e ataca
        if (distanceToTarget <= attackDistance)
        {
            // Apenas olha para o alvo
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime));
            }

            // Chama o sistema de combate para atacar
            if (combatSystem != null)
            {
                combatSystem.TryAttack();
            }
        }
        else // Se estiver fora da distância de ataque, move-se em direção ao alvo
        {
            MoveTowardsPosition(target.position);
        }
    }

    // As funções abaixo não precisaram de grandes mudanças

    public void InitializeEnemy()
    {
        if (enemyData == null)
        {
            Debug.LogError("EnemyData não atribuído em " + gameObject.name);
            return;
        }
        currentDamage = enemyData.GetDamage(nivel);
        currentMoveSpeed = enemyData.GetMoveSpeed(nivel);
        healthSystem.InitializeHealth(nivel);
        currentPointIndex = 0; // Reinicia o caminho
        target = null;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void MoveTowardsPosition(Vector3 targetPosition)
    {
        if (rb == null) return;
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        rb.MovePosition(transform.position + direction * currentMoveSpeed * Time.fixedDeltaTime);
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.fixedDeltaTime));
        }
    }

    public void HandleDeath()
    {
        DropRewards();
        EnemyPoolManager.Instance.ReturnToPool(gameObject);
    }

    public void TakeDamage(float damageAmount, Transform attacker = null)
    {
        healthSystem.TakeDamage(damageAmount);
        if (attacker != null && target == null) // Inicia perseguição se for atacado
        {
            target = attacker;
        }
    }

    private void DropRewards()
    {
        for (int i = 0; i < enemyData.geoditasOnDeath; i++)
        {
            Debug.Log("Dropping geodita");
        }
        if (Random.value <= enemyData.etherDropChance)
        {
            Debug.Log("Dropping éter negro");
        }
    }

    public void SetPatrolPoints(List<Transform> points)
    {
        patrolPoints = points;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, selfDefenseRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}