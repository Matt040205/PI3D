// FuryStackBehavior.cs
using UnityEngine;

/// <summary>
/// Aumenta o dano da torre a cada ataque consecutivo no mesmo alvo.
/// </summary>
public class FuryStackBehavior : TowerBehavior
{
    [Header("Configura��es da F�ria")]
    [Tooltip("B�nus de dano por ac�mulo (0.08 = 8%).")]
    public float damageBonusPerStack = 0.08f;
    public int maxStacks = 6;

    private int currentStacks = 0;
    private EnemyHealthSystem lastTarget; // Armazena a refer�ncia do �ltimo alvo

    public override void Initialize(TowerController owner)
    {
        base.Initialize(owner);
        if (towerController != null)
        {
            // Conectamos a dois eventos:
            // 1. Para saber quando um ataque acontece e em quem.
            towerController.OnTargetDamaged += HandleTargetHit;
            // 2. Para modificar o dano ANTES de ele ser aplicado.
            towerController.OnCalculateDamage += ApplyFuryDamage;
        }
    }

    private void HandleTargetHit(EnemyHealthSystem newTarget)
    {
        // Se o novo alvo for o mesmo que o �ltimo que atacamos...
        if (newTarget != null && newTarget == lastTarget)
        {
            // ...e ainda n�o atingimos o m�ximo de ac�mulos...
            if (currentStacks < maxStacks)
            {
                // ...adicionamos um ac�mulo.
                currentStacks++;
                Debug.Log($"F�RIA ACUMULADA: Acerto consecutivo! Stacks: {currentStacks}");
            }
        }
        else
        {
            // Se o alvo mudou (ou o �ltimo alvo morreu), reiniciamos.
            // Come�amos com 1, pois este acerto j� � o primeiro da nova sequ�ncia.
            currentStacks = 1;
            Debug.Log("F�RIA ACUMULADA: Alvo mudou. Stacks resetados para 1.");
        }

        // Atualizamos a refer�ncia do �ltimo alvo.
        lastTarget = newTarget;
    }

    private float ApplyFuryDamage(EnemyHealthSystem target, float currentDamage)
    {
        // Se n�o tivermos ac�mulos, n�o fazemos nada.
        if (currentStacks == 0)
        {
            return currentDamage;
        }

        // Calcula o b�nus total baseado nos ac�mulos.
        // Subtra�mos 1 porque o b�nus s� se aplica a partir do SEGUNDO acerto consecutivo.
        int effectiveStacks = currentStacks - 1;
        if (effectiveStacks > 0)
        {
            float bonusDamage = currentDamage * (effectiveStacks * damageBonusPerStack);
            return currentDamage + bonusDamage;
        }

        return currentDamage;
    }

    private void OnDestroy()
    {
        if (towerController != null)
        {
            towerController.OnTargetDamaged -= HandleTargetHit;
            towerController.OnCalculateDamage -= ApplyFuryDamage;
        }
    }
}