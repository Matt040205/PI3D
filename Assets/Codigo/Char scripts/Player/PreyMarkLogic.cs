using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PreyMarkLogic : MonoBehaviour
{
    private float markDuration;
    private float bonusDamageMultiplier;

    public void StartEffect(float duration, float damageBonus, CommanderAbilityController abilityController, Ability sourceAbility)
    {
        this.markDuration = duration;
        this.bonusDamageMultiplier = damageBonus;

        if (abilityController != null)
        {
            abilityController.SetAbilityUsage(sourceAbility, true);
        }

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

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        gameObject.SetActive(false);
    }
}