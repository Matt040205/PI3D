using UnityEngine;

[CreateAssetMenu(fileName = "Ca�adora Noturna", menuName = "ExoBeasts/Habilidades/Ca�adora Noturna")]
public class HabilidadeCacadoraNoturna : Ability
{
    [Header("Configura��es da Habilidade")]
    public float damage = 500f; // Dano da ultimate
    public float radius = 1000f; // Raio global (ou muito grande)

    public override bool Activate(GameObject quemUsou)
    {
        // L�gica para disparar um proj�til ou efeito global.
        // Causar dano a todos os inimigos no mapa.
        Debug.Log("Ca�adora Noturna ativado. Dano a todos os inimigos.");
        return true;
    }
}