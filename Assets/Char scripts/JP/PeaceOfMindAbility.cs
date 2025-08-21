// PeaceOfMindAbility.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Paz de Esp�rito", menuName = "ExoBeasts/Habilidades/Paz de Esp�rito")]
public class PeaceOfMindAbility : Ability // Tamb�m usa nosso molde 'Ability'
{
    [Header("Ingredientes da Cura")]
    public float totalHeal = 80f;
    public float duration = 3f;

    // O "Modo de Preparo" da cura.
    public override bool Activate(GameObject quemUsou)
    {
        // Esta receita n�o faz o trabalho sozinha.
        // Ela precisa de um "assistente" no personagem para controlar a cura ao longo do tempo.
        CommanderAbilityController assistente = quemUsou.GetComponent<CommanderAbilityController>();

        if (assistente != null)
        {
            // A receita simplesmente diz para o assistente: "Comece a curar com estes ingredientes".
            assistente.StartHealOverTime(totalHeal, duration);
        }

        // Esta habilidade SEMPRE entra em cooldown depois de usada.
        return true;
    }
}