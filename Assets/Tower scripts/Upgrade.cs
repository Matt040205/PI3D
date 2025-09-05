// Upgrade.cs
using UnityEngine;
using System.Collections.Generic;

// --- MODIFICA��O AQUI ---
public enum StatType { Damage, AttackSpeed, Range, Armor, CritChance, CritDamage, ArmorPenetration }
public enum ModificationType { Additive, Multiplicative }

[System.Serializable]
public struct StatModifier
{
    public StatType statToModify;
    public ModificationType modType;
    public float value;
}

[CreateAssetMenu(fileName = "Novo Upgrade", menuName = "ExoBeasts/Torres/Upgrade")]
public class Upgrade : ScriptableObject
{
    [Header("Informa��es do Upgrade")]
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Custos")]
    public int geoditeCost;
    public int darkEtherCost;

    [Header("B�nus de Status Simples")]
    public List<StatModifier> modifiers;

    [Header("Comportamento Especial Desbloqueado")]
    [Tooltip("Arraste o PREFAB do comportamento que este upgrade desbloqueia (Ex: DoubleAttackBehavior).")]
    public TowerBehavior behaviorToUnlock;
}