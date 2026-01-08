using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyCombatSystem : MonoBehaviour
{
    [Header("Configurações de Combate (Player)")]
    [Tooltip("Raio para detecção do jogador e aplicação de dano.")]
    public float attackRange = 2f;
    [Tooltip("Tempo (em segundos) que o jogador precisa ficar na área para receber dano (Dano Inicial).")]
    public float timeToDamage = 2f;

    [Header("Aura de Dano em Torres")]
    [Tooltip("Raio da aura que causa dano em torres.")]
    public float towerAuraRadius = 10f;
    [Tooltip("Dano base que a aura causa em cada ciclo.")]
    public float towerAuraDamage = 14f;
    [Tooltip("Intervalo (em segundos) entre cada aplicação de dano da aura.")]
    public float towerAuraInterval = 3f;

    [Header("Referências")]
    public Transform attackPoint;
    public LayerMask playerLayer;
    public LayerMask towerLayer;

    private EnemyController enemyController;
    private EnemyDataSO enemyData;
    private float currentDamage;

    private bool playerIsInside = false;
    private Coroutine attackCoroutine;

    private Coroutine towerAuraCoroutine;

    void Awake()
    {
        enemyController = GetComponent<EnemyController>();
    }

    public void InitializeCombat(EnemyDataSO data, int nivel)
    {
        this.enemyData = data;

        if (enemyData != null)
        {
            currentDamage = enemyData.GetDamage(nivel);

            SphereCollider sphereCollider = (attackPoint != null && attackPoint.GetComponent<SphereCollider>() != null)
                                            ? attackPoint.GetComponent<SphereCollider>()
                                            : GetComponent<SphereCollider>();

            if (sphereCollider == null)
            {
                sphereCollider = gameObject.AddComponent<SphereCollider>();
            }

            if (sphereCollider != null)
            {
                sphereCollider.isTrigger = true;
                sphereCollider.radius = attackRange;
            }

            if (towerAuraCoroutine != null) StopCoroutine(towerAuraCoroutine);
            towerAuraCoroutine = StartCoroutine(TowerAuraCycle());
        }
        else
        {
            Debug.LogError("EnemyCombatSystem: InitializeCombat FALHOU. EnemyData está faltando.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            if (!playerIsInside)
            {
                playerIsInside = true;
                if (attackCoroutine != null) StopCoroutine(attackCoroutine);
                attackCoroutine = StartCoroutine(PlayerAttackCycle());
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            if (playerIsInside)
            {
                playerIsInside = false;
                if (attackCoroutine != null)
                {
                    StopCoroutine(attackCoroutine);
                    attackCoroutine = null;
                }
            }
        }
    }

    private IEnumerator PlayerAttackCycle()
    {
        yield return new WaitForSeconds(timeToDamage);

        while (playerIsInside && enemyController != null && !enemyController.IsDead)
        {
            ApplyDamageInArea();

            float cooldown = (enemyData != null && enemyData.attackSpeed > 0) ? (1f / enemyData.attackSpeed) : 1f;
            yield return new WaitForSeconds(cooldown);
        }
        attackCoroutine = null;
    }

    void ApplyDamageInArea()
    {
        if (enemyData == null) return;

        // --- VERIFICAÇÃO DE CEGUEIRA ---
        if (enemyController != null && enemyController.IsBlinded)
        {
            // 80% de chance de errar o ataque
            if (Random.value < 0.8f)
            {
                Debug.Log($"{gameObject.name} ERROU o ataque no player devido à CEGUEIRA!");
                return; // Sai da função sem dar dano
            }
        }

        Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, attackRange, playerLayer);

        if (hitPlayers.Length > 0)
        {
            PlayerHealthSystem playerHealth = hitPlayers[0].GetComponent<PlayerHealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(currentDamage);
            }
        }
        else
        {
            playerIsInside = false;
        }
    }

    private IEnumerator TowerAuraCycle()
    {
        yield return null;

        while (enemyController != null && !enemyController.IsDead)
        {
            ApplyAuraDamageToTowers();
            yield return new WaitForSeconds(towerAuraInterval);
        }
    }

    private void ApplyAuraDamageToTowers()
    {
        Collider[] hitTowers = Physics.OverlapSphere(transform.position, towerAuraRadius, towerLayer);

        foreach (Collider towerCollider in hitTowers)
        {
            TowerController tower = towerCollider.GetComponent<TowerController>();

            if (tower != null)
            {
                tower.TakeDamage(towerAuraDamage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, towerAuraRadius);
    }
}