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

    [Header("Combat Settings")]
    public CombatType combatType = CombatType.Ranged;
    public FireMode fireMode = FireMode.SemiAuto;
    public float meleeRange = 2f;
    public float meleeAngle = 90f;

    [Header("Type Settings")]
    public bool isCommander = true;
    public Sprite characterIcon;

    [Header("Commander Specifics")]
    public int magazineSize = 10;
    public PassiveAbility passive;
    public ActiveAbility ability1;
    public ActiveAbility ability2;
    public ActiveAbility ultimate;

    [Header("Tower Specifics (Placeholder)")]
    public int cost = 50;
}

public enum CombatType
{
    Ranged,
    Melee
}

public enum FireMode
{
    SemiAuto,   // Disparo único por clique
    FullAuto    // Disparo contínuo enquanto o botão estiver pressionado
}

[System.Serializable]
public struct PassiveAbility
{
    public string abilityName;
    [TextArea] public string description;
    public Sprite icon;
}

[System.Serializable]
public struct ActiveAbility
{
    public string abilityName;
    [TextArea] public string description;
    public Sprite icon;
    public float cooldown;
}