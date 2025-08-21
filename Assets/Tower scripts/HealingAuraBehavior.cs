using UnityEngine;

public class HealingAuraBehavior : TowerBehavior
{
    // Parâmetros que definem o funcionamento da aura.
    private float healAmountPerSecond = 5f;
    private float auraRange = 7f;
    private float tickRate = 1f; // A cada 1 segundo para não rodar a cada frame
    private float timer;

    void FixedUpdate()
    {
        // Se o comportamento não foi devidamente inicializado, ele não faz nada.
        if (towerController == null) return;

        timer += Time.fixedDeltaTime;
        if (timer >= tickRate)
        {
            timer = 0f;
            HealNearbyAllies();
        }
    }

    private void HealNearbyAllies()
    {
        // Encontra todos os colliders em um raio
        Collider[] nearbyAllies = Physics.OverlapSphere(transform.position, auraRange);
        foreach (var allyCollider in nearbyAllies)
        {
            // Tenta pegar o sistema de vida do objeto encontrado
            var healthSystem = allyCollider.GetComponent<PlayerHealthSystem>();
            if (healthSystem != null)
            {
                // Aplica a cura
                healthSystem.Heal(healAmountPerSecond * tickRate);
            }
        }
    }
}