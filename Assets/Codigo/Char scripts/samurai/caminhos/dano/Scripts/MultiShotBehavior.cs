// MultiShotBehavior.cs
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

    private void FireExtraProjectiles(EnemyHealthSystem mainTarget)
    {
        Debug.Log($"DANÇA DAS CAUDAS! Disparando {extraProjectiles} projéteis extras.");

        // --- NOTA IMPORTANTE ---
        // A lógica aqui depende de como seu jogo dispara projéteis.
        // Você precisará de uma função no seu TowerController ou em um sistema de projéteis
        // para criar e disparar esses tiros extras.

        // Exemplo de como poderia ser:
        // FindNearbyTargetsAndFire(mainTarget, extraProjectiles, towerController.currentDamage * damageMultiplier);
    }

    private void OnDestroy()
    {
        if (towerController != null)
        {
            towerController.OnTargetDamaged -= FireExtraProjectiles;
        }
    }
}