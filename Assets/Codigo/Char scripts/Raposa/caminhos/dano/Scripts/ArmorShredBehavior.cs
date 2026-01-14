// ArmorShredBehavior.cs
using UnityEngine;
public class ArmorShredBehavior : TowerBehavior
{
    public float armorReductionPercentage = 0.05f;
    public int maxStacks = 3;

    public override void Initialize(TowerController owner)
    {
        base.Initialize(owner);
        if (towerController != null) towerController.OnTargetDamaged += HandleTowerAttack;
    }

    private void HandleTowerAttack(EnemyHealthSystem target)
    {
        if (target != null) target.ApplyArmorShred(armorReductionPercentage, maxStacks);
    }

    private void OnDestroy()
    {
        if (towerController != null) towerController.OnTargetDamaged -= HandleTowerAttack;
    }
}