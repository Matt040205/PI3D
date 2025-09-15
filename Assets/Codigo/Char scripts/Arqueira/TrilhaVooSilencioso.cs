using UnityEngine;

[CreateAssetMenu(fileName = "Trilha Voo Silencioso", menuName = "ExoBeasts/Trilhas/Voo Silencioso")]
public class TrilhaVooSilencioso : UpgradePath
{
    [Header("Configura��es da Trilha")]
    public float invisibilityDuration = 1.5f; // Dura��o da invisibilidade

    // A l�gica de invisibilidade deve ser implementada no script de "Voo Gracioso"
}