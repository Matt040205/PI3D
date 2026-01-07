using UnityEngine;
using System.Collections.Generic;
using System.Collections; // Necessário para Corrotinas
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
    private int paintStacks = 0; // Para o Nível 5 de Controle (Root)
    private float paintStackResetTime;

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

    // Constante para garantir que não erramos a letra
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

        // Reseta status de CC
        speedModifier = 1f;
        isRooted = false;
        isSlipping = false;
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

        // Resetar stacks de tinta se passar 5 segundos sem tomar tiro da torre de controle
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

        // Se estiver escorregando ou preso, não processa IA de movimento normal
        if (isSlipping || isRooted)
        {
            // Opcional: Manter rotação ou parar animação de andar
            if (anim != null) anim.SetBool("isWalking", false);
            return;
        }

        // --- DEBUG DE SEGURANÇA ---
        // Verifica se o alvo virou poça no meio da perseguição
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

    // --- MÉTODOS DE CONTROLE DE GRUPO (CHAMADOS PELA TORRE) ---

    // Torre DPS Nv 3
    public void ApplyKnockback(Vector3 direction, float force)
    {
        if (rb != null && !isRooted)
        {
            // Adiciona um impulso físico instantâneo
            rb.AddForce(direction.normalized * force, ForceMode.Impulse);
        }
    }

    // Torre Controle Nv 2 e 3
    public void ApplySlow(float percentage, float duration)
    {
        StopCoroutine("SlowRoutine"); // Reinicia se já estiver lento
        StartCoroutine(SlowRoutine(percentage, duration));
    }

    private IEnumerator SlowRoutine(float percentage, float duration)
    {
        // Aplica na Física
        speedModifier = Mathf.Clamp01(1f - percentage);

        // Aplica na Animação (Matrix Effect)
        if (anim != null) anim.speed = speedModifier;

        Debug.Log($"<color=cyan>SLOW APLICADO:</color> Velocidade reduzida para {speedModifier * 100}%");

        yield return new WaitForSeconds(duration);

        // Restaura Física
        speedModifier = 1f;

        // Restaura Animação
        if (anim != null) anim.speed = 1f;

        Debug.Log($"<color=cyan>SLOW ACABOU:</color> Velocidade normal.");
    }

    // Torre Controle Nv 4
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
        if (anim != null) anim.SetTrigger("Slip"); // Certifique-se de ter esse Trigger no Animator!

        // Zera a velocidade para simular a queda
        if (rb != null) rb.linearVelocity = Vector3.zero;

        yield return new WaitForSeconds(1.5f); // Tempo caído
        isSlipping = false;
    }

    // Torre Controle Nv 5
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
        // Aqui você pode instanciar um VFX de "Raízes de Tinta" se quiser

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

        // Se for poça, não calcula nada e sai
        if (playerTransform.CompareTag(TAG_POCA))
        {
            target = null;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        bool foundTarget = false;

        if (mainPriority == AITargetPriority.Player && distanceToPlayer <= chaseDistance)
        {
            if (target != playerTransform) Debug.Log($"[IA] {gameObject.name}: Viu o Player. Perseguindo!");
            target = playerTransform;
            foundTarget = true;
        }
        else if (mainPriority == AITargetPriority.Objective && distanceToPlayer <= selfDefenseRadius)
        {
            if (target != playerTransform) Debug.Log($"[IA] {gameObject.name}: Defendendo objetivo. Player perto!");
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
        Debug.Log($"[IA] {gameObject.name}: Recebeu comando LoseTarget.");
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

        // Segurança extra
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

        // CÁLCULO
        float finalSpeed = currentMoveSpeed * speedModifier;

        // --- DEBUG X9 ---
        // Abra o console (Ctrl+Shift+C) e veja se esse número muda quando toma tiro
        // Se aparecer "Mod: 1", o slow não está sendo aplicado.
        // Se aparecer "Mod: 0.1" e ele correr, é o Animator (Root Motion).
        // Debug.Log($"[Mover] Speed Base: {currentMoveSpeed} | Mod: {speedModifier} | Final: {finalSpeed}"); 
        // ----------------

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

        // --- LÓGICA DE REVIDE (IGNORA SE FOR POCA) ---
        if (attacker != null && target == null)
        {
            // Se o atacante for uma Poça, NÃO devemos revidar
            if (attacker.CompareTag(TAG_POCA))
            {
                Debug.Log($"[IA] {gameObject.name}: Tomei dano da Poca, mas IGNOREI o revide.");
            }
            else
            {
                Debug.Log($"[IA] {gameObject.name}: Atacado por {attacker.name}. Revidando!");
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