using UnityEngine;
using System.Collections.Generic;

public class CacadoraNoturnaLogic : MonoBehaviour
{
    private float damage;
    private float radius;
    private GameObject quemUsou;

    public void StartUltimateEffect(GameObject user, float ultimateDamage, float ultimateRadius)
    {
        this.quemUsou = user;
        this.damage = ultimateDamage;
        this.radius = ultimateRadius;

        // Use Physics.OverlapSphere para encontrar todos os inimigos no raio
        Collider[] hitColliders = Physics.OverlapSphere(quemUsou.transform.position, radius);

        foreach (var hitCollider in hitColliders)
        {
            EnemyHealthSystem enemyHealth = hitCollider.GetComponent<EnemyHealthSystem>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log($"<color=red>Ultimate ativada!</color> Inimigo {enemyHealth.gameObject.name} tomou {damage} de dano global!");
            }
        }

        Destroy(gameObject);
    }
}