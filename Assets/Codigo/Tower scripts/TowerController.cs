using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

// Assumindo que EnemyType e EnemyController são acessíveis neste script.
// Se houver um erro de compilação, pode ser necessário adicionar 'using static EnemyDataSO;'
// ou ajustar os namespaces.

public class TowerController : MonoBehaviour
{
    [Header("Referências")]
    public CharacterBase towerData;
    public Transform partToRotate;
    public Transform firePoint;

    [Header("Configurações de IA")]
    [SerializeField] private string enemyTag = "Enemy";

    // --- PROPRIEDADE PARA COMPORTAMENTO DE ALVO (FlyerEnemyTargetingBehavior) ---
    [Tooltip("Define se a torre pode mirar em inimigos do tipo Voador.")]
    public bool TargetsFlyingEnemies { get; set; } = false;

    // --- Eventos para os Behaviors ---
    public event Action<EnemyHealthSystem> OnTargetDamaged;
    public event Func<EnemyHealthSystem, float, float> OnCalculateDamage;
    public event Action<EnemyHealthSystem> OnCriticalHit;
    public event Action<EnemyHealthSystem> OnEnemyKilled;

    // --- Propriedade Pública de Estado ---
    public bool IsDestroyed { get; private set; }

    // --- Status Atuais da Instância da Torre ---
    private float currentHealth;
    // CORRIGIDO: maxHealth e currentRange substituídos por propriedades públicas para acesso externo.
    private float currentArmor;
    private float currentDamage;
    private float currentAttackSpeed;
    private float currentCritChance;
    private float currentCritDamage;
    private float currentArmorPenetration;

    // --- PROPRIEDADES DE STATUS ---
    public float MaxHealth { get; private set; }
    public float CurrentRange { get; private set; }


    // --- Variáveis de Controle Interno ---
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
        // Chamada para UpdateTarget repetida a cada 0.5s para otimização
        InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }

    void CloneBaseStats()
    {
        // 🌟 Herda a vida máxima do CharacterBase
        MaxHealth = towerData.maxHealth;
        currentHealth = MaxHealth;

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

    public void ApplyUpgrade(Upgrade upgradeToApply)
    {
        if (upgradeToApply == null)
        {
            Debug.LogError("Tentativa de aplicar um upgrade NULO!");
            return;
        }

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

            // ... (Lógica de Dano)

            if (isCritical)
            {
                float criticalDamage = damageToDeal * currentCritDamage;
                damageToDeal = criticalDamage;
            }

            // Permite que outros scripts (habilidades) modifiquem o dano antes de ser aplicado
            if (OnCalculateDamage != null)
            {
                foreach (Func<EnemyHealthSystem, float, float> modifier in OnCalculateDamage.GetInvocationList())
                {
                    damageToDeal = modifier(healthSystem, damageToDeal);
                }
            }

            bool enemyDied = healthSystem.TakeDamage(damageToDeal, currentArmorPenetration);

            // Invoca eventos
            if (enemyDied)
            {
                OnEnemyKilled?.Invoke(healthSystem);
            }

            OnTargetDamaged?.Invoke(healthSystem);

            // Invoca o evento de Acerto Crítico para os Behaviors (como Sangramento/Marcação)
            if (isCritical)
            {
                OnCriticalHit?.Invoke(healthSystem);
            }

        }
    }

    public void TakeDamage(float amount)
    {
        if (IsDestroyed) return;
        float remainingDamage = amount;

        // Aplica lógica de barreira / absorção de dano (se houver)
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f);
        foreach (var col in colliders)
        {
            SpiritualBarrierBehavior barrier = col.GetComponent<SpiritualBarrierBehavior>();
            // Nota: Este SpiritualBarrierBehavior precisa ser um script seu
            if (barrier != null && barrier.towerController != this)
            {
                remainingDamage = barrier.AbsorbDamage(remainingDamage);
            }
        }

        // Aplica armadura e calcula o dano final
        float finalDamage = remainingDamage * (1 - currentArmor);
        currentHealth -= finalDamage;

        // 🌟 NOVO LOG PARA CONFIRMAR O DANO DA AURA
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
        Debug.Log($"<color=red>TORRE DESTRUÍDA!</color> Torre {gameObject.name} foi destruída!");
        gameObject.SetActive(false);
    }

    public void Revive(float healthPercentage)
    {
        if (!IsDestroyed) return;
        IsDestroyed = false;
        // Usando a nova propriedade
        currentHealth = MaxHealth * healthPercentage;
        Debug.Log($"Torre {gameObject.name} foi revivida!");
    }

    // MÉTODOS PÚBLICOS E DE CONTROLE
    // CORRIGIDO: O método Heal usa a nova propriedade MaxHealth
    public void Heal(float amount) { currentHealth = Mathf.Min(currentHealth + amount, MaxHealth); }
    public void AddArmorBonus(float amount) { currentArmor += amount; }
    public void AddAttackSpeedBonus(float amount) { currentAttackSpeed *= (1 + amount); }
    public void AddDamageBonus(float amount) { currentDamage *= (1 + amount); }
    public void PerformExtraAttack() { Shoot(); }

    // --- MÉTODO CORRIGIDO PARA MIRAR EM INIMIGOS VOADORES ---
    void UpdateTarget()
    {
        // 1. Encontra todos os inimigos dentro do raio de alcance
        Collider[] collidersInRadius = Physics.OverlapSphere(transform.position, CurrentRange);

        Transform nearestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Collider collider in collidersInRadius)
        {
            if (collider.CompareTag(enemyTag))
            {
                // Tenta obter o EnemyController e o EnemyData
                EnemyController enemyController = collider.GetComponent<EnemyController>();

                if (enemyController == null || enemyController.enemyData == null) continue;

                EnemyType enemyType = enemyController.enemyData.enemyType;

                // Lógica de filtragem:
                bool isTargetable = (enemyType == EnemyType.Terrestre) ||
                                    (TargetsFlyingEnemies && enemyType == EnemyType.Voador);

                if (isTargetable)
                {
                    float distanceToEnemy = Vector3.Distance(transform.position, collider.transform.position);

                    // Lógica de Prioridade: Mudar o alvo apenas se for mais próximo (nearest target priority)
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