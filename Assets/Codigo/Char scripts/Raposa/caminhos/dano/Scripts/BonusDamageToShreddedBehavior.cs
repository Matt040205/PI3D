// BonusDamageToShreddedBehavior.cs
using UnityEngine;

/// <summary>
/// Aumenta o dano da torre contra alvos que já estão com a armadura reduzida.
/// </summary>
public class BonusDamageToShreddedBehavior : TowerBehavior
{
    [Header("Configuração do Bônus")]
    [Tooltip("O bônus de dano multiplicativo. 0.2 para 20%.")]
    public float damageBonus = 0.2f;

    public override void Initialize(TowerController owner)
    {
        base.Initialize(owner);
        if (towerController != null)
        {
            // Se conecta ao evento que calcula o dano
            towerController.OnCalculateDamage += ApplyDamageBonus;
        }
    }

    // Este método recebe o alvo e o dano atual, e retorna o dano modificado.
    private float ApplyDamageBonus(EnemyHealthSystem target, float currentDamage)
    {
        // A mágica acontece aqui: verificamos a propriedade IsArmorShredded do inimigo.
        if (target != null && target.IsArmorShredded)
        {
            // Se for verdade, retorna o dano com o bônus.
            return currentDamage * (1 + damageBonus);
        }

        // Caso contrário, retorna o dano normal.
        return currentDamage;
    }

    private void OnDestroy()
    {
        if (towerController != null)
        {
            towerController.OnCalculateDamage -= ApplyDamageBonus;
        }
    }
}