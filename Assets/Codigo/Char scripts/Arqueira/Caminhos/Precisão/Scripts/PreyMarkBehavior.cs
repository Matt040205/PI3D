using UnityEngine;

public class PreyMarkBehavior : TowerBehavior
{
    public float damageBonusToMarked = 0.3f;
    public float markDuration = 10f; // Exemplo de duração da marca

    public override void Initialize(TowerController owner)
    {
        base.Initialize(owner);
        // Suponha que a torre ataca inimigos e que essa lógica de marcação é tratada
        // no momento do ataque.
        if (towerController != null)
        {
            towerController.OnTargetDamaged += HandleTowerAttack;
        }
    }

    private void HandleTowerAttack(EnemyHealthSystem target)
    {
        if (target != null)
        {
            // Lógica para aplicar a marca no inimigo.
            // Exemplo: target.ApplyMark(damageBonusToMarked, markDuration);
            Debug.Log("Inimigo " + target.name + " marcado. Receberá " + (damageBonusToMarked * 100) + "% de dano extra de todas as torres.");
        }
    }

    private void OnDestroy()
    {
        if (towerController != null)
        {
            towerController.OnTargetDamaged -= HandleTowerAttack;
        }
    }
}