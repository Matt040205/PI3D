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
    private Transform target;

    [Header("Configurações")]
    public float chaseDistance = 50f;
    public float attackDistance = 2f;

    // Referência para o jogador (agora privada e preenchida externamente)
    private Transform playerTransform;

    // Propriedades
    public bool IsDead { get { return healthSystem.isDead; } }
    public Transform Target { get { return target; } }

    void Awake()
    {
        healthSystem = GetComponent<EnemyHealthSystem>();
        combatSystem = GetComponent<EnemyCombatSystem>();
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.useGravity = true;
        }
    }

    void OnEnable() { }

    public void InitializeEnemy(Transform player, List<Transform> path, EnemyDataSO data, int level)
    {
        this.playerTransform = player;
        this.patrolPoints = path;
        this.enemyData = data;
        this.nivel = level;

        if (enemyData == null)
        {
            Debug.LogError("EnemyData não atribuído em " + gameObject.name);
            gameObject.SetActive(false);
            return;
        }

        currentDamage = enemyData.GetDamage(nivel);
        currentMoveSpeed = enemyData.GetMoveSpeed(nivel);
        healthSystem.enemyData = this.enemyData;
        healthSystem.InitializeHealth(nivel);
        currentPointIndex = 0;
        target = null;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        if (IsDead) return;

        DecideTarget();

        if (target != null)
        {
            ChaseTarget();
        }
        else
        {
            Patrol();
        }
    }

    private void DecideTarget()
    {
        if (playerTransform == null)
        {
            target = null;
            return;
        }
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (mainPriority == AITargetPriority.Player && distanceToPlayer <= chaseDistance)
        {
            target = playerTransform;
            return;
        }
        if (mainPriority == AITargetPriority.Objective && distanceToPlayer <= selfDefenseRadius)
        {
            target = playerTransform;
            return;
        }
        target = null;
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Count == 0 || currentPointIndex >= patrolPoints.Count)
        {
            AttackObjectiveAndDie();
            return;
        }
        Transform currentDestination = patrolPoints[currentPointIndex];
        MoveTowardsPosition(currentDestination.position);
    }

    // LÓGICA CORRIGIDA: VERIFICA SE O TRIGGER É O PONTO DE PATRULHA CORRETO
    void OnTriggerEnter(Collider other)
    {
        if (patrolPoints != null && currentPointIndex < patrolPoints.Count)
        {
            // Verifica se o objeto que colidiu é o ponto de patrulha atual na sequência
            if (other.transform == patrolPoints[currentPointIndex])
            {
                Debug.Log("Inimigo chegou ao ponto de patrulha: " + other.gameObject.name + ". Avançando para o próximo.");
                currentPointIndex++;
            }
        }
    }

    private void AttackObjectiveAndDie()
    {
        ObjectiveHealthSystem objective = FindFirstObjectByType<ObjectiveHealthSystem>();
        if (objective != null)
        {
            float damageToObjective = enemyData.GetDamage(nivel);
            objective.TakeDamage(damageToObjective);
        }
        EnemyPoolManager.Instance.ReturnToPool(gameObject);
    }

    private void ChaseTarget()
    {
        if (target == null) return;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget <= attackDistance)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime));
            }
            if (combatSystem != null)
            {
                combatSystem.TryAttack();
            }
        }
        else
        {
            MoveTowardsPosition(target.position);
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
        if (attacker != null && target == null)
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