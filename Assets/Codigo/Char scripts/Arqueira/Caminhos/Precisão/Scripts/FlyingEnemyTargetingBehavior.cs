using UnityEngine;

public class FlyingEnemyTargetingBehavior : TowerBehavior
{
    public override void Initialize(TowerController owner)
    {
        base.Initialize(owner);
        // Lógica para permitir que a torre ataque inimigos voadores.
        // A maneira mais limpa de implementar isso é se a classe TowerController
        // tiver um método como `SetTargetingFlyingEnemies(true)`.
        // Como o seu TowerController original só mira em uma tag, você teria
        // que modificar a lógica de `UpdateTarget` para que ela busque
        // por inimigos com a tag "Enemy" E também com a tag "FlyingEnemy".
        // Uma alternativa é a classe TowerController ter uma lista de tags,
        // que este comportamento pode adicionar.
        Debug.Log("FlyingEnemyTargetingBehavior ativado. A torre agora pode atacar inimigos voadores.");
    }

    private void OnDestroy()
    {
        // Se a torre for vendida, a lógica de remoção deveria ir aqui.
        // Exemplo: towerController.SetTargetingFlyingEnemies(false);
    }
}