// NineTailsDanceAbility.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Dan�a das Nove Caudas", menuName = "ExoBeasts/Habilidades/Dan�a das Nove Caudas")]
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
            // D� a ordem para o assistente iniciar o estado de f�ria
            assistente.StartUltimate(duration, cooldownReductionPercent);
        }
        return true; // Sempre entra em cooldown ap�s o uso.
    }
}