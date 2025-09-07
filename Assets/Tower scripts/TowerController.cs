// TowerController.cs (VERSÃO FINAL PARA TESTES)
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic; // Necessário para a lista de comportamentos

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

    // --- Status Base (Carregados no início) ---
    private float maxHealth;
    private float baseArmor;
    private float baseDamage;
    private float baseAttackSpeed;
    private float baseCritChance;
    private float baseCritDamage;
    private float baseArmorPenetration;
    private float baseRange;

    // --- Status Atuais e Modificadores de Buff ---
    private float currentHealth;
    private float armorBonus = 0f;
    private float attackSpeedBonusMultiplier = 1.0f;
    private float damageBonusMultiplier = 1.0f;

    // --- Variáveis de Controle Interno ---
    private List<TowerBehavior> activeBehaviors = new List<TowerBehavior>();
    public int[] currentPathLevels; // Público para a UI de teste poder ler e modificar
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
        ApplyStatsFromBase(); // Carrega os status iniciais
    }

    // Carrega ou recarrega os status a partir do ScriptableObject
    void ApplyStatsFromBase()
    {
        maxHealth = towerData.maxHealth;
        baseArmor = towerData.armor;
        baseDamage = towerData.damage;
        baseAttackSpeed = towerData.attackSpeed;
        baseCritChance = towerData.critChance;
        baseCritDamage = towerData.critDamage;
        baseArmorPenetration = towerData.armorPenetration;
        baseRange = towerData.meleeRange;

        // Reseta a vida se estiver recarregando os status
        if (currentHealth > maxHealth || currentHealth <= 0 && !IsDestroyed)
        {
            currentHealth = maxHealth;
        }

        IsDestroyed = false; // Garante que a torre não comece destruída
    }

    // --- MÉTODO-CHAVE PARA APLICAR UPGRADES ---
    public void ApplyUpgrade(Upgrade upgradeToApply)
    {
        if (upgradeToApply == null)
        {
            Debug.LogError("Tentativa de aplicar um upgrade NULO!");
            return;
        }

        Debug.Log($"<color=cyan>Aplicando upgrade '{upgradeToApply.upgradeName}' na torre {gameObject.name}</color>");

        // 1. Aplica os modificadores de status (direto no ScriptableObject para ser permanente)
        foreach (var modifier in upgradeToApply.modifiers)
        {
            ApplyModifier(modifier);
        }

        // 2. Adiciona o novo comportamento, se houver
        if (upgradeToApply.behaviorToUnlock != null)
        {
            GameObject behaviorObject = Instantiate(upgradeToApply.behaviorToUnlock.gameObject, transform);
            TowerBehavior newBehavior = behaviorObject.GetComponent<TowerBehavior>();

            if (newBehavior != null)
            {
                newBehavior.Initialize(this);
                activeBehaviors.Add(newBehavior);
                Debug.Log($"Comportamento '{newBehavior.GetType().Name}' adicionado.");
            }
        }

        // 3. Re-aplica todos os status para que as mudanças sejam refletidas imediatamente
        ApplyStatsFromBase();
    }

    // Método auxiliar para aplicar modificadores de status
    private void ApplyModifier(StatModifier modifier)
    {
        float value = modifier.value;
        // NOTA: A lógica aqui assume que bônus multiplicativos são baseados no valor original.
        // Uma lógica mais complexa poderia ser necessária para múltiplos bônus multiplicativos.
        switch (modifier.statToModify)
        {
            case StatType.Damage:
                towerData.damage = (modifier.modType == ModificationType.Additive) ? towerData.damage + value : towerData.damage * (1 + value);
                break;
            case StatType.AttackSpeed:
                towerData.attackSpeed = (modifier.modType == ModificationType.Additive) ? towerData.attackSpeed + value : towerData.attackSpeed * (1 + value);
                break;
            case StatType.Range:
                towerData.meleeRange = (modifier.modType == ModificationType.Additive) ? towerData.meleeRange + value : towerData.meleeRange * (1 + value);
                break;
            // Adicione os outros casos (Armor, CritChance, etc.) aqui seguindo o mesmo padrão
            case StatType.Armor:
                towerData.armor = Mathf.Clamp01(towerData.armor + value);
                break;
            case StatType.CritChance:
                towerData.critChance = Mathf.Clamp01(towerData.critChance + value);
                break;
            case StatType.ArmorPenetration:
                towerData.armorPenetration = Mathf.Clamp01(towerData.armorPenetration + value);
                break;
        }
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

    // MÉTODOS PÚBLICOS PARA BUFFS
    public void Heal(float amount) { currentHealth = Mathf.Min(currentHealth + amount, maxHealth); }
    public void AddArmorBonus(float amount) { armorBonus += amount; }
    public void AddAttackSpeedBonus(float amount) { attackSpeedBonusMultiplier += amount; }
    public void AddDamageBonus(float amount) { damageBonusMultiplier += amount; }
    public void PerformExtraAttack() { Shoot(); }

    // MÉTODOS DE CONTROLE
    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        if (enemies.Length == 0) { targetEnemy = null; return; }
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
        if (nearestEnemy != null && shortestDistance <= baseRange) { targetEnemy = nearestEnemy; }
        else { targetEnemy = null; }
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
        Gizmos.DrawWireSphere(transform.position, baseRange);
    }
}