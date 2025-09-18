using UnityEngine;

[CreateAssetMenu(fileName = "Caçadora Noturna", menuName = "ExoBeasts/Habilidades/Caçadora Noturna")]
public class HabilidadeCacadoraNoturna : Ability
{
    [Header("Configurações da Habilidade")]
    public float damage = 500f;
    public float radius = 1000f;

    [Tooltip("Arraste o prefab da lógica da habilidade aqui.")]
    public CacadoraNoturnaLogic logicPrefab;

    public override bool Activate(GameObject quemUsou)
    {
        if (logicPrefab == null)
        {
            Debug.LogError("O prefab da lógica da habilidade está NULO no ScriptableObject 'Caçadora Noturna'!");
            return true;
        }

        // Instancia a lógica da habilidade e a inicia
        CacadoraNoturnaLogic logic = Instantiate(logicPrefab, quemUsou.transform);
        logic.StartUltimateEffect(quemUsou, damage, radius);

        Debug.Log("Caçadora Noturna ativado. Dano a todos os inimigos.");
        return true;
    }
}