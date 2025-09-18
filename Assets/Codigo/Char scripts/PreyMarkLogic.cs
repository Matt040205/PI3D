using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PreyMarkLogic : MonoBehaviour
{
    private float markDuration;
    private float bonusDamageMultiplier;

    public void StartEffect(float duration, float damageBonus)
    {
        this.markDuration = duration;
        this.bonusDamageMultiplier = damageBonus;
        StartCoroutine(ApplyMarkedStatus());
    }

    private IEnumerator ApplyMarkedStatus()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        List<EnemyHealthSystem> markedEnemies = new List<EnemyHealthSystem>();

        foreach (GameObject enemy in enemies)
        {
            EnemyHealthSystem healthSystem = enemy.GetComponent<EnemyHealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.ApplyMarkedStatus(bonusDamageMultiplier);
                markedEnemies.Add(healthSystem);
            }
        }

        yield return new WaitForSeconds(markDuration);

        foreach (var enemy in markedEnemies)
        {
            if (enemy != null)
            {
                enemy.RemoveMarkedStatus();
            }
        }

        Destroy(gameObject);
    }
}