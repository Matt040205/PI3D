using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CaminhoRastrosData
{
    public string idCaminho;
    public int pontosGastos;
}

[CreateAssetMenu(fileName = "CharacterData", menuName = "Tower Defense/Character Base")]
public class CharacterBase : ScriptableObject
{
    [Header("Basic Info")]
    public new string name;
    [TextArea(3, 5)]
    public string description = "Descrição do personagem ou torre.";

    [Header("Basic Stats")]
    public float maxHealth = 100f;
    public float damage = 10f;
    public float moveSpeed = 5f;
    public float reloadSpeed = 2f;
    public float attackSpeed = 1f;
    public float meleeRange = 2f;
    public float armor = 0f;

    [Range(0f, 1f)]
    public float critChance = 0.05f;
    public float critDamage = 1.5f;
    [Range(0f, 1f)]
    public float armorPenetration = 0f;

    [Header("Ultimate Settings")]
    [Tooltip("Pontos de Ultimate ganhos por segundo, passivamente.")]
    public float ultimateChargePerSecond = 1f;
    [Tooltip("Pontos de Ultimate ganhos por cada 1 ponto de dano causado.")]
    public float ultimateChargePerDamage = 0.1f;

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
    public List<UpgradePath> upgradePaths;

    [Header("Tower Specifics (Placeholder)")]
    public int cost = 50;

    [Header("Rastros Progress")]
    public int pontosRastrosDisponiveis = 10;
    public int pontosRastrosGastos = 0;
    public List<CaminhoRastrosData> pontosPorCaminho = new List<CaminhoRastrosData>();
    public List<string> habilidadesDesbloqueadas = new List<string>();

    public void ResetarRastros()
    {
        pontosRastrosDisponiveis = 10;
        pontosRastrosGastos = 0;
        pontosPorCaminho.Clear();
        habilidadesDesbloqueadas.Clear();
    }
}

public enum CombatType { Ranged, Melee }
public enum FireMode { SemiAuto, FullAuto }