using UnityEngine;

public class EnemyHealthSystem : MonoBehaviour
{
    [Header("Configura��es")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isDead;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        // Implementar l�gica de morte do inimigo
        Destroy(gameObject);
    }
}