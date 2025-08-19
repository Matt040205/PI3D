using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Header("Debug / Options")]
    [Tooltip("Mostra logs no console quando upgrades são aplicados.")]
    public bool debugLogs = true;

    // Aplica todos os efeitos de um nível específico de um caminho de upgrade.
    public void ApplyUpgrade(UpgradePathSO path, int levelIndex, CharacterStats characterStats)
    {
        if (path == null)
        {
            if (debugLogs) Debug.LogWarning("UpgradeManager: Path é nulo!");
            return;
        }
        if (characterStats == null)
        {
            if (debugLogs) Debug.LogWarning("UpgradeManager: CharacterStats é nulo!");
            return;
        }
        if (path.levels == null || path.levels.Length == 0)
        {
            if (debugLogs) Debug.LogWarning($"UpgradeManager: Path '{path.pathName}' não possui níveis!");
            return;
        }
        if (levelIndex < 0 || levelIndex >= path.levels.Length)
        {
            if (debugLogs) Debug.LogWarning($"UpgradeManager: Índice {levelIndex} inválido para o path '{path.pathName}'.");
            return;
        }

        UpgradeLevel level = path.levels[levelIndex];
        if (level.effects == null || level.effects.Length == 0)
        {
            if (debugLogs) Debug.LogWarning($"UpgradeManager: Nível '{level.levelName}' não possui efeitos.");
            return;
        }

        foreach (UpgradeEffectSO effect in level.effects)
        {
            if (effect == null) continue;
            ApplyEffect(effect, characterStats);
        }

        if (debugLogs)
            Debug.Log($"[UpgradeManager] Aplicado upgrade '{level.levelName}' do caminho '{path.pathName}' no alvo '{characterStats.runtimeOwner?.name ?? "sem owner"}'");
    }

    private void ApplyEffect(UpgradeEffectSO effect, CharacterStats characterStats)
    {
        if (effect == null || characterStats == null) return;

        // Aplicação direta dos modificadores (match com CharacterBase)
        characterStats.maxHealth += effect.maxHealthModifier;
        characterStats.damage += effect.damageModifier;
        characterStats.moveSpeed += effect.moveSpeedModifier;
        characterStats.reloadSpeed += effect.reloadSpeedModifier;
        characterStats.attackSpeed += effect.attackSpeedModifier;
        characterStats.magazineSize += effect.magazineModifier;

        // Desbloqueia habilidade se existir
        if (effect.unlockAbility != null)
            characterStats.UnlockAbility(effect.unlockAbility);

        // Spawna VFX se existir
        if (effect.effectPrefab != null && characterStats.runtimeOwner != null)
            Instantiate(effect.effectPrefab, characterStats.runtimeOwner.position, Quaternion.identity);
    }
}