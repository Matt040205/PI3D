using UnityEngine;

[CreateAssetMenu(fileName = "Voo Gracioso", menuName = "ExoBeasts/Habilidades/Voo Gracioso")]
public class HabilidadeVooGracioso : Ability
{
    [Header("Configura��es da Habilidade")]
    public float jumpHeightModifier = 1.5f; // Modificador de pulo para o terceiro pulo
    public float staticAimDuration = 3f; // Dura��o para ficar est�tico no ar
    public float bonusDamageMultiplier = 1.5f; // Multiplicador de dano para a pr�xima flecha
    public float bonusAreaMultiplier = 1.2f; // Multiplicador de �rea de efeito

    public override bool Activate(GameObject quemUsou)
    {
        // L�gica para o pulo duplo e o terceiro pulo.
        // O pulo extra deve ser tratado no script de movimento do personagem.

        // L�gica para ficar est�tico no ar e aumentar o dano.
        // Isso pode envolver uma coroutine ou um temporizador no script do jogador.
        Debug.Log("Voo Gracioso ativado. Pr�xima flecha causa mais dano e �rea.");
        return true;
    }
}