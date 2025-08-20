using UnityEngine;

public class EnemyHealthSystem : MonoBehaviour
{
    [Header("Referências")]
    public EnemyDataSO enemyData;

    [Header("Status Atual")]
    public float currentHealth;
    public bool isDead;

    // Componente controlador
    private EnemyController enemyController;

    void Awake()
    {
        enemyController = GetComponent<EnemyController>();
    }

    public void InitializeHealth(int level)
    {
        if (enemyData == null)
        {
            Debug.LogError("EnemyData não atribuído em " + gameObject.name);
            return;
        }

        currentHealth = enemyData.GetHealth(level);
        isDead = false;
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

        // Notifica o controlador sobre a morte
        if (enemyController != null)
        {
            enemyController.HandleDeath();
        }
        else
        {
            // Fallback caso não haja controlador
            Destroy(gameObject);
        }
    }
}