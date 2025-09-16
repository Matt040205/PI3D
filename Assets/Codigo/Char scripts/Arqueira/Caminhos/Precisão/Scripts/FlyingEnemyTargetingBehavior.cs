using UnityEngine;

public class FlyingEnemyTargetingBehavior : TowerBehavior
{
    public override void Initialize(TowerController owner)
    {
        base.Initialize(owner);
        // L�gica para permitir que a torre ataque inimigos voadores.
        // A maneira mais limpa de implementar isso � se a classe TowerController
        // tiver um m�todo como `SetTargetingFlyingEnemies(true)`.
        // Como o seu TowerController original s� mira em uma tag, voc� teria
        // que modificar a l�gica de `UpdateTarget` para que ela busque
        // por inimigos com a tag "Enemy" E tamb�m com a tag "FlyingEnemy".
        // Uma alternativa � a classe TowerController ter uma lista de tags,
        // que este comportamento pode adicionar.
        Debug.Log("FlyingEnemyTargetingBehavior ativado. A torre agora pode atacar inimigos voadores.");
    }

    private void OnDestroy()
    {
        // Se a torre for vendida, a l�gica de remo��o deveria ir aqui.
        // Exemplo: towerController.SetTargetingFlyingEnemies(false);
    }
}