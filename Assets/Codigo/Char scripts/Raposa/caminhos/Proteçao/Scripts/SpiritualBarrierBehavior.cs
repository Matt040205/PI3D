// SpiritualBarrierBehavior.cs
using UnityEngine;

/// <summary>
/// Absorve uma porcentagem do dano que seria recebido por torres aliadas próximas.
/// </summary>
public class SpiritualBarrierBehavior : TowerBehavior
{
    [Header("Configuração da Barreira")]
    [Tooltip("A porcentagem de dano a ser absorvida (0.15 = 15%).")]
    public float damageAbsorption = 0.15f;
    public float barrierRadius = 5f;

    /// <summary>
    /// Este método é chamado por outras torres que estão sofrendo dano.
    /// </summary>
    /// <param name="incomingDamage">O dano que a outra torre receberia.</param>
    /// <returns>O dano restante que a outra torre deve receber.</returns>
    public float AbsorbDamage(float incomingDamage)
    {
        if (towerController == null)
        {
            return incomingDamage;
        }

        // Calcula a quantidade de dano que esta torre vai absorver
        float absorbedDamage = incomingDamage * damageAbsorption;

        // Esta torre sofre o dano que absorveu
        towerController.TakeDamage(absorbedDamage);
        Debug.Log($"BARREIRA ESPIRITUAL: Torre {towerController.name} absorveu {absorbedDamage} de dano.");

        // Retorna o dano que "sobrou" para a torre original
        return incomingDamage - absorbedDamage;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0f, 1f, 0.5f); // Roxo
        Gizmos.DrawWireSphere(transform.position, barrierRadius);
    }
}