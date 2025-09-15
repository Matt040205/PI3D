using UnityEngine;

[CreateAssetMenu(fileName = "Passiva de Comandante da Coruja", menuName = "ExoBeasts/Passivas/Passiva de Comandante")]
public class PassivaComandanteCoruja : PassivaAbility
{
    [Header("Configurações da Passiva")]
    [Tooltip("Aumento de dano para todas as torres em %")]
    [Range(0, 1)]
    public float bonusDamagePercent = 0.2f; // 20%
    [Tooltip("Permite pulo duplo para o personagem")]
    public bool canDoubleJump = true;

    // A lógica de aplicação da passiva seria no script do CommanderController

    public override void OnEquip(GameObject owner)
    {
        // Lógica para aplicar os efeitos da passiva no jogador ou no jogo.
        // Exemplo: notificar um gerenciador para aumentar o dano das torres
        // e habilitar o pulo duplo no script de movimento do personagem.
        Debug.Log("Passiva de Comandante da Coruja equipada: Dano das torres aumentado e pulo duplo ativado.");
    }

    public override void OnUnequip(GameObject owner)
    {
        // Lógica para remover os efeitos da passiva.
        // Exemplo: notificar o gerenciador para remover o bônus de dano das torres
        // e desabilitar o pulo duplo.
        Debug.Log("Passiva de Comandante da Coruja desequipada: Efeitos removidos.");
    }
}