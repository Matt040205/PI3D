using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Lâmina Cortante", menuName = "ExoBeasts/Habilidades/Lâmina Cortante")]
public class CuttingBladeAbility : Ability
{
    [Header("Ingredientes da Lâmina")]
    public float dashDistance = 7f;
    public float damage = 60f;

    private int playerLayer;
    private int dashingLayer;

    public override void Initialize()
    {
        playerLayer = LayerMask.NameToLayer("Player");
        dashingLayer = LayerMask.NameToLayer("PlayerEmDash");
    }

    public override bool Activate(GameObject quemUsou)
    {
        CharacterController controller = quemUsou.GetComponent<CharacterController>();
        Transform modelPivot = quemUsou.GetComponent<PlayerMovement>().GetModelPivot();

        if (controller == null || modelPivot == null) return true;

        if (dashingLayer == -1)
        {
            Debug.LogError("A camada 'PlayerEmDash' não foi encontrada! Por favor, configure em Edit > Project Settings > Physics.");
            Initialize();
            return true;
        }

        int oldLayer = quemUsou.layer;
        quemUsou.layer = dashingLayer;

        controller.Move(modelPivot.forward * dashDistance);

        quemUsou.layer = oldLayer;

        Collider[] inimigosAcertados = Physics.OverlapSphere(quemUsou.transform.position, 2f);
        bool matouAlguem = false;

        foreach (var inimigo in inimigosAcertados)
        {
            EnemyHealthSystem vidaInimigo = inimigo.GetComponent<EnemyHealthSystem>();
            if (vidaInimigo != null)
            {
                bool inimigoMorreu = vidaInimigo.TakeDamage(damage);
                if (inimigoMorreu)
                {
                    matouAlguem = true;
                    Debug.Log("Inimigo abatido! Habilidade resetada.");
                }
            }
        }

        if (matouAlguem)
        {
            return false;
        }

        return true;
    }
}