// PeaceOfMindAbility.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Paz de Espírito", menuName = "ExoBeasts/Habilidades/Paz de Espírito")]
public class PeaceOfMindAbility : Ability // Também usa nosso molde 'Ability'
{
    [Header("Ingredientes da Cura")]
    public float totalHeal = 80f;
    public float duration = 3f;

    // O "Modo de Preparo" da cura.
    public override bool Activate(GameObject quemUsou)
    {
        // Esta receita não faz o trabalho sozinha.
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