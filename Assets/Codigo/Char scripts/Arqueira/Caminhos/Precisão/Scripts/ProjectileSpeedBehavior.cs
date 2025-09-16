using UnityEngine;

public class ProjectileSpeedBehavior : TowerBehavior
{
    public float projectileSpeedBonus = 0.2f;

    public override void Initialize(TowerController owner)
    {
        base.Initialize(owner);
        // L�gica para aumentar a velocidade do proj�til.
        // Exemplo: towerController.AddProjectileSpeedBonus(projectileSpeedBonus);
        Debug.Log("ProjectileSpeedBehavior ativado. Velocidade do proj�til aumentada em " + (projectileSpeedBonus * 100) + "% .");
    }

    private void OnDestroy()
    {
        // L�gica para remover o b�nus de velocidade do proj�til.
    }
}