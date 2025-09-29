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

    [Tooltip("Chance de causar um acerto crítico, de 0.0 a 1.0 (ex: 0.1 = 10%).")]
    [Range(0f, 1f)]
    public float critChance = 0.05f; // 5% de chance base

    [Tooltip("Multiplicador de dano em um acerto crítico (ex: 1.5 = 150% do dano normal).")]
    public float critDamage = 1.5f; // 150% de dano base

    // --- ADIÇÃO AQUI ---
    [Tooltip("Quanto da armadura do inimigo é ignorada, de 0.0 a 1.0 (ex: 0.5 = 50%).")]
    [Range(0f, 1f)]
    public float armorPenetration = 0f; // Começa com 0%

    [Header("Ultimate Settings")] // <<< NOVO HEADER ADICIONADO AQUI
    [Tooltip("A quantidade de carga da Ultimate (em percentual de 0.0 a 1.0) ganha por 1 ponto de dano causado.")]
    [Range(0f, 0.1f)]
    public float ultimateChargePerDamage = 0.001f; // Exemplo: 0.001f = 0.1% de carga por 1 de dano

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
    [Tooltip("A lista de caminhos de upgrade disponíveis quando este personagem é uma torre.")]
    public List<UpgradePath> upgradePaths;

    [Header("Tower Specifics (Placeholder)")]
    public int cost = 50;
}

public enum CombatType { Ranged, Melee }
public enum FireMode { SemiAuto, FullAuto }