using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "ExoBeats/Ability")]
public class AbilitySO : ScriptableObject
{
    [Header("Basic Info")]
    public string abilityName = "New Ability";
    [TextArea] public string description;
    public Sprite icon;

    [Header("Gameplay Settings")]
    [Tooltip("Cooldown em segundos para reutilizar a habilidade.")]
    public float cooldown = 5f;

    [Tooltip("Prefab que ser� instanciado ao ativar a habilidade (VFX/Hitbox/Proj�til).")]
    public GameObject effectPrefab;

    [Tooltip("Tipo da habilidade: ativa, passiva, ultimate, etc.")]
    public AbilityType abilityType = AbilityType.Active;

    [Header("Extra Settings")]
    [Tooltip("Define se essa habilidade � considerada a Ultimate do personagem.")]
    public bool isUltimate = false;

    [Tooltip("Som opcional tocado ao usar a habilidade.")]
    public AudioClip sfx;

    [Tooltip("Part�cula/VFX adicional para spawnar na ativa��o.")]
    public GameObject vfxOnCast;

    // --- M�todos utilit�rios ---
    public bool HasPrefab => effectPrefab != null;
    public bool HasVFX => vfxOnCast != null;
    public bool HasSFX => sfx != null;
}

public enum AbilityType
{
    Passive,   // Sempre ativa, n�o precisa de input
    Active,    // Usada manualmente
    Ultimate   // Ultimate, mais forte ou especial
}