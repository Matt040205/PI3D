using UnityEngine;

[CreateAssetMenu(fileName = "Voo Gracioso", menuName = "ExoBeasts/Habilidades/Voo Gracioso")]
public class HabilidadeVooGracioso : Ability
{
    [Header("Configurações da Habilidade")]
    public float jumpHeightModifier = 1.5f; // Modificador de pulo para o terceiro pulo
    public float staticAimDuration = 3f; // Duração para ficar estático no ar
    public float bonusDamageMultiplier = 1.5f; // Multiplicador de dano para a próxima flecha
    public float bonusAreaMultiplier = 1.2f; // Multiplicador de área de efeito

    public override bool Activate(GameObject quemUsou)
    {
        // Lógica para o pulo duplo e o terceiro pulo.
        // O pulo extra deve ser tratado no script de movimento do personagem.

        // Lógica para ficar estático no ar e aumentar o dano.
        // Isso pode envolver uma coroutine ou um temporizador no script do jogador.
        Debug.Log("Voo Gracioso ativado. Próxima flecha causa mais dano e área.");
        return true;
    }
}