// HealingAuraBehavior.cs
using UnityEngine;

/// <summary>
/// Cura torres aliadas dentro de um raio a cada segundo.
/// </summary>
public class HealingAuraBehavior : TowerBehavior
{
    [Header("Configuração da Aura")]
    [Tooltip("A cura por segundo, como porcentagem da vida máxima do alvo (0.01 = 1%).")]
    public float healPercentagePerSecond = 0.01f;
    public float auraRadius = 5f;

    private float timer; // Timer para contar a cada segundo

    void Update()
    {
        if (towerController == null) return;

        timer += Time.deltaTime;
        if (timer >= 1f)
        {
            HealNearbyTowers();
            timer = 0f; // Reseta o timer
        }
    }

    void HealNearbyTowers()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, auraRadius);

        foreach (var col in colliders)
        {
            TowerController otherTower = col.GetComponent<TowerController>();

            if (otherTower != null && otherTower != this.towerController)
            {

                float healAmount = otherTower.MaxHealth * healPercentagePerSecond;
                otherTower.Heal(healAmount);
                Debug.Log($"Aura curando a torre {otherTower.name}.");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, auraRadius);
    }
}