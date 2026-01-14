// ArmorAuraBehavior.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Concede um bônus de armadura para torres aliadas dentro de um raio.
/// </summary>
public class ArmorAuraBehavior : TowerBehavior
{
    [Header("Configuração da Aura")]
    public float armorBonus = 0.10f; // 10%
    public float auraRadius = 5f;

    // Lista para guardar as torres que estão atualmente recebendo o bônus
    private List<TowerController> affectedTowers = new List<TowerController>();

    private float timer;

    void Update()
    {
        if (towerController == null) return;

        timer += Time.deltaTime;
        // Verificamos a cada meio segundo para não sobrecarregar
        if (timer >= 0.5f)
        {
            UpdateAura();
            timer = 0f;
        }
    }

    void UpdateAura()
    {
        List<TowerController> towersInRange = new List<TowerController>();
        Collider[] colliders = Physics.OverlapSphere(transform.position, auraRadius);

        foreach (var col in colliders)
        {
            TowerController otherTower = col.GetComponent<TowerController>();
            if (otherTower != null && otherTower != this.towerController)
            {
                towersInRange.Add(otherTower);
            }
        }

        // Remove o bônus das torres que saíram do alcance
        foreach (var tower in affectedTowers.ToList()) // Usamos .ToList() para poder modificar a lista original
        {
            if (!towersInRange.Contains(tower))
            {
                tower.AddArmorBonus(-armorBonus); // Remove o bônus
                affectedTowers.Remove(tower);
                Debug.Log($"Torre {tower.name} saiu da aura de armadura.");
            }
        }

        // Adiciona o bônus para as novas torres que entraram no alcance
        foreach (var tower in towersInRange)
        {
            if (!affectedTowers.Contains(tower))
            {
                tower.AddArmorBonus(armorBonus); // Adiciona o bônus
                affectedTowers.Add(tower);
                Debug.Log($"Torre {tower.name} entrou na aura de armadura.");
            }
        }
    }

    // Garante que o bônus seja removido de todas as torres se esta for destruída
    private void OnDestroy()
    {
        foreach (var tower in affectedTowers)
        {
            if (tower != null)
            {
                tower.AddArmorBonus(-armorBonus);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, auraRadius);
    }
}