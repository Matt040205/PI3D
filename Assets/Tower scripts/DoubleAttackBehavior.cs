using UnityEngine;

public class DoubleAttackBehavior : TowerBehavior
{
    private float chance = 0.25f; // 25% de chance de atacar novamente

    /// <summary>
    /// Sobrescrevemos o método Initialize para nos "inscrevermos" no evento de ataque da torre.
    /// </summary>
    public override void Initialize(TowerController owner)
    {
        base.Initialize(owner);
        if (towerController != null)
        {
            // Diz para a torre: "Quando você atacar, me avise chamando meu método HandleAttack."
            towerController.OnAttack += HandleAttack;
        }
    }

    /// <summary>
    /// Este método é chamado toda vez que a torre ataca.
    /// </summary>
    private void HandleAttack(Transform target)
    {
        // Roda a chance de atacar novamente
        if (Random.value <= chance)
        {
            Debug.Log("CORTE DUPLO ATIVADO!");
            // Pede para a torre atacar o mesmo alvo novamente.
            // towerController.PerformAttack(target); // Descomente quando o método de ataque existir
        }
    }

    /// <summary>
    /// Método chamado automaticamente quando o componente é destruído.
    /// É MUITO importante se "desinscrever" do evento para evitar erros.
    /// </summary>
    private void OnDestroy()
    {
        if (towerController != null)
        {
            towerController.OnAttack -= HandleAttack;
        }
    }
}