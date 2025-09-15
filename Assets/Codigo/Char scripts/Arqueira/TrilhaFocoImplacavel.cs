using UnityEngine;

[CreateAssetMenu(fileName = "Trilha Foco Implacavel", menuName = "ExoBeasts/Trilhas/Foco Implacável")]
public class TrilhaFocoImplacavel : UpgradePath
{
    [Header("Configurações da Trilha")]
    [Tooltip("Redução de cooldown em % por headshot")]
    [Range(0, 1)]
    public float cooldownReductionPercent = 0.3f; // 30%

    // A lógica de redução de cooldown deve ser implementada no sistema de combate do personagem.
}