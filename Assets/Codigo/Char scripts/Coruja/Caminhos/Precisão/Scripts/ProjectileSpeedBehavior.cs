using UnityEngine;

public class ProjectileSpeedBehavior : TowerBehavior
{
    public float projectileSpeedBonus = 0.2f;

    public override void Initialize(TowerController owner)
    {
        base.Initialize(owner);
        // Lógica para aumentar a velocidade do projétil.
        // Exemplo: towerController.AddProjectileSpeedBonus(projectileSpeedBonus);
        Debug.Log("ProjectileSpeedBehavior ativado. Velocidade do projétil aumentada em " + (projectileSpeedBonus * 100) + "% .");
    }

    private void OnDestroy()
    {
        // Lógica para remover o bônus de velocidade do projétil.
    }
}