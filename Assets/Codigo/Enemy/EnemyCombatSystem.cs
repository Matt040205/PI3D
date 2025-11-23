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

    [Header("Aura de Dano em Torres")] // <<< NOVA SEÇÃO ADICIONADA AQUI
    [Tooltip("Raio da aura que causa dano em torres.")]
    public float towerAuraRadius = 10f;
    [Tooltip("Dano base que a aura causa em cada ciclo.")]
    public float towerAuraDamage = 14f;
    [Tooltip("Intervalo (em segundos) entre cada aplicação de dano da aura.")]
    public float towerAuraInterval = 3f;

    [Header("Referências")]
    public Transform attackPoint;
    public LayerMask playerLayer;
    public LayerMask towerLayer; // <<< NOVA VARIÁVEL DE LAYER

    // Componentes
    private EnemyController enemyController;
    private EnemyDataSO enemyData;
    private float currentDamage;

    // Estado de ataque baseado em área (Player)
    private bool playerIsInside = false;
    private Coroutine attackCoroutine;

    // Estado da Aura (Torres)
    private Coroutine towerAuraCoroutine; // <<< NOVA COROUTINE

    void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        // Debugs de inicialização removidos para manter o código limpo, mas a lógica de Awake está OK.
    }

    void Start()
    {
        // Debug de Start removido.
    }

    // --- MÉTODO DE INICIALIZAÇÃO CHAMADO PELO ENEMYCONTROLLER ---
    public void InitializeCombat(EnemyDataSO data, int nivel)
    {
        this.enemyData = data;

        if (enemyData != null)
        {
            currentDamage = enemyData.GetDamage(nivel);

            // Tenta obter/adicionar/configurar o colisor para a detecção do Player (Trigger)
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

            // NOVO: Inicia a Coroutine de Dano da Aura
            if (towerAuraCoroutine != null) StopCoroutine(towerAuraCoroutine);
            towerAuraCoroutine = StartCoroutine(TowerAuraCycle());
        }
        else
        {
            Debug.LogError("EnemyCombatSystem: InitializeCombat FALHOU. EnemyData está faltando.");
        }
    }

    // --- LÓGICA DO PLAYER (OnTriggerEnter/Exit e AttackCycle) ---

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

    // Coroutine renomeada de AttackCycle para PlayerAttackCycle
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

        Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, attackRange, playerLayer);

        if (hitPlayers.Length > 0)
        {
            PlayerHealthSystem playerHealth = hitPlayers[0].GetComponent<PlayerHealthSystem>();
            if (playerHealth != null)
            {
                // Removendo logs de debug que não são mais necessários para o ataque do player
                playerHealth.TakeDamage(currentDamage);
            }
        }
        else
        {
            playerIsInside = false;
        }
    }

    // --- NOVA LÓGICA: AURA DE DANO EM TORRES ---

    private IEnumerator TowerAuraCycle()
    {
        // Garante que o loop só começa depois da inicialização completa (que acontece no FixedUpdate)
        // e que não tente rodar no primeiro frame.
        yield return null;

        // Roda continuamente enquanto o inimigo estiver vivo
        while (enemyController != null && !enemyController.IsDead)
        {
            ApplyAuraDamageToTowers();

            // Espera pelo intervalo configurado (3s por padrão)
            yield return new WaitForSeconds(towerAuraInterval);
        }
    }

    private void ApplyAuraDamageToTowers()
    {
        // 1. Usa OverlapSphere para encontrar todos os objetos na camada "Towers"
        Collider[] hitTowers = Physics.OverlapSphere(transform.position, towerAuraRadius, towerLayer);

        // 2. Itera sobre todas as torres encontradas
        foreach (Collider towerCollider in hitTowers)
        {
            TowerController tower = towerCollider.GetComponent<TowerController>();

            // 3. Aplica dano na torre
            if (tower != null)
            {
                // Usa o método TakeDamage da Torre
                tower.TakeDamage(towerAuraDamage);
            }
        }
    }


    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            // Gizmo para o ataque de Player (vermelho)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        // NOVO GIZMO: Aura de Dano em Torres (roxo)
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, towerAuraRadius);
    }
}