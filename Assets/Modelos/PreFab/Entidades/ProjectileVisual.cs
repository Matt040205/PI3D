using UnityEngine;

public class ProjectileVisual : MonoBehaviour
{
    [Header("Configurações")]
    public float speed = 80f;
    public float maxLifetime = 2f;

    private Vector3 targetPosition;
    private bool hasTarget;
    private float startTime;
    private ProjectilePool pool;

    public void Initialize(Vector3 target)
    {
        targetPosition = target;
        hasTarget = true;
        startTime = Time.time;

        // Destruir após tempo máximo (segurança)
        Invoke("ReturnToPool", maxLifetime);
    }

    public void SetPoolReference(ProjectilePool poolReference)
    {
        pool = poolReference;
    }

    void Update()
    {
        if (!hasTarget) return;

        // Calcular direção e distância
        Vector3 direction = (targetPosition - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        // Mover em direção ao alvo
        float step = speed * Time.deltaTime;

        // Se estiver muito perto, teletransportar para o alvo
        if (step > distanceToTarget)
        {
            transform.position = targetPosition;
        }
        else
        {
            transform.position += direction * step;
        }

        // Rotacionar na direção do movimento
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Destruir quando chegar perto do alvo
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            ReturnToPool();
        }
    }

    void ReturnToPool()
    {
        if (pool != null)
        {
            pool.ReturnProjectile(gameObject);
        }
        else
        {
            // --- MUDANÇA PARA POOLING: Fallback removido ---
            // Substitui o Destroy(gameObject) por um log e desativação.
            Debug.LogError("Projetil escapou do Pool! Desativando em vez de destruir.");
            gameObject.SetActive(false);
        }
    }
}