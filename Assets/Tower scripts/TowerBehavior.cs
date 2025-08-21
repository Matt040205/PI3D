using UnityEngine;

/// <summary>
/// Classe base para todos os comportamentos especiais que uma torre pode ganhar via upgrades.
/// 'abstract' significa que este script não pode ser usado diretamente, apenas herdado.
/// </summary>
public abstract class TowerBehavior : MonoBehaviour
{
    // Uma referência protegida para a torre que possui este comportamento.
    protected TowerController towerController;

    /// <summary>
    /// Método de inicialização chamado pelo TowerController quando este comportamento é adicionado.
    /// </summary>
    /// <param name="owner">A instância do TowerController que está recebendo este comportamento.</param>
    public virtual void Initialize(TowerController owner)
    {
        this.towerController = owner;
    }
}