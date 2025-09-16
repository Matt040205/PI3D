// ArmorAuraBehavior.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Concede um b�nus de armadura para torres aliadas dentro de um raio.
/// </summary>
public class ArmorAuraBehavior : TowerBehavior
{
    [Header("Configura��o da Aura")]
    public float armorBonus = 0.10f; // 10%
    public float auraRadius = 5f;

    // Lista para guardar as torres que est�o atualmente recebendo o b�nus
    private List<TowerController> affectedTowers = new List<TowerController>();

    private float timer;

    void Update()
    {
        if (towerController == null) return;

        timer += Time.deltaTime;
        // Verificamos a cada meio segundo para n�o sobrecarregar
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

        // Remove o b�nus das torres que sa�ram do alcance
        foreach (var tower in affectedTowers.ToList()) // Usamos .ToList() para poder modificar a lista original
        {
            if (!towersInRange.Contains(tower))
            {
                tower.AddArmorBonus(-armorBonus); // Remove o b�nus
                affectedTowers.Remove(tower);
                Debug.Log($"Torre {tower.name} saiu da aura de armadura.");
            }
        }

        // Adiciona o b�nus para as novas torres que entraram no alcance
        foreach (var tower in towersInRange)
        {
            if (!affectedTowers.Contains(tower))
            {
                tower.AddArmorBonus(armorBonus); // Adiciona o b�nus
                affectedTowers.Add(tower);
                Debug.Log($"Torre {tower.name} entrou na aura de armadura.");
            }
        }
    }

    // Garante que o b�nus seja removido de todas as torres se esta for destru�da
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