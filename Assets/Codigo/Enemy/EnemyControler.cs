using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using FMODUnity;
using FMOD.Studio;

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

    [Header("Configurações")]
    public float chaseDistance = 50f;
    public float attackDistance = 2f;
    public float respawnYThreshold = -10f;

    [Header("Controle de Grupo (CC)")]
    private float speedModifier = 1f;
    private bool isRooted = false;
    private bool isSlipping = false;
    private int paintStacks = 0;
    private float paintStackResetTime;

    // Variável para controle da cegueira
    private bool isBlinded = false;
    public bool IsBlinded => isBlinded;

    [Header("FMOD")]
    [EventRef]
    public string eventoMonstro = "event:/SFX/Monstro";
    private EventInstance monstroSoundInstance;

    private EnemyHealthSystem healthSystem;
    private EnemyCombatSystem combatSystem;
    private Rigidbody rb;
    private Animator anim;

    private float currentMoveSpeed;
    private float originalMoveSpeed;
    private bool isSlowed = false;

    private int currentPointIndex = 0;
    private Transform target;
    private Transform playerTransform;
    private Transform lastWaypointReached;

    private const string TAG_POCA = "Poca";

    public bool IsDead { get { return healthSystem.isDead; } }
    public Transform Target { get { return target; } }

    void Awake()
    {
        healthSystem = GetComponent<EnemyHealthSystem>();
        combatSystem = GetComponent<EnemyCombatSystem>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.useGravity = true;
        }

        if (!string.IsNullOrEmpty(eventoMonstro))
        {
            monstroSoundInstance = RuntimeManager.CreateInstance(eventoMonstro);
            RuntimeManager.AttachInstanceToGameObject(monstroSoundInstance, transform);
        }
    }

    void OnEnable()
    {
        if (monstroSoundInstance.isValid()) monstroSoundInstance.start();
    }

    void OnDisable()
    {
        if (monstroSoundInstance.isValid()) monstroSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    private void OnDestroy()
    {
        if (monstroSoundInstance.isValid()) monstroSoundInstance.release();
    }

    public void InitializeEnemy(Transform player, List<Transform> path, EnemyDataSO data, int level)
    {
        this.playerTransform = player;
        this.patrolPoints = path;
        this.enemyData = data;
        this.nivel = level;

        if (enemyData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        originalMoveSpeed = enemyData.GetMoveSpeed(nivel);
        currentMoveSpeed = originalMoveSpeed;
        isSlowed = false;

        speedModifier = 1f;
        isRooted = false;
        isSlipping = false;
        isBlinded = false; // Reset cegueira
        paintStacks = 0;

        healthSystem.enemyData = this.enemyData;
        healthSystem.InitializeHealth(nivel);
        currentPointIndex = 0;
        target = null;

        if (patrolPoints != null && patrolPoints.Count > 0)
            lastWaypointReached = patrolPoints[0];
        else
            lastWaypointReached = null;

        if (anim == null) anim = GetComponent<Animator>();
        if (anim != null) anim.SetBool("isWalking", false);

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

    void Update()
    {
        if (IsDead) return;

        if (transform.position.y < respawnYThreshold)
        {
            RespawnAtLastWaypoint();
        }

        if (paintStacks > 0 && Time.time > paintStackResetTime)
        {
            paintStacks = 0;
        }
    }

    void FixedUpdate()
    {
        if (IsDead)
        {
            if (anim != null) anim.SetBool("isWalking", false);
            return;
        }

        if (isSlipping || isRooted)
        {
            if (anim != null) anim.SetBool("isWalking", false);
            return;
        }

        if (target != null && target.CompareTag(TAG_POCA))
        {
            Debug.Log($"[IA] {gameObject.name}: Player virou Poca. Perdi o alvo!");
            target = null;
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

    // --- RECEBE A MENSAGEM DA FUMAÇA ---
    public void SetBlinded(bool state)
    {
        isBlinded = state;
    }

    // --- MÉTODOS DE CONTROLE DE GRUPO ---

    public void ApplyKnockback(Vector3 direction, float force)
    {
        if (rb != null && !isRooted)
        {
            rb.AddForce(direction.normalized * force, ForceMode.Impulse);
        }
    }

    public void ApplySlow(float percentage, float duration)
    {
        StopCoroutine("SlowRoutine");
        StartCoroutine(SlowRoutine(percentage, duration));
    }

    private IEnumerator SlowRoutine(float percentage, float duration)
    {
        speedModifier = Mathf.Clamp01(1f - percentage);

        if (anim != null) anim.speed = speedModifier;

        yield return new WaitForSeconds(duration);

        speedModifier = 1f;

        if (anim != null) anim.speed = 1f;
    }

    public void ApplySlip()
    {
        if (!isSlipping && !isRooted)
        {
            StartCoroutine(SlipRoutine());
        }
    }

    private IEnumerator SlipRoutine()
    {
        isSlipping = true;
        if (anim != null) anim.SetTrigger("Slip");

        if (rb != null) rb.linearVelocity = Vector3.zero;

        yield return new WaitForSeconds(1.5f);
        isSlipping = false;
    }

    public void AddPaintStack()
    {
        paintStacks++;
        paintStackResetTime = Time.time + 5f;

        if (paintStacks >= 5)
        {
            StartCoroutine(RootRoutine(2f));
            paintStacks = 0;
        }
    }

    private IEnumerator RootRoutine(float duration)
    {
        isRooted = true;
        if (rb != null) rb.linearVelocity = Vector3.zero;

        yield return new WaitForSeconds(duration);
        isRooted = false;
    }

    // -----------------------------------------------------------

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

        if (playerTransform.CompareTag(TAG_POCA))
        {
            target = null;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        bool foundTarget = false;

        if (mainPriority == AITargetPriority.Player && distanceToPlayer <= chaseDistance)
        {
            target = playerTransform;
            foundTarget = true;
        }
        else if (mainPriority == AITargetPriority.Objective && distanceToPlayer <= selfDefenseRadius)
        {
            target = playerTransform;
            foundTarget = true;
        }

        if (!foundTarget)
        {
            target = null;
        }
    }

    public void LoseTarget()
    {
        target = null;
    }

    public void SetTargetNull()
    {
        target = null;
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Count == 0 || currentPointIndex >= patrolPoints.Count)
        {
            if (anim != null) anim.SetBool("isWalking", false);
            AttackObjectiveAndDie();
            return;
        }

        if (anim != null) anim.SetBool("isWalking", true);

        Transform currentDestination = patrolPoints[currentPointIndex];
        MoveTowardsPosition(currentDestination.position);
    }

    void OnTriggerEnter(Collider other)
    {
        if (patrolPoints != null && currentPointIndex < patrolPoints.Count)
        {
            if (other.transform == patrolPoints[currentPointIndex])
            {
                lastWaypointReached = patrolPoints[currentPointIndex];
                currentPointIndex++;
            }
        }
    }

    private void RespawnAtLastWaypoint()
    {
        if (lastWaypointReached == null)
        {
            if (patrolPoints != null && patrolPoints.Count > 0)
                lastWaypointReached = patrolPoints[0];
            else
            {
                EnemyPoolManager.Instance.ReturnToPool(gameObject);
                return;
            }
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = lastWaypointReached.position;
        target = null;
        if (anim != null) anim.SetBool("isWalking", true);
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

        if (target.CompareTag(TAG_POCA))
        {
            target = null;
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= attackDistance)
        {
            if (rb != null) rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

            if (anim != null)
            {
                anim.SetBool("isWalking", false);
                anim.SetTrigger("doAttack");
            }

            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime));
            }
        }
        else
        {
            if (anim != null) anim.SetBool("isWalking", true);
            MoveTowardsPosition(target.position);
        }
    }

    private void MoveTowardsPosition(Vector3 targetPosition)
    {
        if (rb == null) return;

        if (isRooted || isSlipping) return;

        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;

        float finalSpeed = currentMoveSpeed * speedModifier;

        Vector3 horizontalVelocity = direction * finalSpeed;
        float verticalVelocity = rb.linearVelocity.y;

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
        EnemyPoolManager.Instance.ReturnToPool(gameObject);
    }

    public void TakeDamage(float damageAmount, Transform attacker = null)
    {
        healthSystem.TakeDamage(damageAmount);

        if (attacker != null && target == null)
        {
            if (attacker.CompareTag(TAG_POCA))
            {
                Debug.Log($"[IA] {gameObject.name}: Tomei dano da Poca, mas IGNOREI o revide.");
            }
            else
            {
                target = attacker;
            }
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
        Gizmos.color = Color.red;
        if (target != null) Gizmos.DrawLine(transform.position, target.position);
    }
}