// MultiShotBehavior.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Faz com que o ataque principal da torre dispare projéteis secundários.
/// </summary>
public class MultiShotBehavior : TowerBehavior
{
    [Header("Configuração do Multi-Tiro")]
    public int extraProjectiles = 3;
    [Tooltip("O dano dos projéteis extras em relação ao dano normal (0.5 = 50%).")]
    public float damageMultiplier = 0.5f;

    public override void Initialize(TowerController owner)
    {
        base.Initialize(owner);
        if (towerController != null)
        {
            towerController.OnTargetDamaged += FireExtraProjectiles;
        }
    }

    // Exemplo de como implementar MultiShotBehavior.cs/FireExtraProjectiles
    private void FireExtraProjectiles(EnemyHealthSystem mainTarget)
    {
        Debug.Log($"DANÇA DAS CAUDAS! Disparando {extraProjectiles} projéteis extras.");

        // Obter alvos próximos (excluindo o alvo principal)
        Collider[] colliders = Physics.OverlapSphere(towerController.transform.position, towerController.CurrentRange);

        // Lista de alvos válidos (com EnemyController e vivos)
        List<EnemyHealthSystem> validTargets = new List<EnemyHealthSystem>();
        foreach (var col in colliders)
        {
            EnemyHealthSystem target = col.GetComponent<EnemyHealthSystem>();
            if (target != null && target != mainTarget && !target.isDead) // Assumindo isDead existe
            {
                validTargets.Add(target);
            }
        }

        // Disparar o número de projéteis extras para alvos aleatórios
        for (int i = 0; i < extraProjectiles && validTargets.Count > 0; i++)
        {
            // Escolhe um alvo aleatório entre os válidos
            EnemyHealthSystem randomTarget = validTargets[Random.Range(0, validTargets.Count)];

            // Dispara o projétil (Esta função de disparo deve existir no seu TowerController ou em um Projétil Manager)
            // towerController.FireProjectileAt(randomTarget.transform, towerController.CurrentDamage * damageMultiplier);
        }
    }

    private void OnDestroy()
    {
        if (towerController != null)
        {
            towerController.OnTargetDamaged -= FireExtraProjectiles;
        }
    }
}