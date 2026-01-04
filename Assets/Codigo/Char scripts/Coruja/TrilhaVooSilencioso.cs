using UnityEngine;

[CreateAssetMenu(fileName = "Trilha Voo Silencioso", menuName = "ExoBeasts/Personagens/Coruja/Trilha/Voo Silencioso")]
public class TrilhaVooSilencioso : UpgradePath
{
    [Header("Configurações da Trilha")]
    public float invisibilityDuration = 1.5f; // Duração da invisibilidade
    // A lógica de invisibilidade deve ser implementada no script de "Voo Gracioso"
}