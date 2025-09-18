// TowerController.cs
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class TowerController : MonoBehaviour
{
    [Header("Refer�ncias")]
    public CharacterBase towerData;
    public Transform partToRotate;
    public Transform firePoint;

    [Header("Configura��es de IA")]
    [SerializeField] private string enemyTag = "Enemy";

    // --- Eventos para os Behaviors ---
    public event Action<EnemyHealthSystem> OnTargetDamaged;
    public event Func<EnemyHealthSystem, float, float> OnCalculateDamage;
    public event Action<EnemyHealthSystem> OnCriticalHit;
    public event Action<EnemyHealthSystem> OnEnemyKilled; // NOVO EVENTO ADICIONADO AQUI

    // --- Propriedade P�blica de Estado ---
    public bool IsDestroyed { get; private set; }

    // --- Status Atuais da Inst�ncia da Torre ---
    private float currentHealth;
    private float maxHealth;
    private float currentArmor;
    private float currentDamage;
    private float currentAttackSpeed;
    private float currentCritChance;
    private float currentCritDamage;
    private float currentArmorPenetration;
    private float currentRange;

    // --- Vari�veis de Controle Interno ---
    private List<TowerBehavior> activeBehaviors = new List<TowerBehavior>();
    public int[] currentPathLevels;
    private Transform targetEnemy;
    private float fireCountdown = 0f;

    void Start()
    {
        if (towerData == null)
        {
            Debug.LogError("FALHA: TowerData est� NULO na torre '" + gameObject.name + "'!", this.gameObject);
            this.enabled = false;
            return;
        }
        currentPathLevels = new int[towerData.upgradePaths.Count];
        CloneBaseStats();
        InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }

    void CloneBaseStats()
    {
        maxHealth = towerData.maxHealth;
        currentHealth = maxHealth;
        currentArmor = towerData.armor;
        currentDamage = towerData.damage;
        currentAttackSpeed = towerData.attackSpeed;
        currentCritChance = towerData.critChance;
        currentCritDamage = towerData.critDamage;
        currentArmorPenetration = towerData.armorPenetration;
        currentRange = towerData.meleeRange;
        IsDestroyed = false;
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
                // ESTE LOG CONFIRMA QUE A HABILIDADE FOI ADICIONADA
                Debug.Log($"<color=yellow>HABILIDADE DESBLOQUEADA: '{newBehavior.GetType().Name}' foi adicionada � torre {gameObject.name}.</color>");
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
                currentRange = (modifier.modType == ModificationType.Additive) ? currentRange + value : currentRange * (1 + value);
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

    // ===== �REA DE TESTE COM LOGS DE DANO =====
    void Shoot()
    {
        if (targetEnemy == null)
            return;
        EnemyHealthSystem healthSystem = targetEnemy.GetComponent<EnemyHealthSystem>();
        Debug.Log("0");
        if (healthSystem != null)
        {
            float damageToDeal = currentDamage;
            bool isCritical = UnityEngine.Random.value <= currentCritChance;

            string logMessage = $"<color=white>{gameObject.name} atacou {targetEnemy.name}.</color> ";
            Debug.Log("1");
            if (isCritical)
            {
                float criticalDamage = damageToDeal * currentCritDamage;
                logMessage += $"<color=red>CR�TICO! Dano: {criticalDamage.ToString("F1")}</color> (Base: {damageToDeal.ToString("F1")} * {currentCritDamage.ToString("F1")})";
                damageToDeal = criticalDamage;
            }
            else
            {
                logMessage += $"<color=green>Dano normal: {damageToDeal.ToString("F1")}</color>";
            }
            Debug.Log("2");
            // Permite que outros scripts (habilidades) modifiquem o dano antes de ser aplicado
            if (OnCalculateDamage != null)
            {
                float originalDamage = damageToDeal;
                foreach (Func<EnemyHealthSystem, float, float> modifier in OnCalculateDamage.GetInvocationList())
                {
                    damageToDeal = modifier(healthSystem, damageToDeal);
                }
                if (originalDamage != damageToDeal)
                {
                    logMessage += $" | <color=lightblue>Dano modificado por habilidade para: {damageToDeal.ToString("F1")}</color>";
                }
                Debug.Log("3");
            }
      

                // NOVO LOG DE DANO!
                Debug.Log(logMessage);

            bool enemyDied = healthSystem.TakeDamage(damageToDeal, currentArmorPenetration);

            // Verifica se o inimigo morreu para invocar o novo evento
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
        Debug.Log("4");
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
        Debug.Log($"Torre {gameObject.name} foi destru�da!");
    }

    public void Revive(float healthPercentage)
    {
        if (!IsDestroyed) return;
        IsDestroyed = false;
        currentHealth = maxHealth * healthPercentage;
        Debug.Log($"Torre {gameObject.name} foi revivida!");
    }

    // M�TODOS P�BLICOS E DE CONTROLE (Sem altera��es)
    public void Heal(float amount) { currentHealth = Mathf.Min(currentHealth + amount, maxHealth); }
    public void AddArmorBonus(float amount) { currentArmor += amount; }
    public void AddAttackSpeedBonus(float amount) { currentAttackSpeed *= (1 + amount); }
    public void AddDamageBonus(float amount) { currentDamage *= (1 + amount); }
    public void PerformExtraAttack() { Shoot(); }

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
        if (nearestEnemy != null && shortestDistance <= currentRange) { targetEnemy = nearestEnemy; }
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
        Gizmos.DrawWireSphere(transform.position, currentRange);
    }
}