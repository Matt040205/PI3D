using UnityEngine;

[CreateAssetMenu(fileName = "Dança das Nove Caudas", menuName = "ExoBeasts/Personagens/Raposa/Habilidade/Dança das Nove Caudas")]
public class NineTailsDanceAbility : Ability
{
    [Header("Ingredientes da Ultimate")]
    public float duration = 8f;
    [Range(0, 1)]
    public float cooldownReductionPercent = 0.4f;

    public override bool Activate(GameObject quemUsou)
    {
        CommanderAbilityController controller = quemUsou.GetComponent<CommanderAbilityController>();
        if (controller == null) return true;

        controller.ReduceAllAbilityCooldowns(cooldownReductionPercent);

        NineTailsDanceLogic ajudante = quemUsou.GetComponent<NineTailsDanceLogic>();
        if (ajudante == null)
        {
            ajudante = quemUsou.AddComponent<NineTailsDanceLogic>();
        }

        ajudante.StartEffect(duration);

        return true;
    }
}