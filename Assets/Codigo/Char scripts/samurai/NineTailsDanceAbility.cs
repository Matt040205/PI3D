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
        // Pega o controlador para usar suas ferramentas
        CommanderAbilityController controller = quemUsou.GetComponent<CommanderAbilityController>();
        if (controller == null) return true; // Se não encontrar, não faz nada mas entra em cooldown

        // --- EFEITOS INSTANTÂNEOS ---
        // 1. Pede para o controlador reduzir os cooldowns
        controller.ReduceAllAbilityCooldowns(cooldownReductionPercent);

        // --- EFEITOS DE DURAÇÃO ---
        // 2. Cria o "ajudante" para cuidar do resto
        NineTailsDanceLogic ajudante = quemUsou.AddComponent<NineTailsDanceLogic>();

        // 3. Manda o ajudante começar a trabalhar, passando a duração
        ajudante.StartEffect(duration);

        // A ultimate sempre entra em cooldown
        return true;
    }
}