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

    [Tooltip("Prefab que será instanciado ao ativar a habilidade (VFX/Hitbox/Projétil).")]
    public GameObject effectPrefab;

    [Tooltip("Tipo da habilidade: ativa, passiva, ultimate, etc.")]
    public AbilityType abilityType = AbilityType.Active;

    [Header("Extra Settings")]
    [Tooltip("Define se essa habilidade é considerada a Ultimate do personagem.")]
    public bool isUltimate = false;

    [Tooltip("Som opcional tocado ao usar a habilidade.")]
    public AudioClip sfx;

    [Tooltip("Partícula/VFX adicional para spawnar na ativação.")]
    public GameObject vfxOnCast;

    // --- Métodos utilitários ---
    public bool HasPrefab => effectPrefab != null;
    public bool HasVFX => vfxOnCast != null;
    public bool HasSFX => sfx != null;
}

public enum AbilityType
{
    Passive,   // Sempre ativa, não precisa de input
    Active,    // Usada manualmente
    Ultimate   // Ultimate, mais forte ou especial
}