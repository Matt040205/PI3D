// MultiShotBehavior.cs
using UnityEngine;

/// <summary>
/// Faz com que o ataque principal da torre dispare proj�teis secund�rios.
/// </summary>
public class MultiShotBehavior : TowerBehavior
{
    [Header("Configura��o do Multi-Tiro")]
    public int extraProjectiles = 3;
    [Tooltip("O dano dos proj�teis extras em rela��o ao dano normal (0.5 = 50%).")]
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
        Debug.Log($"DAN�A DAS CAUDAS! Disparando {extraProjectiles} proj�teis extras.");

        // --- NOTA IMPORTANTE ---
        // A l�gica aqui depende de como seu jogo dispara proj�teis.
        // Voc� precisar� de uma fun��o no seu TowerController ou em um sistema de proj�teis
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