using UnityEngine;
using FMODUnity;

[CreateAssetMenu(fileName = "Perseguindo as Presas", menuName = "ExoBeasts/Personagens/Coruja/Habilidade/Perseguindo as Presas")]
public class HabilidadePerseguindoPresas : Ability
{
    [Header("Configurações da Habilidade")]
    public float markDuration = 5f;
    public float bonusDamageMultiplier = 1.25f;

    [Tooltip("Arraste o prefab da lógica da habilidade aqui.")]
    public PreyMarkLogic logicPrefab;

    [Header("FMOD")]
    [EventRef]
    public string eventoTEC = "event:/SFX/TEC";

    public override bool Activate(GameObject quemUsou)
    {
        if (logicPrefab == null)
        {
            Debug.LogError("O prefab da lógica da habilidade está NULO no ScriptableObject 'Perseguindo as Presas'!");
            return true;
        }

        if (!string.IsNullOrEmpty(eventoTEC))
        {
            RuntimeManager.PlayOneShot(eventoTEC, quemUsou.transform.position);
        }

        PreyMarkLogic logic = Instantiate(logicPrefab, quemUsou.transform);
        logic.StartEffect(markDuration, bonusDamageMultiplier);

        Debug.Log("Perseguindo as Presas ativado. Inimigos marcados e recebem mais dano.");
        return true;
    }
}