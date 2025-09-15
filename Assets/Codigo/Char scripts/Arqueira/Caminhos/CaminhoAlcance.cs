using UnityEngine;

[CreateAssetMenu(fileName = "Caminho de Alcance", menuName = "ExoBeasts/Torres/Caminho/Alcance")]
public class CaminhoAlcance : UpgradePath
{
    [Header("Configurações da Trilha Alcance")]
    public float rangeBonusNV1 = 0.15f; // +15% Alcance
    public float projectileSpeedBonusNV2 = 0.2f; // +20% Vel. Projétil
    public bool attackFlyingEnemiesNV3 = true; // Ataca inimigos voadores
    public float darkVisionBonusNV4 = 0.5f; // Visão em área escura +50%
    public float markedDamageBonusNV5 = 0.3f; // Inimigos marcados recebem +30% dano

    // A lógica de cada nível deve ser implementada no script da torre.
}