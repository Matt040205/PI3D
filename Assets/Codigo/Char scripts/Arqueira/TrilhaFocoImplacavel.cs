using UnityEngine;

[CreateAssetMenu(fileName = "Trilha Foco Implacavel", menuName = "ExoBeasts/Trilhas/Foco Implac�vel")]
public class TrilhaFocoImplacavel : UpgradePath
{
    [Header("Configura��es da Trilha")]
    [Tooltip("Redu��o de cooldown em % por headshot")]
    [Range(0, 1)]
    public float cooldownReductionPercent = 0.3f; // 30%

    // A l�gica de redu��o de cooldown deve ser implementada no sistema de combate do personagem.
}