using UnityEngine;
using System.Collections.Generic;

// Os Enums e a Struct de StatModifier continuam aqui
public enum StatType { Damage, AttackSpeed, Range, Armor }
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
    [Header("Informações do Upgrade")]
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Custos")]
    public int geoditeCost;
    public int darkEtherCost;

    [Header("Bônus de Status Simples")]
    public List<StatModifier> modifiers;

    [Header("Comportamento Especial Desbloqueado")]
    [Tooltip("Arraste o SCRIPT do comportamento que este upgrade desbloqueia (Ex: DoubleAttackBehavior).")]
    public TowerBehavior behaviorToUnlock;
}