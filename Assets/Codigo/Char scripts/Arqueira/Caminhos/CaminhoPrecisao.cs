using UnityEngine;

[CreateAssetMenu(fileName = "Caminho de Precisao", menuName = "ExoBeasts/Torres/Caminho/Precis�o")]
public class CaminhoPrecisao : UpgradePath
{
    [Header("Configura��es da Trilha Precis�o")]
    public float damageBonusNV1 = 0.2f; // +20% Dano
    public bool piercingNV2 = true; // Flechas perfuram 1 inimigo
    public float criticalChanceNV3 = 0.25f; // +25% Chance de Cr�tico
    public float eyeOfOwlDurationNV4 = 5f; // Dura��o de "Olho da Coruja"
    public float bleedingDurationNV5 = 3f; // Dura��o do sangramento

    // A l�gica de cada n�vel deve ser implementada no script da torre.
}