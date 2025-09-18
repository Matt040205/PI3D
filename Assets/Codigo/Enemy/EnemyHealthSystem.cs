// EnemyHealthSystem.cs
using UnityEngine;
using System.Collections;

public class EnemyHealthSystem : MonoBehaviour
{
    [Header("Referências")]
    public EnemyDataSO enemyData;
    public Material markedMaterial;
    private Renderer enemyRenderer;
    private Material originalMaterial;

    [Header("Status Atual")]
    public float currentHealth;
    public bool isDead;

    // --- Status de Combate ---
    private float baseArmor;
    private float currentArmorModifier = 0f;
    private int armorShredStacks = 0;
    private float markedDamageMultiplier = 1f;

    private EnemyController enemyController;
    private bool isMarked = false;

    public bool IsArmorShredded => armorShredStacks > 0;

    void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        enemyRenderer = GetComponentInChildren<Renderer>();
        if (enemyRenderer != null)
        {
            originalMaterial = enemyRenderer.material;
        }
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
        markedDamageMultiplier = 1f;
        isMarked = false;
    }

    public bool TakeDamage(float damage, float armorPenetration = 0f)
    {
        if (isDead) return false;

        float damageWithMark = damage * markedDamageMultiplier;
        float armorToIgnore = baseArmor * armorPenetration;
        float effectiveArmor = Mathf.Max(0, baseArmor - currentArmorModifier - armorToIgnore);
        float damageMultiplier = 1f - effectiveArmor;
        float finalDamage = damageWithMark * damageMultiplier;

        currentHealth -= finalDamage;

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
        }
    }

    public void ApplyMarkedStatus(float multiplier)
    {
        markedDamageMultiplier = multiplier;
        if (enemyRenderer != null && markedMaterial != null && !isMarked)
        {
            enemyRenderer.material = markedMaterial;
            isMarked = true;
            Debug.Log("<color=green>SUCESSO:</color> O material de " + gameObject.name + " foi alterado para o material de marcação.");
        }
        else
        {
            // NOVO LOG PARA DEPURAR A FALHA
            Debug.Log("<color=red>FALHA:</color> O material de " + gameObject.name + " não foi alterado. Motivo: " +
                      "Renderer Nulo? " + (enemyRenderer == null) +
                      ", MarkedMaterial Nulo? " + (markedMaterial == null) +
                      ", Já Marcado? " + isMarked);
        }
        Debug.Log("Inimigo " + gameObject.name + " foi marcado!");
    }

    public void RemoveMarkedStatus()
    {
        markedDamageMultiplier = 1f;
        if (enemyRenderer != null && originalMaterial != null && isMarked)
        {
            enemyRenderer.material = originalMaterial;
            isMarked = false;
        }
        Debug.Log("Inimigo " + gameObject.name + " não está mais marcado.");
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