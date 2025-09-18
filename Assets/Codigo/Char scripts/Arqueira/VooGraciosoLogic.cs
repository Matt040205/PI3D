using UnityEngine;

public class VooGraciosoLogic : MonoBehaviour
{
    private GameObject owner;
    private PlayerMovement playerMovement;
    private PlayerShooting playerShooting;

    public void StartEffect(GameObject quemUsou, float jumpHeightModifier, float staticAimDuration, float bonusDamage, float bonusArea)
    {
        owner = quemUsou;
        playerMovement = owner.GetComponent<PlayerMovement>();
        playerShooting = owner.GetComponent<PlayerShooting>();

        // L�gica para o pulo com for�a modificada
        if (playerMovement != null)
        {
            playerMovement.jumpHeightModifier = jumpHeightModifier;
        }

        // L�gica para o tiro b�nus e a flutua��o
        if (playerShooting != null)
        {
            playerShooting.SetNextShotBonus(bonusDamage, bonusArea);

            // Inicia a flutua��o apenas se o jogador estiver no ar
            if (playerMovement != null && !playerMovement.isGrounded)
            {
                playerMovement.isFloating = true;
                playerMovement.floatDuration = staticAimDuration;
            }
        }

        // A l�gica de remo��o dos efeitos ser� tratada em PlayerMovement e PlayerShooting
        Destroy(gameObject);
    }
}