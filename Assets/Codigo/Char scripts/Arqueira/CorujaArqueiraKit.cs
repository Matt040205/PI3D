using UnityEngine;

[CreateAssetMenu(fileName = "Kit da Coruja Arqueira", menuName = "ExoBeasts/Personagens/Coruja Arqueira")]
public class CorujaArqueiraKit : ScriptableObject
{
    [Header("Atributos Básicos")]
    public float health = 100f; // Exemplo de vida
    public string weaponType = "Arco";
    public string primaryAttackName = "Flecha Certeira";
    public float damage = 20f; // Dano base
    public float headshotBonus = 5f; // Bônus de dano para headshot
    public float movementSpeed = 5f; // Velocidade de movimento moderada
    public float attackSpeed = 0.5f; // Velocidade de ataque lenta
    public float reloadSpeed = 0.5f; // Velocidade de recarga lenta
    public float abilitySpeed = 1.5f; // Velocidade de habilidades rápida

    [Header("Habilidades")]
    public PassivaComandanteCoruja passivaComandante;
    public HabilidadeVooGracioso vooGracioso;
    public HabilidadePerseguindoPresas perseguindoPresas;
    public HabilidadeCacadoraNoturna cacadoraNoturna;
}