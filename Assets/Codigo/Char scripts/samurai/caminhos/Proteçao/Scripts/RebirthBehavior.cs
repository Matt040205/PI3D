// RebirthBehavior.cs
using UnityEngine;

/// <summary>
/// Procura por torres destru�das e as revive ap�s um tempo de recarga.
/// </summary>
public class RebirthBehavior : TowerBehavior
{
    [Header("Configura��o do Renascimento")]
    public float reviveHealthPercentage = 0.20f; // 20%
    public float cooldown = 90f; // 90 segundos
    public float scanRadius = 10f; // Raio de alcance para reviver

    private float cooldownTimer = 0f;

    void Update()
    {
        // Se a habilidade estiver em recarga, contamos o tempo.
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            return; // Sai do Update, n�o faz mais nada
        }

        // Se n�o estiver em recarga, procura por um alvo.
        FindAndReviveTower();
    }

    void FindAndReviveTower()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, scanRadius);
        foreach (var col in colliders)
        {
            TowerController otherTower = col.GetComponent<TowerController>();

            // Se encontrarmos uma torre que est� destru�da e n�o � esta mesma torre...
            if (otherTower != null && otherTower.IsDestroyed && otherTower != this.towerController)
            {
                // ...a revivemos!
                otherTower.Revive(reviveHealthPercentage);

                // Colocamos a habilidade em recarga.
                cooldownTimer = cooldown;
                Debug.Log($"RENASCIMENTO ativado! {otherTower.name} foi revivida. Habilidade em recarga por {cooldown}s.");

                // Revivemos apenas uma torre por vez, ent�o sa�mos do loop.
                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, scanRadius);
    }
}