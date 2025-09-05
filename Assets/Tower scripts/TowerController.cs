// TowerController.cs (VERSÃO FINAL CORRIGIDA)
using UnityEngine;
using System;
using System.Linq;

public class TowerController : MonoBehaviour
{
    [Header("Referências")]
    public CharacterBase towerData;
    public Transform partToRotate;
    public Transform firePoint;

    [Header("Configurações de IA")]
    [SerializeField] private string enemyTag = "Enemy";

    // --- Eventos para os Behaviors ---
    public event Action<EnemyHealthSystem> OnTargetDamaged;
    public event Func<EnemyHealthSystem, float, float> OnCalculateDamage;
    public event Action<EnemyHealthSystem> OnCriticalHit;

    // --- Propriedade Pública de Estado ---
    public bool IsDestroyed { get; private set; }

    // --- Status Base (Carregados no início e não mudam) ---
    private float maxHealth;
    private float baseArmor;
    private float baseDamage;
    private float baseAttackSpeed;
    private float baseCritChance;
    private float baseCritDamage;
    private float baseArmorPenetration;
    private float baseRange; // <<< VARIÁVEL RESTAURADA

    // --- Status Atuais e Modificadores de Buff ---
    private float currentHealth;
    private float armorBonus = 0f;
    private float attackSpeedBonusMultiplier = 1.0f;
    private float damageBonusMultiplier = 1.0f;

    // --- Variáveis de Controle Interno ---
    private int[] currentPathLevels;
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

        // Carrega os status base do ScriptableObject
        maxHealth = towerData.maxHealth;
        baseArmor = towerData.armor;
        baseDamage = towerData.damage;
        baseAttackSpeed = towerData.attackSpeed;
        baseCritChance = towerData.critChance;
        baseCritDamage = towerData.critDamage;
        baseArmorPenetration = towerData.armorPenetration;
        baseRange = towerData.meleeRange; // <<< LINHA RESTAURADA

        // Inicializa os status de combate
        currentHealth = maxHealth;
        IsDestroyed = false;

        currentPathLevels = new int[towerData.upgradePaths.Count];
        InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }

    void Update()
    {
        if (IsDestroyed) return;
        if (targetEnemy == null) return;
        if (partToRotate != null) RotateTowardsTarget();

        fireCountdown -= Time.deltaTime;
        if (fireCountdown <= 0f)
        {
            float finalAttackSpeed = baseAttackSpeed * attackSpeedBonusMultiplier;
            fireCountdown = 1f / finalAttackSpeed;
            Shoot();
        }
    }

    void Shoot()
    {
        if (targetEnemy == null) return;
        EnemyHealthSystem healthSystem = targetEnemy.GetComponent<EnemyHealthSystem>();
        if (healthSystem != null)
        {
            float damageToDeal = baseDamage * damageBonusMultiplier;

            bool isCritical = UnityEngine.Random.value <= baseCritChance;
            if (isCritical)
            {
                damageToDeal *= baseCritDamage;
            }

            if (OnCalculateDamage != null)
            {
                foreach (Func<EnemyHealthSystem, float, float> modifier in OnCalculateDamage.GetInvocationList())
                {
                    damageToDeal = modifier(healthSystem, damageToDeal);
                }
            }

            healthSystem.TakeDamage(damageToDeal, baseArmorPenetration);
            OnTargetDamaged?.Invoke(healthSystem);

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

        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f);
        foreach (var col in colliders)
        {
            SpiritualBarrierBehavior barrier = col.GetComponent<SpiritualBarrierBehavior>();
            if (barrier != null && barrier.towerController != this)
            {
                remainingDamage = barrier.AbsorbDamage(remainingDamage);
            }
        }

        float totalArmor = Mathf.Clamp01(baseArmor + armorBonus);
        float finalDamage = remainingDamage * (1 - totalArmor);
        currentHealth -= finalDamage;

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
        Debug.Log($"Torre {gameObject.name} foi destruída!");
    }

    public void Revive(float healthPercentage)
    {
        if (!IsDestroyed) return;
        IsDestroyed = false;
        currentHealth = maxHealth * healthPercentage;
        Debug.Log($"Torre {gameObject.name} foi revivida!");
    }

    // --- MÉTODOS PÚBLICOS PARA OS BEHAVIORS ---
    public void Heal(float amount) { currentHealth = Mathf.Min(currentHealth + amount, maxHealth); }
    public void AddArmorBonus(float amount) { armorBonus += amount; }
    public void AddAttackSpeedBonus(float amount) { attackSpeedBonusMultiplier += amount; }
    public void AddDamageBonus(float amount) { damageBonusMultiplier += amount; }
    public void PerformExtraAttack() { Shoot(); }

    // --- MÉTODOS DE CONTROLE DA TORRE ---
    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        if (enemies.Length == 0)
        {
            targetEnemy = null;
            return;
        }
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

        // <<< CORREÇÃO AQUI ---
        if (nearestEnemy != null && shortestDistance <= baseRange)
        {
            targetEnemy = nearestEnemy;
        }
        else
        {
            targetEnemy = null;
        }
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
        // <<< CORREÇÃO AQUI ---
        Gizmos.DrawWireSphere(transform.position, baseRange);
    }
}