using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Necessário para usar .ToArray() no Awake

public class EnemyHealthSystem : MonoBehaviour
{
    [Header("Referências")]
    public EnemyDataSO enemyData;
    public Material markedMaterial;
    private Renderer enemyRenderer;

    // --- MODIFICAÇÃO: Armazenar todos os materiais originais ---
    private Material[] originalMaterials;

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
        // Tenta encontrar o Renderer no próprio GameObject ou nos filhos
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer == null)
        {
            enemyRenderer = GetComponentInChildren<Renderer>();
        }

        if (enemyRenderer != null)
        {
            // --- MUDANÇA PRINCIPAL: CLONA e salva todos os materiais originais ---
            // Usar .materials (plural) para clonar o array, garantindo que não altere assets originais.
            originalMaterials = enemyRenderer.materials.ToArray();
            Debug.Log($"<color=blue>EnemyHealthSystem:</color> Renderer '{enemyRenderer.gameObject.name}' encontrado. {originalMaterials.Length} materiais originais salvos.");
        }
        else
        {
            Debug.LogError($"<color=red>EnemyHealthSystem:</color> NENHUM RENDERER ENCONTRADO para o inimigo '{gameObject.name}'. A marcação visual NÃO FUNCIONARÁ!");
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

        // Garante que o inimigo comece com o material original ao ser reutilizado
        if (enemyRenderer != null && originalMaterials != null)
        {
            enemyRenderer.materials = originalMaterials;
        }
        isMarked = false;
    }

    public bool TakeDamage(float damage, float armorPenetration = 0f)
    {
        if (isDead) return false;

        float damageWithMark = damage * markedDamageMultiplier;

        float armorToIgnore = baseArmor * armorPenetration;
        float effectiveArmor = Mathf.Max(0, baseArmor - currentArmorModifier - armorToIgnore);
        float damageReduction = effectiveArmor;
        float damageMultiplier = 1f - damageReduction;

        float finalDamage = damageWithMark * damageMultiplier;

        if (markedDamageMultiplier > 1f)
        {
            Debug.Log($"<color=orange>Dano MARCADO:</color> Dano Base {damage.ToString("F1")}, Multiplicador de Marcação {markedDamageMultiplier.ToString("F2")}, Armadura Efetiva {effectiveArmor.ToString("F1")}. Dano Final: {finalDamage.ToString("F1")}");
        }

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
            Debug.Log($"<color=purple>ARMOR SHRED:</color> {gameObject.name} shredado. Stacks: {armorShredStacks}. Modificador atual: {currentArmorModifier}.");
        }
    }

    public void ApplyMarkedStatus(float multiplier)
    {
        markedDamageMultiplier = multiplier;

        if (enemyRenderer != null && markedMaterial != null && !isMarked)
        {
            // --- MUDANÇA PRINCIPAL: Cria e aplica um novo array com o MarkedMaterial em todas as slots ---
            Material[] markedMaterialsArray = new Material[enemyRenderer.materials.Length];
            for (int i = 0; i < markedMaterialsArray.Length; i++)
            {
                markedMaterialsArray[i] = markedMaterial;
            }
            enemyRenderer.materials = markedMaterialsArray;

            isMarked = true;
            Debug.Log($"<color=green>MARCADO VISUALMENTE:</color> {gameObject.name} agora com material de marcação em todas as {markedMaterialsArray.Length} slots.");
        }
        else if (isMarked)
        {
            // Apenas atualiza a lógica
            Debug.Log($"<color=yellow>JÁ MARCADO:</color> {gameObject.name} já estava marcado. Multiplicador atualizado para {multiplier}.");
        }
        else
        {
            Debug.LogError($"<color=red>FALHA VISUAL DE MARCAÇÃO:</color> {gameObject.name} marcado logicamente, mas material NÃO ALTERADO. MarkedMaterial Nulo? {markedMaterial == null}. Renderer Nulo? {enemyRenderer == null}");
        }
    }

    public void RemoveMarkedStatus()
    {
        markedDamageMultiplier = 1f;

        if (enemyRenderer != null && originalMaterials != null && isMarked)
        {
            // --- MUDANÇA PRINCIPAL: Restaura o array de materiais original ---
            enemyRenderer.materials = originalMaterials;

            isMarked = false;
            Debug.Log($"<color=blue>MARCAÇÃO REMOVIDA VISUALMENTE:</color> {gameObject.name} voltou ao material original.");
        }
        else if (!isMarked)
        {
            Debug.Log($"<color=gray>MARCAÇÃO JÁ REMOVIDA:</color> {gameObject.name} não estava marcado.");
        }
        else
        {
            Debug.LogError($"<color=red>FALHA VISUAL AO REMOVER MARCAÇÃO:</color> {gameObject.name} desmarcado logicamente, mas material NÃO RESTAURADO. OriginalMaterials Nulo? {originalMaterials == null}.");
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
            if (UnityEngine.Random.value <= enemyData.etherDropChance)
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
            Debug.LogError("Inimigo morreu sem um EnemyController! Desativando em vez de destruir.");
            gameObject.SetActive(false);
        }
    }
}