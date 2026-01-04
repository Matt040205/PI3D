using UnityEngine;

public class VooGraciosoLogic : MonoBehaviour
{
    private GameObject owner;
    private PlayerMovement playerMovement;
    private PlayerShooting playerShooting;

    public void StartEffect(GameObject quemUsou, float jumpHeightModifier, float staticAimDuration, float bonusDamage, float bonusRadius)
    {
        owner = quemUsou;
        playerMovement = owner.GetComponent<PlayerMovement>();
        playerShooting = owner.GetComponent<PlayerShooting>();

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

        Destroy(gameObject);
    }
}