using UnityEngine;

public class OwlEyeBehavior : TowerBehavior
{
    public float revealDuration = 5f;

    public override void Initialize(TowerController owner)
    {
        base.Initialize(owner);
        if (towerController != null)
        {
            // Se inscreve no evento existente OnCriticalHit, já que não existe um OnHeadshot
            towerController.OnCriticalHit += HandleCriticalHit;
        }
    }

    private void HandleCriticalHit(EnemyHealthSystem target)
    {
        if (target != null)
        {
            // Lógica para revelar o inimigo.
            // Exemplo: target.Reveal(revealDuration);
            Debug.Log("Acerto crítico! Inimigo " + target.name + " revelado por " + revealDuration + " segundos.");
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