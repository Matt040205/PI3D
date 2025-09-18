using UnityEngine;

[CreateAssetMenu(fileName = "Passiva de Comandante da Coruja", menuName = "ExoBeasts/Passivas/Passiva de Comandante")]
public class PassivaComandanteCoruja : PassivaAbility
{
    [Header("Configurações da Passiva")]
    [Tooltip("Aumento de dano para todas as torres em %")]
    [Range(0, 1)]
    public float bonusDamagePercent = 0.2f; // 20%
    [Tooltip("Permite pulo duplo para o personagem")]
    public bool canDoubleJump = true;

    // NOVO: Adiciona um campo para o BuildManager para que a passiva possa acessá-lo.
    // Isso deve ser atribuído pelo CommanderController ou GameManager.
    public BuildManager buildManager;

    public override void OnEquip(GameObject owner)
    {
        Debug.Log("Passiva de Comandante da Coruja equipada: Dano das torres aumentado e pulo duplo ativado.");

        // 1. Aplica o bônus de dano a todas as torres
        if (buildManager != null)
        {
           // buildManager.ApplyDamageBonusToAllTowers(bonusDamagePercent);
        }

        // 2. Habilita o pulo duplo no script de movimento do jogador
        PlayerMovement playerMovement = owner.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.canDoubleJump = canDoubleJump;
        }
    }

    public override void OnUnequip(GameObject owner)
    {
        Debug.Log("Passiva de Comandante da Coruja desequipada: Efeitos removidos.");

        // 1. Remove o bônus de dano de todas as torres
        if (buildManager != null)
        {
            //buildManager.RemoveDamageBonusFromAllTowers(bonusDamagePercent);
        }

        // 2. Desabilita o pulo duplo
        PlayerMovement playerMovement = owner.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.canDoubleJump = false;
        }
    }
}