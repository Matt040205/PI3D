using UnityEngine;

public class VooGraciosoLogic : MonoBehaviour
{
    private GameObject owner;
    private PlayerMovement playerMovement;
    private PlayerShooting playerShooting;
    private CommanderAbilityController abilityController;
    private Ability sourceAbility;
    private bool isActive = false;

    public void StartEffect(GameObject quemUsou, float jumpHeightModifier, float staticAimDuration, float bonusDamage, float bonusRadius, CommanderAbilityController controller, Ability ability)
    {
        owner = quemUsou;
        playerMovement = owner.GetComponent<PlayerMovement>();
        playerShooting = owner.GetComponent<PlayerShooting>();
        abilityController = controller;
        sourceAbility = ability;

        if (abilityController != null)
        {
            abilityController.SetAbilityUsage(sourceAbility, true);
        }

        if (playerMovement != null)
        {
            playerMovement.jumpHeightModifier = jumpHeightModifier;
        }

        if (playerShooting != null)
        {
            playerShooting.SetNextShotBonus(bonusDamage, bonusRadius);

            if (playerMovement != null && !playerMovement.isGrounded)
            {
                playerMovement.isFloating = true;
                playerMovement.floatDuration = staticAimDuration;
            }
        }

        isActive = true;
    }

    void Update()
    {
        if (!isActive || playerMovement == null) return;

        if (!playerMovement.isGrounded)
        {
            if (abilityController != null && sourceAbility != null)
            {
                abilityController.SetAbilityUsage(sourceAbility, true);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}