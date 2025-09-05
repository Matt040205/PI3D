// LegacyAuraBehavior.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Concede um bônus de dano e velocidade de ataque para torres aliadas em um raio.
/// </summary>
public class LegacyAuraBehavior : TowerBehavior
{
    [Header("Configuração do Legado")]
    public float damageBonus = 0.25f; // 25%
    public float attackSpeedBonus = 0.15f; // 15%
    public float auraRadius = 8f; // Um raio maior para a habilidade final

    private List<TowerController> affectedTowers = new List<TowerController>();
    private float timer;

    void Update()
    {
        if (towerController == null) return;

        timer += Time.deltaTime;
        if (timer >= 0.5f)
        {
            UpdateAura();
            timer = 0f;
        }
    }

    void UpdateAura()
    {
        var towersInRange = FindTowersInRange();

        // Remove o bônus das torres que saíram do alcance
        foreach (var tower in affectedTowers.ToList())
        {
            if (!towersInRange.Contains(tower))
            {
                tower.AddDamageBonus(-damageBonus);
                tower.AddAttackSpeedBonus(-attackSpeedBonus);
                affectedTowers.Remove(tower);
            }
        }

        // Adiciona o bônus para as novas torres que entraram no alcance
        foreach (var tower in towersInRange)
        {
            if (!affectedTowers.Contains(tower))
            {
                tower.AddDamageBonus(damageBonus);
                tower.AddAttackSpeedBonus(attackSpeedBonus);
                affectedTowers.Add(tower);
            }
        }
    }

    List<TowerController> FindTowersInRange()
    {
        return Physics.OverlapSphere(transform.position, auraRadius)
            .Select(col => col.GetComponent<TowerController>())
            .Where(tower => tower != null && tower != this.towerController)
            .ToList();
    }

    private void OnDestroy()
    {
        foreach (var tower in affectedTowers)
        {
            if (tower != null)
            {
                tower.AddDamageBonus(-damageBonus);
                tower.AddAttackSpeedBonus(-attackSpeedBonus);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, auraRadius);
    }
}