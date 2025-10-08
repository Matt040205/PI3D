using UnityEngine;

// IMPORTANTE: Adicione um campo protegido 'protected TowerController owner;'
// à sua classe base TowerBehavior, se ainda não o tiver.
// Assumindo que o enum EnemyType está globalmente acessível.

public class FlyingEnemyTargetingBehavior : TowerBehavior
{
    // A classe base (TowerBehavior) provavelmente armazena o 'owner'
    // em um campo acessível (como 'protected TowerController towerController;').
    // Vamos assumir que 'owner' é o nome da referência para a clareza.
    private TowerController towerOwner; // Adicionamos uma referência para o OnDestroy

    public override void Initialize(TowerController owner)
    {
        base.Initialize(owner);
        this.towerOwner = owner; // Armazena a referência.

        // NOTA: Esta linha é a chave. Ela assume que TowerController possui
        // a propriedade TargetsFlying, que você deve adicionar.
        if (owner != null)
        {
            owner.TargetsFlyingEnemies = true;
            Debug.Log("FlyingEnemyTargetingBehavior ativado. A torre agora pode atacar inimigos voadores.");
        }
    }

    private void OnDestroy()
    {
        // Se a torre for vendida ou o comportamento for removido, desativa a funcionalidade.
        if (towerOwner != null)
        {
            towerOwner.TargetsFlyingEnemies = false;
            Debug.Log("FlyingEnemyTargetingBehavior desativado. A torre não atacará mais inimigos voadores.");
        }
    }
}