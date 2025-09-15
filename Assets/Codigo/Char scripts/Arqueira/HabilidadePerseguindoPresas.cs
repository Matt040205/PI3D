using UnityEngine;

[CreateAssetMenu(fileName = "Perseguindo as Presas", menuName = "ExoBeasts/Habilidades/Perseguindo as Presas")]
public class HabilidadePerseguindoPresas : Ability
{
    [Header("Configurações da Habilidade")]
    public float markDuration = 5f; // Duração da marca nos inimigos
    public float bonusDamageMultiplier = 1.25f; // Aumento de dano

    public override bool Activate(GameObject quemUsou)
    {
        // Lógica para marcar inimigos na tela do jogador.
        // Aumentar dano contra inimigos marcados.
        Debug.Log("Perseguindo as Presas ativado. Inimigos marcados e recebem mais dano.");
        return true;
    }
}