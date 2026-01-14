using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class ProjectileVisual : MonoBehaviour
{
    [Header("Configura��es")]
    public float speed = 80f;
    public float maxLifetime = 2f;
    public GameObject impactEffectPrefab;

    private float damage;
    private bool isCritical;
    private float armorPenetration;
    private PlayerHealthSystem playerHealth;

    private ProjectilePool pool;
    private Rigidbody rb;
    private bool hasHit;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
        GetComponent<Collider>().isTrigger = true;
    }

    public void Initialize(float damage, bool isCritical, float armorPenetration, PlayerHealthSystem playerHealth, Vector3 direction)
    {
        this.damage = damage;
        this.isCritical = isCritical;
        this.armorPenetration = armorPenetration;
        this.playerHealth = playerHealth;
        this.hasHit = false;

        rb.linearVelocity = direction * speed;
        CancelInvoke(nameof(ReturnToPool));
        Invoke(nameof(ReturnToPool), maxLifetime);
    }

    public void SetPoolReference(ProjectilePool poolReference)
    {
        pool = poolReference;
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        if (other.CompareTag("Player")) return;

        hasHit = true;
        rb.linearVelocity = Vector3.zero;

        EnemyHealthSystem enemyHealth = other.GetComponent<EnemyHealthSystem>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage, armorPenetration, isCritical);

            if (playerHealth != null)
            {
                playerHealth.TriggerDamageDealt(damage);
            }
        }

        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, Quaternion.LookRotation(other.transform.position - transform.position));
        }

        ReturnToPool();
    }

    void ReturnToPool()
    {
        CancelInvoke();
        rb.linearVelocity = Vector3.zero;
        if (pool != null)
        {
            pool.ReturnProjectile(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}