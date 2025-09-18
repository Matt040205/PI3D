using UnityEngine;

[CreateAssetMenu(fileName = "Perseguindo as Presas", menuName = "ExoBeasts/Habilidades/Perseguindo as Presas")]
public class HabilidadePerseguindoPresas : Ability
{
    [Header("Configura��es da Habilidade")]
    public float markDuration = 5f;
    public float bonusDamageMultiplier = 1.25f;

    [Tooltip("Arraste o prefab da l�gica da habilidade aqui.")]
    public PreyMarkLogic logicPrefab;

    public override bool Activate(GameObject quemUsou)
    {
        if (logicPrefab == null)
        {
            Debug.LogError("O prefab da l�gica da habilidade est� NULO no ScriptableObject 'Perseguindo as Presas'!");
            return true; // Ainda coloca em cooldown para evitar spam, mas avisa sobre a falha.
        }

        PreyMarkLogic logic = Instantiate(logicPrefab, quemUsou.transform);
        logic.StartEffect(markDuration, bonusDamageMultiplier);

        Debug.Log("Perseguindo as Presas ativado. Inimigos marcados e recebem mais dano.");
        return true;
    }
}