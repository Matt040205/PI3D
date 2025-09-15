using UnityEngine;

[CreateAssetMenu(fileName = "Caçadora Noturna", menuName = "ExoBeasts/Habilidades/Caçadora Noturna")]
public class HabilidadeCacadoraNoturna : Ability
{
    [Header("Configurações da Habilidade")]
    public float damage = 500f; // Dano da ultimate
    public float radius = 1000f; // Raio global (ou muito grande)

    public override bool Activate(GameObject quemUsou)
    {
        // Lógica para disparar um projétil ou efeito global.
        // Causar dano a todos os inimigos no mapa.
        Debug.Log("Caçadora Noturna ativado. Dano a todos os inimigos.");
        return true;
    }
}