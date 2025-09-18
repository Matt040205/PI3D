using UnityEngine;

[CreateAssetMenu(fileName = "Voo Gracioso", menuName = "ExoBeasts/Habilidades/Voo Gracioso")]
public class HabilidadeVooGracioso : Ability
{
    [Header("Configura��es da Habilidade")]
    public float jumpHeightModifier = 1.5f;
    public float staticAimDuration = 3f;
    public float bonusDamageMultiplier = 1.5f;
    public float bonusAreaMultiplier = 1.2f;

    [Tooltip("Arraste o prefab da l�gica da habilidade aqui.")]
    public VooGraciosoLogic logicPrefab;

    public override bool Activate(GameObject quemUsou)
    {
        if (logicPrefab == null)
        {
            Debug.LogError("O prefab da l�gica da habilidade est� NULO no ScriptableObject 'Voo Gracioso'!");
            return true;
        }

        VooGraciosoLogic logic = Instantiate(logicPrefab, quemUsou.transform);
        logic.StartEffect(quemUsou, jumpHeightModifier, staticAimDuration, bonusDamageMultiplier, bonusAreaMultiplier);

        Debug.Log("Voo Gracioso ativado. Pr�xima flecha causa mais dano e �rea.");
        return true;
    }
}