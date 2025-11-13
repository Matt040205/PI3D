using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class TowerController : MonoBehaviour
{
    [Header("Referências")]
    public CharacterBase towerData;
    public Transform partToRotate;
    public Transform firePoint;

    [Header("Configurações de IA")]
    [SerializeField] private string enemyTag = "Enemy";

    [Tooltip("Define se a torre pode mirar em inimigos do tipo Voador.")]
    public bool TargetsFlyingEnemies { get; set; } = false;

    public event Action<EnemyHealthSystem> OnTargetDamaged;
    public event Func<EnemyHealthSystem, float, float> OnCalculateDamage;
    public event Action<EnemyHealthSystem> OnCriticalHit;
    public event Action<EnemyHealthSystem> OnEnemyKilled;

    public bool IsDestroyed { get; private set; }

    [HideInInspector]
    public int totalCostSpent { get; private set; }

    private float currentHealth;
    private float currentArmor;
    private float currentDamage;
    private float currentAttackSpeed;
    private float currentCritChance;
    private float currentCritDamage;
    private float currentArmorPenetration;

    public float MaxHealth { get; private set; }
    public float CurrentRange { get; private set; }

    private List<TowerBehavior> activeBehaviors = new List<TowerBehavior>();
    public int[] currentPathLevels;
    private Transform targetEnemy;
    private float fireCountdown = 0f;

    void Start()
    {
        if (towerData == null)
        {
            Debug.LogError("FALHA: TowerData está NULO na torre '" + gameObject.name + "'!", this.gameObject);
            this.enabled = false;
            return;
        }
        currentPathLevels = new int[towerData.upgradePaths.Count];
        CloneBaseStats();
        InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }

    void CloneBaseStats()
    {
        MaxHealth = towerData.maxHealth;
        currentHealth = MaxHealth;

        totalCostSpent = towerData.cost;

        currentArmor = towerData.armor;
        currentDamage = towerData.damage;
        currentAttackSpeed = towerData.attackSpeed;
        currentCritChance = towerData.critChance;
        currentCritDamage = towerData.critDamage;
        currentArmorPenetration = towerData.armorPenetration;

        CurrentRange = towerData.meleeRange;

        IsDestroyed = false;

        Debug.Log($"<color=green>TORRE INICIALIZADA:</color> {gameObject.name} (Vida Máxima: {MaxHealth}, Dano: {currentDamage})");
    }

    public void ApplyUpgrade(Upgrade upgradeToApply, int geoditeCost, int darkEtherCost)
    {
        if (upgradeToApply == null)
        {
            Debug.LogError("Tentativa de aplicar um upgrade NULO!");
            return;
        }

        totalCostSpent += geoditeCost;
        totalCostSpent += darkEtherCost;

        Debug.Log($"<color=cyan>Aplicando upgrade '{upgradeToApply.upgradeName}' na torre {gameObject.name}</color>");

        foreach (var modifier in upgradeToApply.modifiers)
        {
            ApplyModifier(modifier);
        }

        if (upgradeToApply.behaviorToUnlock != null)
        {
            GameObject behaviorObject = Instantiate(upgradeToApply.behaviorToUnlock.gameObject, transform);
            TowerBehavior newBehavior = behaviorObject.GetComponent<TowerBehavior>();
            if (newBehavior != null)
            {
                newBehavior.Initialize(this);
                activeBehaviors.Add(newBehavior);
                Debug.Log($"<color=yellow>HABILIDADE DESBLOQUEADA: '{newBehavior.GetType().Name}' foi adicionada à torre {gameObject.name}.</color>");
            }
        }
    }

    private void ApplyModifier(StatModifier modifier)
    {
        float value = modifier.value;
        switch (modifier.statToModify)
        {
            case StatType.Damage:
                currentDamage = (modifier.modType == ModificationType.Additive) ? currentDamage + value : currentDamage * (1 + value);
                break;
            case StatType.AttackSpeed:
                currentAttackSpeed = (modifier.modType == ModificationType.Additive) ? currentAttackSpeed + value : currentAttackSpeed * (1 + value);
                break;
            case StatType.Range:
                CurrentRange = (modifier.modType == ModificationType.Additive) ? CurrentRange + value : CurrentRange * (1 + value);
                break;
            case StatType.Armor:
                currentArmor = Mathf.Clamp01(currentArmor + value);
                break;
            case StatType.CritChance:
                currentCritChance = Mathf.Clamp01(currentCritChance + value);
                break;
            case StatType.CritDamage:
                currentCritDamage = (modifier.modType == ModificationType.Additive) ? currentCritDamage + value : currentCritDamage * (1 + value);
                break;
            case StatType.ArmorPenetration:
                currentArmorPenetration = Mathf.Clamp01(currentArmorPenetration + value);
                break;
        }
    }

    void Update()
    {
        if (IsDestroyed || targetEnemy == null) return;
        if (partToRotate != null) RotateTowardsTarget();

        fireCountdown -= Time.deltaTime;
        if (fireCountdown <= 0f)
        {
            fireCountdown = 1f / currentAttackSpeed;
            Shoot();
        }
    }

    void Shoot()
    {
        if (targetEnemy == null)
            return;

        EnemyHealthSystem healthSystem = targetEnemy.GetComponent<EnemyHealthSystem>();

        if (healthSystem != null)
        {
            float damageToDeal = currentDamage;
            bool isCritical = UnityEngine.Random.value <= currentCritChance;

            if (isCritical)
            {
                float criticalDamage = damageToDeal * currentCritDamage;
                damageToDeal = criticalDamage;
            }

            if (OnCalculateDamage != null)
            {
                foreach (Func<EnemyHealthSystem, float, float> modifier in OnCalculateDamage.GetInvocationList())
                {
                    damageToDeal = modifier(healthSystem, damageToDeal);
                }
            }

            bool enemyDied = healthSystem.TakeDamage(damageToDeal, currentArmorPenetration);

            if (enemyDied)
            {
                OnEnemyKilled?.Invoke(healthSystem);
            }

            OnTargetDamaged?.Invoke(healthSystem);

            if (isCritical)
            {
                OnCriticalHit?.Invoke(healthSystem);
            }
        }
    }

    public void SellTower(float refundPercentage)
    {
        if (CurrencyManager.Instance != null)
        {
            int refundAmount = Mathf.FloorToInt(totalCostSpent * refundPercentage);
            CurrencyManager.Instance.AddCurrency(refundAmount, CurrencyType.Geodites);
        }
        DestroyTower();
    }

    public void TakeDamage(float amount)
    {
        if (IsDestroyed) return;
        float remainingDamage = amount;

        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f);
        foreach (var col in colliders)
        {
            SpiritualBarrierBehavior barrier = col.GetComponent<SpiritualBarrierBehavior>();
            if (barrier != null && barrier.towerController != this)
            {
                remainingDamage = barrier.AbsorbDamage(remainingDamage);
            }
        }

        float finalDamage = remainingDamage * (1 - currentArmor);
        currentHealth -= finalDamage;

        Debug.Log($"<color=red>DANO RECEBIDO:</color> {gameObject.name} sofreu {finalDamage:F1} de dano. Vida Restante: {currentHealth:F1} / {MaxHealth:F1}.");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            DestroyTower();
        }
    }

    private void DestroyTower()
    {
        IsDestroyed = true;
        targetEnemy = null;

        if (TowerSelectionManager.Instance != null)
        {
            TowerSelectionManager.Instance.DeselectAll();
        }

        Debug.Log($"<color=red>TORRE DESTRUÍDA!</color> Torre {gameObject.name} foi destruída!");
        Destroy(gameObject);
    }

    public void Revive(float healthPercentage)
    {
        if (!IsDestroyed) return;
        IsDestroyed = false;
        currentHealth = MaxHealth * healthPercentage;
        Debug.Log($"Torre {gameObject.name} foi revivida!");
    }

    public void Heal(float amount) { currentHealth = Mathf.Min(currentHealth + amount, MaxHealth); }
    public void AddArmorBonus(float amount) { currentArmor += amount; }
    public void AddAttackSpeedBonus(float amount) { currentAttackSpeed *= (1 + amount); }
    public void AddDamageBonus(float amount) { currentDamage *= (1 + amount); }
    public void PerformExtraAttack() { Shoot(); }

    void UpdateTarget()
    {
        Collider[] collidersInRadius = Physics.OverlapSphere(transform.position, CurrentRange);
        Transform nearestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Collider collider in collidersInRadius)
        {
            if (collider.CompareTag(enemyTag))
            {
                EnemyController enemyController = collider.GetComponent<EnemyController>();

                if (enemyController == null || enemyController.enemyData == null) continue;

                EnemyType enemyType = enemyController.enemyData.enemyType;

                bool isTargetable = (enemyType == EnemyType.Terrestre) ||
                          (TargetsFlyingEnemies && enemyType == EnemyType.Voador);

                if (isTargetable)
                {
                    float distanceToEnemy = Vector3.Distance(transform.position, collider.transform.position);

                    if (distanceToEnemy < shortestDistance)
                    {
                        shortestDistance = distanceToEnemy;
                        nearestEnemy = collider.transform;
                    }
                }
            }
        }

        targetEnemy = nearestEnemy;
    }

    void RotateTowardsTarget()
    {
        Vector3 direction = targetEnemy.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * 10f).eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, CurrentRange);
    }
}