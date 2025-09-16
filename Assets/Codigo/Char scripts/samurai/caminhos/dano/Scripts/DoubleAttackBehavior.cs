// DoubleAttackBehavior.cs
using UnityEngine;

/// <summary>
/// Concede à torre uma chance de atacar uma segunda vez imediatamente.
/// </summary>
public class DoubleAttackBehavior : TowerBehavior
{
    [Header("Configuração do Ataque Duplo")]
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

    // Este método é chamado após o dano ser aplicado.
    private void TryDoubleAttack(EnemyHealthSystem target)
    {
        // Random.value gera um número aleatório entre 0.0 e 1.0
        if (Random.value <= chance)
        {
            Debug.Log("SORTE! Corte Duplo ativado!");
            // Chama o método público que criamos na torre para forçar um novo tiro.
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