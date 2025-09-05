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

    // Usamos o Update para a lógica de aura que roda constantemente
    void Update()
    {
        // Se a torre não estiver inicializada, não faz nada.
        if (towerController == null) return;

        timer += Time.deltaTime;
        // A cada 1 segundo...
        if (timer >= 1f)
        {
            // ...procura e cura as torres.
            HealNearbyTowers();
            timer = 0f; // Reseta o timer
        }
    }

    void HealNearbyTowers()
    {
        // Cria uma esfera invisível e pega tudo que colidir com ela
        Collider[] colliders = Physics.OverlapSphere(transform.position, auraRadius);

        foreach (var col in colliders)
        {
            // Verifica se o objeto encontrado tem um TowerController
            TowerController otherTower = col.GetComponent<TowerController>();

            // Se for uma torre e não for ela mesma...
            if (otherTower != null && otherTower != this.towerController)
            {
                // ...calcula a cura e chama o método Heal que criamos.
                // (Esta linha está comentada pois precisa que o TowerController tenha o 'maxHealth' público)
                // float healAmount = otherTower.maxHealth * healPercentagePerSecond;
                // otherTower.Heal(healAmount);
                Debug.Log($"Aura curando a torre {otherTower.name}.");
            }
        }
    }

    // Desenha uma esfera no editor para vermos o alcance da aura
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, auraRadius);
    }
}