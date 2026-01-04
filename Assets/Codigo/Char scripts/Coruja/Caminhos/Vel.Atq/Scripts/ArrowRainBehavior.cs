using UnityEngine;
using System;

public class ArrowRainBehavior : TowerBehavior
{
    public int freeArrows = 3;

    public override void Initialize(TowerController owner)
    {
        base.Initialize(owner);
        if (towerController != null)
        {
            // Agora se inscreve no novo evento OnEnemyKilled
            towerController.OnEnemyKilled += HandleEnemyKilled;
        }
    }

    private void HandleEnemyKilled(EnemyHealthSystem target)
    {
        // Lógica para disparar 3 flechas adicionais
        // Exemplo: towerController.ShootArrows(freeArrows);
        Debug.Log("Chuva de Flechas! Disparando " + freeArrows + " flechas extras.");
    }

    private void OnDestroy()
    {
        if (towerController != null)
        {
            towerController.OnEnemyKilled -= HandleEnemyKilled;
        }
    }
}