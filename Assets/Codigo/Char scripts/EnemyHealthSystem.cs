// EnemyHealthSystem.cs (MODIFICADO)
using UnityEngine;

public class EnemyHealthSystem : MonoBehaviour
{
    [Header("Referências")]
    public EnemyDataSO enemyData;

    [Header("Status Atual")]
    public float currentHealth;
    public bool isDead;

    // --- Status de Combate ---
    private float baseArmor;
    private float currentArmorModifier = 0f;
    private int armorShredStacks = 0;

    private EnemyController enemyController;

    public bool IsArmorShredded => armorShredStacks > 0;
    void Awake()
    {
        enemyController = GetComponent<EnemyController>();
    }

    public void InitializeHealth(int level)
    {
        if (enemyData == null)
        {
            Debug.LogError("EnemyData não atribuído em " + gameObject.name);
            return;
        }
        currentHealth = enemyData.GetHealth(level);
        baseArmor = enemyData.GetArmor(level);
        currentArmorModifier = 0f;
        armorShredStacks = 0;
        isDead = false;
    }

    public bool TakeDamage(float damage, float armorPenetration = 0f)
    {
        if (isDead) return false;

        // Calcula a porção da armadura que será ignorada pelo ataque atual
        float armorToIgnore = baseArmor * armorPenetration;

        // A armadura efetiva considera a redução de armadura (debuff) E a penetração (do ataque)
        float effectiveArmor = Mathf.Max(0, baseArmor - currentArmorModifier - armorToIgnore);
        float damageMultiplier = 1f - effectiveArmor;
        float finalDamage = damage * damageMultiplier;

        currentHealth -= finalDamage;
        // Debug.Log($"{gameObject.name} tomou {finalDamage} de dano (Dano base: {damage}, Armadura Efetiva: {effectiveArmor * 100}%). Vida restante: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
            return true;
        }
        return false;
    }

    public void ApplyArmorShred(float percentage, int maxStacks)
    {
        if (armorShredStacks < maxStacks)
        {
            armorShredStacks++;
            currentArmorModifier += percentage;
            // Debug.Log($"ARMOR SHRED aplicado em {gameObject.name}! Stacks: {armorShredStacks}, Redução total: {currentArmorModifier * 100}%");
        }
    }

    private void Die()
    {
        isDead = true;
        if (CurrencyManager.Instance != null && enemyData != null)
        {
            int geoditesAmount = enemyData.geoditasOnDeath;
            if (geoditesAmount > 0)
            {
                CurrencyManager.Instance.AddCurrency(geoditesAmount, CurrencyType.Geodites);
            }

            if (Random.value <= enemyData.etherDropChance)
            {
                CurrencyManager.Instance.AddCurrency(1, CurrencyType.DarkEther);
            }
        }

        if (enemyController != null)
        {
            enemyController.HandleDeath();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}