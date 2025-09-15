using UnityEngine;

[CreateAssetMenu(fileName = "Caminho de Alcance", menuName = "ExoBeasts/Torres/Caminho/Alcance")]
public class CaminhoAlcance : UpgradePath
{
    [Header("Configura��es da Trilha Alcance")]
    public float rangeBonusNV1 = 0.15f; // +15% Alcance
    public float projectileSpeedBonusNV2 = 0.2f; // +20% Vel. Proj�til
    public bool attackFlyingEnemiesNV3 = true; // Ataca inimigos voadores
    public float darkVisionBonusNV4 = 0.5f; // Vis�o em �rea escura +50%
    public float markedDamageBonusNV5 = 0.3f; // Inimigos marcados recebem +30% dano

    // A l�gica de cada n�vel deve ser implementada no script da torre.
}