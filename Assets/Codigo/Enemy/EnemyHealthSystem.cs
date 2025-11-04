using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemyHealthSystem : MonoBehaviour
{
    [Header("Referências")]
    public EnemyDataSO enemyData;
    public Material markedMaterial;
    private Renderer enemyRenderer;
    private WorldSpaceEnemyUI worldSpaceUI;

    private Material[] originalMaterials;

    [Header("Status Atual")]
    public float currentHealth;
    public bool isDead;

    private float baseArmor;
    private float currentArmorModifier = 0f;
    private int armorShredStacks = 0;
    private float markedDamageMultiplier = 1f;
    private float vulnerabilityMultiplier = 1f;

    private EnemyController enemyController;
    private bool isMarked = false;

    public bool IsArmorShredded => armorShredStacks > 0;

    void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        worldSpaceUI = GetComponentInChildren<WorldSpaceEnemyUI>();
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer == null)
        {
            enemyRenderer = GetComponentInChildren<Renderer>();
        }

        if (enemyRenderer != null)
        {
            originalMaterials = enemyRenderer.materials.ToArray();
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
        vulnerabilityMultiplier = 1f;

        if (enemyRenderer != null && originalMaterials != null)
        {
            enemyRenderer.materials = originalMaterials;
        }
        isMarked = false;
    }

    // --- FUNÇÃO CORRIGIDA ---
    // Agora aceita o parâmetro "isCritical"
    public bool TakeDamage(float damage, float armorPenetration = 0f, bool isCritical = false)
    {
        if (isDead) return false;

        float damageWithMark = damage * markedDamageMultiplier * vulnerabilityMultiplier;

        float armorToIgnore = baseArmor * armorPenetration;
        float effectiveArmor = Mathf.Max(0, baseArmor - currentArmorModifier - armorToIgnore);
        float damageReduction = effectiveArmor;
        float damageMultiplier = 1f - damageReduction;

        float finalDamage = damageWithMark * damageMultiplier;

        if (markedDamageMultiplier > 1f || vulnerabilityMultiplier > 1f)
        {
            Debug.Log($"<color=orange>Dano Modificado:</color> Dano Base {damage.ToString("F1")}, Multiplicador (Mark*Vuln) {(markedDamageMultiplier * vulnerabilityMultiplier).ToString("F2")}, Armadura Efetiva {effectiveArmor.ToString("F1")}. Dano Final: {finalDamage.ToString("F1")}");
        }

        // --- CHAMADA CORRIGIDA ---
        // Agora passa o "isCritical" para a UI
        if (worldSpaceUI != null && finalDamage > 0)
        {
            worldSpaceUI.ShowDamageNumber(finalDamage, isCritical);
        }
        // -------------------------

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

    public void AplicarVulnerabilidade(float multiplicador)
    {
        vulnerabilityMultiplier = multiplicador;
    }

    public void RemoverVulnerabilidade()
    {
        vulnerabilityMultiplier = 1f;
    }

    public void ApplyMarkedStatus(float multiplier)
    {
        markedDamageMultiplier = multiplier;

        if (enemyRenderer != null && markedMaterial != null && !isMarked)
        {
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