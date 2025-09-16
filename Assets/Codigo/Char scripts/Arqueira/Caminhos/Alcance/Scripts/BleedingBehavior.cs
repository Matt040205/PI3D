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
            // Assume que existe um evento para acertos cr�ticos
            towerController.OnCriticalHit += HandleCriticalHit;
        }
    }

    private void HandleCriticalHit(EnemyHealthSystem target)
    {
        if (target != null)
        {
            // L�gica para aplicar o efeito de sangramento
            // Exemplo: target.ApplyBleed(bleedDamagePerSecond, bleedDuration);
            Debug.Log("Acerto cr�tico! Inimigo " + target.name + " est� sangrando por " + bleedDuration + " segundos.");
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