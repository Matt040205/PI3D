using UnityEngine;

[CreateAssetMenu(fileName = "Trilha Sobrevivencia da Ca�adora", menuName = "ExoBeasts/Trilhas/Sobreviv�ncia da Ca�adora")]
public class TrilhaSobrevivenciaCacadora : UpgradePath
{
    [Header("Configura��es da Trilha")]
    [Tooltip("Porcentagem de vampirismo (cura por dano)")]
    [Range(0, 1)]
    public float vampirismPercent = 0.1f; // 10%

    // A l�gica de vampirismo deve ser implementada no sistema de ataque do personagem
}