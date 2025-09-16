using UnityEngine;

public class FuryStackyBehavior : TowerBehavior
{
    public float bonusPerStack = 0.05f; // 5%
    public int maxStacks = 8;
    private int currentStacks = 0;

    public override void Initialize(TowerController owner)
    {
        base.Initialize(owner);
        if (towerController != null)
        {
            towerController.OnCriticalHit += HandleCriticalHit;
        }
    }

    private void HandleCriticalHit(EnemyHealthSystem target)
    {
        if (currentStacks < maxStacks)
        {
            currentStacks++;
            // L�gica para aplicar o b�nus de velocidade de ataque
            // Exemplo: towerController.AddAttackSpeedBonus(bonusPerStack);
            Debug.Log("F�ria da Ca�adora! Ac�mulo " + currentStacks + "/" + maxStacks + ". Vel. de ataque aumentada.");
        }
    }

    private void OnDestroy()
    {
        if (towerController != null)
        {
            towerController.OnCriticalHit -= HandleCriticalHit;
            // Se a torre for vendida ou destru�da, remover o b�nus total
            // Exemplo: towerController.RemoveAttackSpeedBonus(currentStacks * bonusPerStack);
        }
    }
}