using UnityEngine;

[CreateAssetMenu(fileName = "Caminho de Velocidade de Ataque", menuName = "ExoBeasts/Torres/Caminho/Velocidade de Ataque")]
public class CaminhoVelocidadeAtaque : UpgradePath
{
    [Header("Configura��es da Trilha Velocidade de Ataque")]
    public float attackSpeedBonusNV1 = 0.1f; // +10% de velocidade de ataque
    public float doubleShotChanceNV2 = 0.15f; // 15% chance de disparar 2 flechas
    public int freeArrowsOnKillNV3 = 3; // Dispara 3 flechas gr�tis ao matar inimigo
    public float reloadSpeedBonusNV4 = 0.3f; // +30% Vel. Recarga por 4s ap�s cr�tico
    public float furyAttackSpeedBonusNV5 = 0.05f; // 5% de vel. ataque por ac�mulo
    public int furyMaxStacksNV5 = 8; // Acumula at� 8x

    // A l�gica de cada n�vel deve ser implementada no script da torre.
}