using UnityEngine;

[CreateAssetMenu(fileName = "Caminho de Precisao", menuName = "ExoBeasts/Torres/Caminho/Precisão")]
public class CaminhoPrecisao : UpgradePath
{
    [Header("Configurações da Trilha Precisão")]
    public float damageBonusNV1 = 0.2f; // +20% Dano
    public bool piercingNV2 = true; // Flechas perfuram 1 inimigo
    public float criticalChanceNV3 = 0.25f; // +25% Chance de Crítico
    public float eyeOfOwlDurationNV4 = 5f; // Duração de "Olho da Coruja"
    public float bleedingDurationNV5 = 3f; // Duração do sangramento

    // A lógica de cada nível deve ser implementada no script da torre.
}