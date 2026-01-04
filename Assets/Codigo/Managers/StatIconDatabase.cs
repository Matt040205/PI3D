using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public struct StatIconEntry
{
    public CharacterStatType stat;
    public Sprite icon;
}

[CreateAssetMenu(fileName = "Novo Banco de Icones", menuName = "ScriptableObjects/Rastros/Base de imagens")]
public class StatIconDatabase : ScriptableObject
{
    [Header("Configuração dos Ícones")]
    public List<StatIconEntry> statIcons;

    // Dicionário para busca rápida (cache)
    private Dictionary<CharacterStatType, Sprite> _iconMap;

    public Sprite GetIconForStat(CharacterStatType statType)
    {
        // Inicializa o dicionário na primeira vez que for chamado
        if (_iconMap == null)
        {
            _iconMap = new Dictionary<CharacterStatType, Sprite>();
            foreach (var entry in statIcons)
            {
                if (entry.icon != null && !_iconMap.ContainsKey(entry.stat))
                {
                    _iconMap.Add(entry.stat, entry.icon);
                }
            }
        }

        // Tenta buscar o ícone
        if (_iconMap.ContainsKey(statType))
        {
            return _iconMap[statType];
        }

        // Se não achar, retorna nulo (ou você pode colocar um sprite padrão aqui)
        return null;
    }
}