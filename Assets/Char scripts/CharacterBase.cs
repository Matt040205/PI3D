using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Tower Defense/Character Base")]
public class CharacterBase : ScriptableObject
{
    [Header("Basic Stats")]
    public float maxHealth = 100f;
    public float damage = 10f;
    public float moveSpeed = 5f;
    public float reloadSpeed = 2f;
    public float attackSpeed = 1f; // Tiros por segundo
    public float meleeRange = 2f; // Pode ser usado como Range da torre

    [Header("Combat Settings")]
    public CombatType combatType = CombatType.Ranged;
    public FireMode fireMode = FireMode.SemiAuto;
    public float meleeAngle = 90f;

    [Header("Type Settings")]
    public bool isCommander = true;
    public Sprite characterIcon;

    [Header("Commander Specifics")]
    public int magazineSize = 10;
    public PassivaAbility passive;
    public Ability ability1;
    public Ability ability2;
    public Ability ultimate;

    // --- ADIÇÃO AQUI ---
    [Header("Tower Upgrades")]
    [Tooltip("A lista de caminhos de upgrade disponíveis quando este personagem é uma torre.")]
    public List<UpgradePath> upgradePaths;
    // --- FIM DA ADIÇÃO ---

    [Header("Tower Specifics (Placeholder)")]
    public int cost = 50;
}

// Os Enums (CombatType, FireMode) continuam os mesmos
public enum CombatType { Ranged, Melee }
public enum FireMode { SemiAuto, FullAuto }

// As Structs de Habilidade foram substituídas pelos ScriptableObjects 'Ability' e 'PassiveAbility'