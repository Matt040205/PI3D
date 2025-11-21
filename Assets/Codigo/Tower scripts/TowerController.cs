using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class TowerController : MonoBehaviour
{
    [Header("Referências Principais")]
    public CharacterBase towerData;
    [Tooltip("Arraste o modelo visual (Filho) aqui. NÃO arraste a base.")]
    public Transform partToRotate;
    [Tooltip("Objeto vazio na ponta da arma.")]
    public Transform firePoint;

    [Header("Visual e Animação")]
    [Tooltip("Arraste o Animator do modelo aqui.")]
    public Animator animator;

    [Tooltip("Nome do Trigger de ataque no Animator.")]
    public string shootTrigger = "Attack";

    [Tooltip("Nome do Bool que impede a personagem de cair/pular.")]
    public string towerModeBool = "IsTower";

    [Tooltip("Ajuste se a personagem estiver mirando torto (Ex: 0, 90, 0).")]
    public Vector3 rotationOffset;

    [Header("Configurações de IA")]
    [SerializeField] private string enemyTag = "Enemy";
    [Tooltip("Define se a torre pode mirar em inimigos do tipo Voador.")]
    public bool TargetsFlyingEnemies { get; set; } = false;

    // Eventos do Sistema de Combate
    public event Action<EnemyHealthSystem> OnTargetDamaged;
    public event Func<EnemyHealthSystem, float, float> OnCalculateDamage;
    public event Action<EnemyHealthSystem> OnCriticalHit;
    public event Action<EnemyHealthSystem> OnEnemyKilled;

    // Estado da Torre
    public bool IsDestroyed { get; private set; }
    public float MaxHealth { get; private set; }
    public float CurrentRange { get; private set; }

    [HideInInspector]
    public int totalCostSpent { get; private set; }

    // Atributos de RPG (Stats)
    private float currentHealth;
    private float currentArmor;
    private float currentDamage;
    private float currentAttackSpeed;
    private float currentCritChance;
    private float currentCritDamage;
    private float currentArmorPenetration;

    // Variáveis Internas
    private List<TowerBehavior> activeBehaviors = new List<TowerBehavior>();
    public int[] currentPathLevels;
    private Transform targetEnemy;
    private float fireCountdown = 0f;

    void Start()
    {
        // 1. Verificação de Segurança
        if (towerData == null)
        {
            Debug.LogError("FALHA: TowerData está NULO na torre '" + gameObject.name + "'!", this.gameObject);
            this.enabled = false;
            return;
        }

        // 2. Inicializar Stats
        currentPathLevels = new int[towerData.upgradePaths.Count];
        CloneBaseStats();

        // 3. Configuração Automática do Animator e Física
        SetupTowerMode();

        // 4. Iniciar Radar
        InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }

    // Configura a personagem para se comportar como Torre (sem cair/pular)
    void SetupTowerMode()
    {
        // Tenta achar o Animator se não foi arrastado
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            // Ativa o modo torre para impedir transições de Jump/Fall
            animator.SetBool(towerModeBool, true);
        }

        // Desativa CharacterController para não colidir ou tentar mover
        CharacterController cc = GetComponentInChildren<CharacterController>();
        if (cc != null) cc.enabled = false;

        // Desativa a gravidade do Rigidbody para não cair
        Rigidbody rb = GetComponentInChildren<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
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

        Debug.Log($"<color=green>TORRE INICIALIZADA:</color> {gameObject.name} (Dano: {currentDamage})");
    }

    public void ApplyUpgrade(Upgrade upgradeToApply, int geoditeCost, int darkEtherCost)
    {
        if (upgradeToApply == null) return;

        totalCostSpent += geoditeCost;
        totalCostSpent += darkEtherCost;

        Debug.Log($"<color=cyan>Aplicando upgrade '{upgradeToApply.upgradeName}'</color>");

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
        if (IsDestroyed) return;

        // Se não tiver inimigo, não faz nada (mantém animação Idle)
        if (targetEnemy == null) return;

        // Gira o modelo visual em direção ao inimigo
        if (partToRotate != null) RotateTowardsTarget();

        // Controle de tiro
        fireCountdown -= Time.deltaTime;
        if (fireCountdown <= 0f)
        {
            fireCountdown = 1f / currentAttackSpeed;
            Shoot();
        }
    }

    void Shoot()
    {
        if (targetEnemy == null) return;

        // Dispara animação
        if (animator != null)
        {
            animator.SetTrigger(shootTrigger);
        }

        // Lógica de Dano
        EnemyHealthSystem healthSystem = targetEnemy.GetComponent<EnemyHealthSystem>();

        if (healthSystem != null)
        {
            float damageToDeal = currentDamage;
            bool isCritical = UnityEngine.Random.value <= currentCritChance;

            if (isCritical)
            {
                damageToDeal *= currentCritDamage;
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

    // Lógica de Rotação (com correção de Offset)
    void RotateTowardsTarget()
    {
        Vector3 direction = targetEnemy.position - transform.position;
        direction.y = 0; // Mantém a rotação apenas no eixo horizontal

        if (direction == Vector3.zero) return;

        // Rotação para o inimigo
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        // Aplica o Offset (correção para modelos tortos)
        Quaternion offsetRotation = Quaternion.Euler(rotationOffset);
        Quaternion finalTargetRotation = lookRotation * offsetRotation;

        // Suaviza o movimento (Lerp)
        Vector3 smoothedRotation = Quaternion.Lerp(partToRotate.rotation, finalTargetRotation, Time.deltaTime * 10f).eulerAngles;

        // Aplica a rotação apenas no eixo Y
        partToRotate.rotation = Quaternion.Euler(0f, smoothedRotation.y, 0f);
    }

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

        Debug.Log($"<color=red>DANO RECEBIDO:</color> {gameObject.name} sofreu {finalDamage:F1} de dano.");

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
        Destroy(gameObject);
    }

    public void Revive(float healthPercentage)
    {
        if (!IsDestroyed) return;
        IsDestroyed = false;
        currentHealth = MaxHealth * healthPercentage;
    }

    public void Heal(float amount) { currentHealth = Mathf.Min(currentHealth + amount, MaxHealth); }
    public void AddArmorBonus(float amount) { currentArmor += amount; }
    public void AddAttackSpeedBonus(float amount) { currentAttackSpeed *= (1 + amount); }
    public void AddDamageBonus(float amount) { currentDamage *= (1 + amount); }
    public void PerformExtraAttack() { Shoot(); }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, CurrentRange);
    }
}