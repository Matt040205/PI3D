// FuryStackBehavior.cs
using UnityEngine;

/// <summary>
/// Aumenta o dano da torre a cada ataque consecutivo no mesmo alvo.
/// </summary>
public class FuryStackBehavior : TowerBehavior
{
    [Header("Configurações da Fúria")]
    [Tooltip("Bônus de dano por acúmulo (0.08 = 8%).")]
    public float damageBonusPerStack = 0.08f;
    public int maxStacks = 6;

    private int currentStacks = 0;
    private EnemyHealthSystem lastTarget; // Armazena a referência do último alvo

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
        // Se o novo alvo for o mesmo que o último que atacamos...
        if (newTarget != null && newTarget == lastTarget)
        {
            // ...e ainda não atingimos o máximo de acúmulos...
            if (currentStacks < maxStacks)
            {
                // ...adicionamos um acúmulo.
                currentStacks++;
                Debug.Log($"FÚRIA ACUMULADA: Acerto consecutivo! Stacks: {currentStacks}");
            }
        }
        else
        {
            // Se o alvo mudou (ou o último alvo morreu), reiniciamos.
            // Começamos com 1, pois este acerto já é o primeiro da nova sequência.
            currentStacks = 1;
            Debug.Log("FÚRIA ACUMULADA: Alvo mudou. Stacks resetados para 1.");
        }

        // Atualizamos a referência do último alvo.
        lastTarget = newTarget;
    }

    private float ApplyFuryDamage(EnemyHealthSystem target, float currentDamage)
    {
        // Se não tivermos acúmulos, não fazemos nada.
        if (currentStacks == 0)
        {
            return currentDamage;
        }

        // Calcula o bônus total baseado nos acúmulos.
        // Subtraímos 1 porque o bônus só se aplica a partir do SEGUNDO acerto consecutivo.
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