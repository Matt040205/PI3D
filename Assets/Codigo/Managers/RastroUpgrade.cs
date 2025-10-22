// Arquivo: RastroUpgrade.cs (Corrigido)
using UnityEngine;
using System.Collections.Generic;

// Enum com os status do PERSONAGEM. 
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

// O enum 'ModificationType' FOI REMOVIDO DAQUI.
// Vamos usar o que já existe no seu script "Upgrade.cs".

[System.Serializable]
public struct CharacterStatModifier
{
    public CharacterStatType statToModify;

    // Ele vai encontrar o 'ModificationType' do seu outro script
    public ModificationType modType;

    public float value;
}


[CreateAssetMenu(fileName = "Novo Upgrade de Rastro", menuName = "ExoBeasts/Personagem/Rastro Upgrade")]
public class RastroUpgrade : ScriptableObject
{
    [Header("Informações do Upgrade")]
    public string upgradeName;
    [TextArea] public string description;

    [Header("Modificadores de Status")]
    [Tooltip("Lista de status que este upgrade vai modificar no CharacterBase")]
    public List<CharacterStatModifier> modifiers;
}