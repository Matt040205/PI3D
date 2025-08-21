// NineTailsDanceAbility.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Dança das Nove Caudas", menuName = "ExoBeasts/Habilidades/Dança das Nove Caudas")]
public class NineTailsDanceAbility : Ability
{
    [Header("Ingredientes da Ultimate")]
    public float duration = 8f;
    [Range(0, 1)]
    public float cooldownReductionPercent = 0.4f; // 40%

    public override bool Activate(GameObject quemUsou)
    {
        CommanderAbilityController assistente = quemUsou.GetComponent<CommanderAbilityController>();
        if (assistente != null)
        {
            // Dá a ordem para o assistente iniciar o estado de fúria
            assistente.StartUltimate(duration, cooldownReductionPercent);
        }
        return true; // Sempre entra em cooldown após o uso.
    }
}