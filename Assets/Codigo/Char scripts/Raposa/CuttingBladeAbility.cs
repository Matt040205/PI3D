using UnityEngine;
using System.Collections;
using FMODUnity;

[CreateAssetMenu(fileName = "Lâmina Cortante", menuName = "ExoBeasts/Personagens/Raposa/Habilidade/Lâmina Cortante")]
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
        PlayerMovement movementScript = quemUsou.GetComponent<PlayerMovement>();
        CommanderAbilityController abilityController = quemUsou.GetComponent<CommanderAbilityController>();

        if (controller == null || movementScript == null || abilityController == null)
        {
            Debug.LogError("CuttingBladeAbility: Faltam componentes (Controller, PlayerMovement ou AbilityController).");
            return false;
        }

        Transform modelPivot = movementScript.GetModelPivot();
        if (modelPivot == null) return false;

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