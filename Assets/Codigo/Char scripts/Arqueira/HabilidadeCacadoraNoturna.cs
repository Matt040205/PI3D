using UnityEngine;

[CreateAssetMenu(fileName = "Ca�adora Noturna", menuName = "ExoBeasts/Habilidades/Ca�adora Noturna")]
public class HabilidadeCacadoraNoturna : Ability
{
    [Header("Configura��es da Habilidade")]
    public float damage = 500f;
    public float radius = 1000f;

    [Tooltip("Arraste o prefab da l�gica da habilidade aqui.")]
    public CacadoraNoturnaLogic logicPrefab;

    public override bool Activate(GameObject quemUsou)
    {
        if (logicPrefab == null)
        {
            Debug.LogError("O prefab da l�gica da habilidade est� NULO no ScriptableObject 'Ca�adora Noturna'!");
            return true;
        }

        // Instancia a l�gica da habilidade e a inicia
        CacadoraNoturnaLogic logic = Instantiate(logicPrefab, quemUsou.transform);
        logic.StartUltimateEffect(quemUsou, damage, radius);

        Debug.Log("Ca�adora Noturna ativado. Dano a todos os inimigos.");
        return true;
    }
}