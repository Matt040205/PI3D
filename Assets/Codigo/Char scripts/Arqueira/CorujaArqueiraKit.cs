using UnityEngine;
using System.Collections.Generic; // Necessário para a lista de caminhos de upgrade

[CreateAssetMenu(fileName = "CorujaArqueiraData", menuName = "Tower Defense/Personagens/Coruja Arqueira")]
public class CorujaArqueiraKit : CharacterBase
{
    private void OnValidate()
    {
        // Define os atributos da Coruja Arqueira de acordo com o design
        maxHealth = 100f; // Vida Baixa
        damage = 25f; // Dano Alto (exemplo de valor base)
        critDamage = 2.5f; // Dano super alto para headshot/crítico
        moveSpeed = 5f; // Velocidade moderada
        attackSpeed = 0.5f; // Ataque lento
        reloadSpeed = 0.5f; // Recarga lenta

        // Tipo de Combate
        combatType = CombatType.Ranged;
        fireMode = FireMode.SemiAuto;
        meleeRange = 20f; // Grande alcance para um arqueiro
    }
}