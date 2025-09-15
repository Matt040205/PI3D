// CharacterBase.cs
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
    public float armor = 0f;

    [Tooltip("Chance de causar um acerto cr�tico, de 0.0 a 1.0 (ex: 0.1 = 10%).")]
    [Range(0f, 1f)]
    public float critChance = 0.05f; // 5% de chance base

    [Tooltip("Multiplicador de dano em um acerto cr�tico (ex: 1.5 = 150% do dano normal).")]
    public float critDamage = 1.5f; // 150% de dano base

    // --- ADI��O AQUI ---
    [Tooltip("Quanto da armadura do inimigo � ignorada, de 0.0 a 1.0 (ex: 0.5 = 50%).")]
    [Range(0f, 1f)]
    public float armorPenetration = 0f; // Come�a com 0%

    [Header("Combat Settings")]
    public CombatType combatType = CombatType.Ranged;
    public FireMode fireMode = FireMode.SemiAuto;
    public float meleeAngle = 90f;

    [Header("Type Settings")]
    public bool isCommander = true;
    public Sprite characterIcon;
    public GameObject commanderPrefab;
    public GameObject towerPrefab;

    [Header("Commander Specifics")]
    public int magazineSize = 10;
    public PassivaAbility passive;
    public Ability ability1;
    public Ability ability2;
    public Ability ultimate;

    [Header("Tower Upgrades")]
    [Tooltip("A lista de caminhos de upgrade dispon�veis quando este personagem � uma torre.")]
    public List<UpgradePath> upgradePaths;

    [Header("Tower Specifics (Placeholder)")]
    public int cost = 50;
}

public enum CombatType { Ranged, Melee }
public enum FireMode { SemiAuto, FullAuto }