using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeEffect", menuName = "ExoBeats/Upgrade Effect")]
public class UpgradeEffectSO : ScriptableObject
{
    [Header("Basic Info")]
    public string effectName = "New Effect";
    [TextArea] public string description;
    public Sprite icon;

    [Header("Stat Modifiers (referência: CharacterBase)")]
    [Tooltip("Aumenta a vida máxima em valor fixo.")]
    public float maxHealthModifier = 0f;

    [Tooltip("Aumenta o dano base em valor fixo.")]
    public float damageModifier = 0f;

    [Tooltip("Altera a velocidade de movimento em valor fixo.")]
    public float moveSpeedModifier = 0f;

    [Tooltip("Altera a velocidade de recarga (reload) em valor fixo.")]
    public float reloadSpeedModifier = 0f;

    [Tooltip("Altera a cadência de ataque (attackSpeed) em valor fixo.")]
    public float attackSpeedModifier = 0f;

    [Tooltip("Altera a quantidade do magazine (balas por recarga).")]
    public int magazineModifier = 0;

    [Header("Special Effects")]
    [Tooltip("Se definido, desbloqueia uma nova habilidade.")]
    public AbilitySO unlockAbility;

    [Tooltip("Prefab visual ao aplicar este upgrade (ex.: aura ou efeito).")]
    public GameObject effectPrefab;

    // --- Métodos utilitários ---
    public bool HasStatChanges =>
        maxHealthModifier != 0f || damageModifier != 0f || moveSpeedModifier != 0f ||
        reloadSpeedModifier != 0f || attackSpeedModifier != 0f || magazineModifier != 0;

    public bool UnlocksAbility => unlockAbility != null;
    public bool HasVFX => effectPrefab != null;
}