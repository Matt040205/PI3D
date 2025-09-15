using UnityEngine;

[CreateAssetMenu(fileName = "Trilha Sobrevivencia da Caçadora", menuName = "ExoBeasts/Trilhas/Sobrevivência da Caçadora")]
public class TrilhaSobrevivenciaCacadora : UpgradePath
{
    [Header("Configurações da Trilha")]
    [Tooltip("Porcentagem de vampirismo (cura por dano)")]
    [Range(0, 1)]
    public float vampirismPercent = 0.1f; // 10%

    // A lógica de vampirismo deve ser implementada no sistema de ataque do personagem
}