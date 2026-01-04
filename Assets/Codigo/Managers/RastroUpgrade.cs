using UnityEngine;
using System.Collections.Generic;

public enum CharacterStatType
{
    MaxHealth,
    Damage,
    MoveSpeed,
    AttackSpeed,
    Range,
    Armor,
    CritChance,
    CritDamage,
    ArmorPenetration
}

[System.Serializable]
public struct CharacterStatModifier
{
    public CharacterStatType statToModify;
    public ModificationType modType;
    public float value;
}

[CreateAssetMenu(fileName = "Novo Rastro Upgrade", menuName = "ScriptableObjects/Rastros/Rastro")]
public class RastroUpgrade : ScriptableObject
{
    [Header("Informações do Upgrade")]
    public string upgradeName;
    [TextArea] public string description;

    [Header("Modificadores de Status")]
    public List<CharacterStatModifier> modifiers;

    [Header("Comportamento Especial Desbloqueado")]
    [Tooltip("Arraste o PREFAB que contém a lógica especial (Ex: Aura de fogo, Rastro de gelo, etc).")]
    public MonoBehaviour behaviorToUnlock;
}