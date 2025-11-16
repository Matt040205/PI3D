using UnityEngine;
using System.Collections;
using FMODUnity;

[CreateAssetMenu(fileName = "Lâmina Cortante", menuName = "ExoBeasts/Habilidades/Lâmina Cortante")]
public class CuttingBladeAbility : Ability
{
    [Header("Ingredientes da Lâmina")]
    public float dashDistance = 7f;
    public float damage = 60f;
    public bool resetCooldownOnKill = true;

    [Header("FMOD")]
    [EventRef]
    public string eventoDash = "event:/SFX/Dash";

    public override void Initialize()
    {
    }

    public override bool Activate(GameObject quemUsou)
    {
        CharacterController controller = quemUsou.GetComponent<CharacterController>();
        Transform modelPivot = quemUsou.GetComponent<PlayerMovement>().GetModelPivot();
        CommanderAbilityController abilityController = quemUsou.GetComponent<CommanderAbilityController>();

        if (controller == null || modelPivot == null || abilityController == null)
        {
            Debug.LogError("CuttingBladeAbility: Faltam componentes (Controller, Pivot ou AbilityController).");
            return true;
        }

        CuttingBladeLogic logic = quemUsou.AddComponent<CuttingBladeLogic>();
        logic.StartDash(
            quemUsou,
            controller,
            modelPivot,
            dashDistance,
            damage,
            eventoDash,
            abilityController,
            this,
            resetCooldownOnKill
        );

        return true;
    }
}