using UnityEngine;

public class ReloadSpeedBehavior : TowerBehavior
{
    public float reloadSpeedBonus = 0.3f;
    public float bonusDuration = 4f;

    public override void Initialize(TowerController owner)
    {
        base.Initialize(owner);
        if (towerController != null)
        {
            // Assume que existe um evento para acertos críticos
            towerController.OnCriticalHit += HandleCriticalHit;
        }
    }

    private void HandleCriticalHit(EnemyHealthSystem target)
    {
        // Lógica para aplicar o buff de velocidade de recarga
        // Exemplo: towerController.ApplyReloadSpeedBuff(reloadSpeedBonus, bonusDuration);
        Debug.Log("Acerto crítico! Velocidade de recarga aumentada em " + (reloadSpeedBonus * 100) + "% por " + bonusDuration + " segundos.");
    }

    private void OnDestroy()
    {
        if (towerController != null)
        {
            towerController.OnCriticalHit -= HandleCriticalHit;
        }
    }
}