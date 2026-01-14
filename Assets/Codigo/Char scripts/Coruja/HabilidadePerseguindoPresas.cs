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
            return true;
        }

        if (!string.IsNullOrEmpty(eventoTEC))
        {
            RuntimeManager.PlayOneShot(eventoTEC, quemUsou.transform.position);
        }

        CommanderAbilityController abilityController = quemUsou.GetComponent<CommanderAbilityController>();
        PreyMarkLogic logic = Instantiate(logicPrefab, quemUsou.transform);

        logic.StartEffect(markDuration, bonusDamageMultiplier, abilityController, this);

        return true;
    }
}