using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Tower Defense/Character Base")]
public class CharacterBase : ScriptableObject
{
    [Header("Basic Stats")]
    public float maxHealth = 100f;
    public float damage = 10f;
    public float moveSpeed = 5f;
    public float reloadSpeed = 2f;
    public float attackSpeed = 1f;

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