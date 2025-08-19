using UnityEngine;

[System.Serializable]
public class UpgradeLevel
{
    [Header("Level Info")]
    public string levelName = "Level 1";
    [Tooltip("Custo em pontos/moeda para desbloquear este nível.")]
    public int cost = 100;

    [TextArea] public string flavorText;

    [Header("Effects Applied")]
    [Tooltip("Lista de efeitos aplicados quando este nível é comprado.")]
    public UpgradeEffectSO[] effects;
}

[CreateAssetMenu(fileName = "UpgradePath", menuName = "ExoBeats/Upgrade Path")]
public class UpgradePathSO : ScriptableObject
{
    [Header("Path Info")]
    public string pathName = "New Path";
    [TextArea] public string description;
    public Sprite icon;

    [Header("Levels")]
    public UpgradeLevel[] levels;

    // --- Métodos utilitários ---
    public int LevelsCount => levels != null ? levels.Length : 0;

    public bool HasLevels => levels != null && levels.Length > 0;

    public UpgradeLevel GetLevel(int index)
    {
        if (!HasLevels || index < 0 || index >= levels.Length)
            return null;
        return levels[index];
    }
}