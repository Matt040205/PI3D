using UnityEngine;

[CreateAssetMenu(fileName = "Passiva de Comandante da Coruja", menuName = "ExoBeasts/Personagens/Coruja/Passiva/Passiva de Comandante")]
public class PassivaComandanteCoruja : PassivaAbility
{
    [Header("Configurações da Passiva")]
    [Tooltip("Aumento de dano para todas as torres em %")]
    [Range(0, 1)]
    public float bonusDamagePercent = 0.2f; // 20%
    [Tooltip("Permite pulo duplo para o personagem")]
    public bool canDoubleJump = true;

    public BuildManager buildManager;

    public override void OnEquip(GameObject owner)
    {
        Debug.Log("Passiva de Comandante da Coruja equipada: Dano das torres aumentado e pulo duplo ativado.");

        if (buildManager != null)
        {
            // buildManager.ApplyDamageBonusToAllTowers(bonusDamagePercent);
        }

        PlayerMovement playerMovement = owner.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.canDoubleJump = canDoubleJump;
        }
    }

    public override void OnUnequip(GameObject owner)
    {
        Debug.Log("Passiva de Comandante da Coruja desequipada: Efeitos removidos.");

        if (buildManager != null)
        {
            //buildManager.RemoveDamageBonusFromAllTowers(bonusDamagePercent);
        }

        PlayerMovement playerMovement = owner.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.canDoubleJump = false;
        }
    }
}