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
    private float markedDamageMultiplier = 1f; // Valor padrão de 1f

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

        // O dano é multiplicado pelo fator de marcação (ex: 10 * 1.25 = 12.5)
        float damageWithMark = damage * markedDamageMultiplier;

        float armorToIgnore = baseArmor * armorPenetration;
        float effectiveArmor = Mathf.Max(0, baseArmor - currentArmorModifier - armorToIgnore);
        float damageReduction = effectiveArmor;
        float damageMultiplier = 1f - damageReduction;

        // O dano final é aplicado após a redução de armadura
        float finalDamage = damageWithMark * damageMultiplier;

        // --- ADIÇÃO PARA DEPURAR O DANO E CONFIRMAR O MULTIPLICADOR ---
        if (markedDamageMultiplier > 1f)
        {
            Debug.Log($"<color=orange>Dano MARCADO:</color> Dano Base {damage}, Multiplicador de Marcação {markedDamageMultiplier}, Armadura Efetiva {effectiveArmor:P0}. Dano Final: {finalDamage}");
        }
        // ---------------------------------------------------------------

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
            Debug.Log($"<color=green>SUCESSO:</color> Inimigo {gameObject.name} marcado! Multiplicador de dano: {multiplier}.");
        }
        else if (isMarked)
        {
            // Se já estiver marcado, apenas atualiza o multiplicador e o tempo de marcação
            Debug.Log($"<color=yellow>ATENÇÃO:</color> Inimigo {gameObject.name} já estava marcado. Multiplicador atualizado para {multiplier}.");
        }
        else
        {
            // Log para depurar a falha na alteração visual
            Debug.Log($"<color=red>FALHA VISUAL:</color> Inimigo {gameObject.name} marcado, mas material não foi alterado. Renderer Nulo? {enemyRenderer == null}, MarkedMaterial Nulo? {markedMaterial == null}");
        }
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