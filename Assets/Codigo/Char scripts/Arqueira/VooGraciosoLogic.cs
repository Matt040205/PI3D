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

        // Lógica para o pulo com força modificada
        if (playerMovement != null)
        {
            playerMovement.jumpHeightModifier = jumpHeightModifier;
        }

        // Lógica para o tiro bônus e a flutuação
        if (playerShooting != null)
        {
            playerShooting.SetNextShotBonus(bonusDamage, bonusArea);

            // Inicia a flutuação apenas se o jogador estiver no ar
            if (playerMovement != null && !playerMovement.isGrounded)
            {
                playerMovement.isFloating = true;
                playerMovement.floatDuration = staticAimDuration;
            }
        }

        // A lógica de remoção dos efeitos será tratada em PlayerMovement e PlayerShooting
        Destroy(gameObject);
    }
}