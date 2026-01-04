using UnityEngine;

public class BleedingBehavior : TowerBehavior
{
    public float bleedDuration = 3f;
    public float bleedDamagePerSecond = 5f;

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
        if (target != null)
        {
            // Lógica para aplicar o efeito de sangramento
            // Exemplo: target.ApplyBleed(bleedDamagePerSecond, bleedDuration);
            Debug.Log("Acerto crítico! Inimigo " + target.name + " está sangrando por " + bleedDuration + " segundos.");
        }
    }

    private void OnDestroy()
    {
        if (towerController != null)
        {
            towerController.OnCriticalHit -= HandleCriticalHit;
        }
    }
}