// DoubleAttackBehavior.cs
using UnityEngine;

/// <summary>
/// Concede � torre uma chance de atacar uma segunda vez imediatamente.
/// </summary>
public class DoubleAttackBehavior : TowerBehavior
{
    [Header("Configura��o do Ataque Duplo")]
    [Tooltip("A chance de atacar uma segunda vez (0.25 para 25%).")]
    [Range(0f, 1f)]
    public float chance = 0.25f;

    public override void Initialize(TowerController owner)
    {
        base.Initialize(owner);
        if (towerController != null)
        {
            // Se conecta ao evento que avisa quando um alvo tomou dano.
            towerController.OnTargetDamaged += TryDoubleAttack;
        }
    }

    // Este m�todo � chamado ap�s o dano ser aplicado.
    private void TryDoubleAttack(EnemyHealthSystem target)
    {
        // Random.value gera um n�mero aleat�rio entre 0.0 e 1.0
        if (Random.value <= chance)
        {
            Debug.Log("SORTE! Corte Duplo ativado!");
            // Chama o m�todo p�blico que criamos na torre para for�ar um novo tiro.
            towerController.PerformExtraAttack();
        }
    }

    private void OnDestroy()
    {
        if (towerController != null)
        {
            towerController.OnTargetDamaged -= TryDoubleAttack;
        }
    }
}