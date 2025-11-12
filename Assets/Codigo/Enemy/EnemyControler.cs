using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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

    private EnemyHealthSystem healthSystem;
    private EnemyCombatSystem combatSystem;
    private Rigidbody rb;

    // Referência para o Animator
    private Animator anim;

    private float currentDamage;
    private float currentMoveSpeed;
    private float originalMoveSpeed;
    private bool isSlowed = false;

    private int currentPointIndex = 0;
    private Transform target;

    [Header("Configurações")]
    public float chaseDistance = 50f;
    public float attackDistance = 2f;

    private Transform playerTransform;

    public bool IsDead { get { return healthSystem.isDead; } }
    public Transform Target { get { return target; } }

    void Awake()
    {
        healthSystem = GetComponent<EnemyHealthSystem>();
        combatSystem = GetComponent<EnemyCombatSystem>();
        rb = GetComponent<Rigidbody>();

        // Pega o componente Animator
        anim = GetComponent<Animator>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.useGravity = true;
        }
    }

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
        originalMoveSpeed = enemyData.GetMoveSpeed(nivel);
        currentMoveSpeed = originalMoveSpeed;
        isSlowed = false;

        healthSystem.enemyData = this.enemyData;
        healthSystem.InitializeHealth(nivel);
        currentPointIndex = 0;
        target = null;

        // Garante que o inimigo comece no estado "Idle" ao ser ativado
        if (anim == null) anim = GetComponent<Animator>(); // Garante que temos a referência
        if (anim != null) // Adiciona verificação para evitar erro
        {
            anim.SetBool("isWalking", false);
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (combatSystem != null)
        {
            combatSystem.InitializeCombat(enemyData, nivel);
        }
    }

    void FixedUpdate()
    {
        if (IsDead)
        {
            if (anim != null) anim.SetBool("isWalking", false); // Garante que pare de andar se morrer
            return;
        }

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

    public void AplicarDesaceleracao(float percentual)
    {
        if (!isSlowed)
        {
            currentMoveSpeed = originalMoveSpeed * (1f - percentual);
            isSlowed = true;
        }
    }

    public void RemoverDesaceleracao()
    {
        if (isSlowed)
        {
            currentMoveSpeed = originalMoveSpeed;
            isSlowed = false;
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
            if (anim != null) anim.SetBool("isWalking", false); // Parou de patrulhar, vai atacar objetivo
            AttackObjectiveAndDie();
            return;
        }

        if (anim != null) anim.SetBool("isWalking", true); // Está em patrulha, então está andando

        Transform currentDestination = patrolPoints[currentPointIndex];
        MoveTowardsPosition(currentDestination.position);
    }

    void OnTriggerEnter(Collider other)
    {
        if (patrolPoints != null && currentPointIndex < patrolPoints.Count)
        {
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

    // --- FUNÇÃO MODIFICADA ---
    private void ChaseTarget()
    {
        if (target == null) return;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= attackDistance) // ESTADO DE ATAQUE
        {
            if (rb != null)
            {
                // Paramos apenas o X e Z, mantendo o Y (gravidade).
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            }

            if (anim != null)
            {
                anim.SetBool("isWalking", false); // Parou de andar
                anim.SetTrigger("doAttack");      // Toca a animação de ataque
            }

            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime));
            }
        }
        else // ESTADO DE PERSEGUIÇÃO
        {
            if (anim != null) anim.SetBool("isWalking", true); // Está perseguindo, então está andando
            MoveTowardsPosition(target.position);
        }
    }

    // --- FUNÇÃO MODIFICADA ---
    private void MoveTowardsPosition(Vector3 targetPosition)
    {
        if (rb == null) return;
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;

        // Em vez de MovePosition, vamos definir a velocidade.

        // 1. Calculamos a velocidade horizontal desejada
        Vector3 horizontalVelocity = direction * currentMoveSpeed;

        // 2. Mantemos a velocidade vertical atual (gravidade)
        float verticalVelocity = rb.linearVelocity.y;

        // 3. Combinamos as duas e aplicamos
        rb.linearVelocity = new Vector3(horizontalVelocity.x, verticalVelocity, horizontalVelocity.z);

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.fixedDeltaTime));
        }
    }

    public void HandleDeath()
    {
        if (anim != null) anim.SetBool("isWalking", false);
        // NOTA: O ideal aqui seria disparar um trigger "doDie" e
        // esperar a animação de morte terminar antes de chamar ReturnToPool.

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