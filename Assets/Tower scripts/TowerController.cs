using UnityEngine;
using System;

public class TowerController : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Arraste o ScriptableObject do personagem (ex: Samurai) para cá.")]
    public CharacterBase towerData;
    [Tooltip("A parte da torre que deve girar para apontar para o inimigo.")]
    public Transform partToRotate;

    [Header("Configurações de IA")]
    [Tooltip("A tag dos objetos que a torre deve considerar como inimigos.")]
    [SerializeField] private string enemyTag = "Enemy";

    // --- Evento para Upgrades ---
    public event Action<Transform> OnAttack;

    // --- Status Atuais da Torre (modificados por upgrades) ---
    private float currentDamage;
    private float currentAttackSpeed; // Ataques por segundo
    private float currentRange;
    private int[] currentPathLevels;

    // --- Controle de IA e Ataque ---
    private Transform targetEnemy;
    private float attackCountdown = 0f;

    void Start()
    {
        // 1. Pega os status iniciais do Scriptable Object
        if (towerData == null)
        {
            Debug.LogError("TowerData não foi atribuído neste TowerController!", this.gameObject);
            this.enabled = false; // Desativa o script para evitar erros
            return;
        }

        currentDamage = towerData.damage;
        currentAttackSpeed = towerData.attackSpeed;
        currentRange = towerData.meleeRange; // Usa o "meleeRange" como alcance da torre
        currentPathLevels = new int[towerData.upgradePaths.Count];

        // 2. Começa a procurar por alvos de forma otimizada
        InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }

    void Update()
    {
        // Se não tem um alvo, não faz mais nada.
        if (targetEnemy == null) return;

        // Lógica de rotação (do TurretController)
        RotateTowardsTarget();

        // Lógica de ataque com cooldown (melhor que a do TurretController)
        attackCountdown -= Time.deltaTime;
        if (attackCountdown <= 0f)
        {
            attackCountdown = 1f / currentAttackSpeed; // Reseta o tempo para o próximo ataque
            Shoot();
        }
    }

    // Lógica de busca de alvo (do TurretController)
    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        Transform nearestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy.transform;
            }
        }

        // Verifica se o inimigo mais próximo está dentro do alcance
        if (nearestEnemy != null && shortestDistance <= currentRange)
        {
            targetEnemy = nearestEnemy;
        }
        else
        {
            targetEnemy = null;
        }
    }

    // Lógica de rotação (do TurretController)
    void RotateTowardsTarget()
    {
        if (partToRotate == null) return;

        Vector3 direction = targetEnemy.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        // Suaviza a rotação e a aplica apenas no eixo Y para a torre não inclinar
        Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * 10f).eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);
    }

    // Lógica de tiro (do TurretController) combinada com o evento (do TowerController)
    void Shoot()
    {
        // Tenta encontrar o componente de vida no inimigo
        EnemyHealthSystem healthSystem = targetEnemy.GetComponent<EnemyHealthSystem>();

        if (healthSystem != null)
        {
            // Causa dano usando o 'currentDamage' que pode ser modificado por upgrades
            healthSystem.TakeDamage(currentDamage);
        }

        // Dispara o evento para que os upgrades (como Corte Duplo) possam reagir
        OnAttack?.Invoke(targetEnemy);
    }

    // Desenha o alcance da torre no Editor (do TurretController)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, currentRange);
    }

    // --- MÉTODOS DE UPGRADE (do TowerController original) ---
    // (O restante do código para comprar e aplicar upgrades continua o mesmo)

    public void PurchaseUpgrade(int pathIndex)
    {
        if (!CanPurchaseUpgrade(pathIndex)) return;

        int nextTier = currentPathLevels[pathIndex];
        Upgrade upgradeToPurchase = towerData.upgradePaths[pathIndex].upgradesInPath[nextTier];

        if (!CurrencyManager.Instance.HasEnoughCurrency(upgradeToPurchase.geoditeCost, CurrencyType.Geodites) ||
            !CurrencyManager.Instance.HasEnoughCurrency(upgradeToPurchase.darkEtherCost, CurrencyType.DarkEther))
        {
            Debug.Log("Moeda insuficiente!");
            return;
        }

        CurrencyManager.Instance.SpendCurrency(upgradeToPurchase.geoditeCost, CurrencyType.Geodites);
        CurrencyManager.Instance.SpendCurrency(upgradeToPurchase.darkEtherCost, CurrencyType.DarkEther);

        ApplyUpgrade(upgradeToPurchase);
        currentPathLevels[pathIndex]++;
    }

    private void ApplyUpgrade(Upgrade upgrade)
    {
        foreach (var modifier in upgrade.modifiers)
        {
            // Lembre-se de adicionar os outros casos para Range, AttackSpeed, etc.
            if (modifier.statToModify == StatType.Damage)
            {
                currentDamage = CalculateNewValue(currentDamage, modifier);
            }
            else if (modifier.statToModify == StatType.Range)
            {
                currentRange = CalculateNewValue(currentRange, modifier);
            }
            else if (modifier.statToModify == StatType.AttackSpeed)
            {
                currentAttackSpeed = CalculateNewValue(currentAttackSpeed, modifier);
            }
        }

        if (upgrade.behaviorToUnlock != null)
        {
            var newBehavior = gameObject.AddComponent(upgrade.behaviorToUnlock.GetType()) as TowerBehavior;
            if (newBehavior != null)
            {
                newBehavior.Initialize(this);
                Debug.Log("Equipamento adicionado: " + newBehavior.GetType().Name);
            }
        }
    }

    private bool CanPurchaseUpgrade(int pathIndex)
    {
        if (towerData.upgradePaths == null || pathIndex >= towerData.upgradePaths.Count ||
            currentPathLevels[pathIndex] >= towerData.upgradePaths[pathIndex].upgradesInPath.Count)
        {
            return false;
        }
        return true;
    }

    private float CalculateNewValue(float baseValue, StatModifier modifier)
    {
        if (modifier.modType == ModificationType.Additive)
            return baseValue + modifier.value;
        else // Multiplicative
            return baseValue * (1 + modifier.value);
    }
}