using UnityEngine;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    [Header("Dados do Inimigo")]
    public EnemyDataSO enemyData;

    [Header("Status Atual")]
    public int nivel = 1;

    [Header("Pontos de Patrulha")]
    public List<Transform> patrolPoints;

    // Componentes
    private EnemyHealthSystem healthSystem;
    private EnemyCombatSystem combatSystem;

    // Status calculados
    private float currentDamage;
    private float currentMoveSpeed;

    // Variáveis de comportamento
    private int currentPointIndex = 0;
    private Transform target;
    private Vector3 lastPatrolPosition;
    private bool returningToPatrol = false;

    [Header("Configurações")]
    public float chaseDistance = 50f;
    public float attackDistance = 2f;
    public float stoppingDistance = 1.5f;

    // Referência para o jogador
    private Transform playerTransform;

    // Propriedades
    public bool IsDead { get { return healthSystem.isDead; } }
    public Transform Target { get { return target; } }

    void Awake()
    {
        healthSystem = GetComponent<EnemyHealthSystem>();
        combatSystem = GetComponent<EnemyCombatSystem>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void OnEnable()
    {
        InitializeEnemy();
    }

    void Update()
    {
        if (IsDead) return;

        // Verifica continuamente se o jogador está dentro da distância de perseguição
        CheckForPlayer();

        if (target != null)
        {
            ChaseTarget();
        }
        else
        {
            Patrol();
        }
    }

    public void InitializeEnemy()
    {
        if (enemyData == null)
        {
            Debug.LogError("EnemyData não atribuído em " + gameObject.name);
            return;
        }

        // Calcula status baseado no nível
        currentDamage = enemyData.GetDamage(nivel);
        currentMoveSpeed = enemyData.GetMoveSpeed(nivel);

        // Inicializa o sistema de saúde
        healthSystem.InitializeHealth(nivel);

        // Reseta estado
        currentPointIndex = 0;
        returningToPatrol = false;
        target = null;
    }

    public void TakeDamage(float damageAmount, Transform attacker = null)
    {
        healthSystem.TakeDamage(damageAmount);

        // Define o atacante como alvo imediatamente ao receber dano
        if (attacker != null)
        {
            lastPatrolPosition = transform.position;
            target = attacker;
            returningToPatrol = false;
            Debug.Log("Inimigo começou a perseguir o jogador após levar dano");
        }
    }

    public void HandleDeath()
    {
        DropRewards();
        EnemyPoolManager.Instance.ReturnToPool(gameObject);
    }

    private void DropRewards()
    {
        // Lógica para dropar geoditas
        for (int i = 0; i < enemyData.geoditasOnDeath; i++)
        {
            // Implemente sua lógica de drop aqui
            Debug.Log("Dropping geodita");
        }

        // Lógica para dropar éter negro
        if (Random.value <= enemyData.etherDropChance)
        {
            // Implemente drop de éter negro aqui
            Debug.Log("Dropping éter negro");
        }
    }

    private void CheckForPlayer()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Se o jogador estiver dentro da distância de perseguição, começa a perseguir
        if (distanceToPlayer <= chaseDistance && target == null)
        {
            lastPatrolPosition = transform.position;
            target = playerTransform;
            returningToPatrol = false;
            Debug.Log("Jogador detectado, iniciando perseguição");
        }
        // Se o jogador estiver muito longe, para de perseguir
        else if (distanceToPlayer > chaseDistance && target == playerTransform)
        {
            ForgetTarget();
        }
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Count == 0) return;

        if (returningToPatrol)
        {
            float distanceToLastPoint = Vector3.Distance(transform.position, lastPatrolPosition);
            if (distanceToLastPoint > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, lastPatrolPosition, currentMoveSpeed * Time.deltaTime);
            }
            else
            {
                returningToPatrol = false;
            }
            return;
        }

        Transform currentPoint = patrolPoints[currentPointIndex];
        transform.position = Vector3.MoveTowards(transform.position, currentPoint.position, currentMoveSpeed * Time.deltaTime);

        float distanceToPoint = Vector3.Distance(transform.position, currentPoint.position);
        if (distanceToPoint < 0.1f)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
        }
    }

    private void ChaseTarget()
    {
        if (target == null)
        {
            ForgetTarget();
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // Se o alvo estiver muito longe, para de perseguir
        if (distanceToTarget > chaseDistance)
        {
            ForgetTarget();
            return;
        }

        // Se estiver dentro da distância de ataque, para de se mover e ataca
        if (distanceToTarget <= attackDistance)
        {
            // Apenas olha para o alvo, o sistema de combate cuidará do ataque
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 5f * Time.deltaTime);
            }

            // Chama o sistema de combate para atacar
            if (combatSystem != null)
            {
                combatSystem.TryAttack();
            }
            return;
        }

        // Move-se em direção ao alvo
        transform.position = Vector3.MoveTowards(transform.position, target.position, currentMoveSpeed * Time.deltaTime);

        // Rotaciona para olhar para o alvo
        Vector3 moveDirection = (target.position - transform.position).normalized;
        moveDirection.y = 0;
        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), 5f * Time.deltaTime);
        }
    }

    private void ForgetTarget()
    {
        target = null;
        returningToPatrol = true;
        Debug.Log("Alvo perdido, retornando à patrulha");
    }

    // Para ser chamado pelo HordeManager
    public void SetPatrolPoints(List<Transform> points)
    {
        patrolPoints = points;
    }

    // Visualização no editor para debug
    void OnDrawGizmosSelected()
    {
        // Desenha a esfera de detecção
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);

        // Desenha a esfera de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        // Desenha a direção do movimento se estiver perseguindo
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}