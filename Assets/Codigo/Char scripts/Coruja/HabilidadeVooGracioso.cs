using UnityEngine;

[CreateAssetMenu(fileName = "Voo Gracioso", menuName = "ExoBeasts/Personagens/Coruja/Habilidade/Voo Gracioso")]
public class HabilidadeVooGracioso : Ability
{
    [Header("Configurações da Habilidade")]
    public float jumpHeightModifier = 1.5f;
    public float staticAimDuration = 3f;
    public float bonusDamageMultiplier = 1.5f;
    public float bonusExplosionRadius = 5f;

    [Tooltip("Arraste o prefab da lógica da habilidade aqui.")]
    public VooGraciosoLogic logicPrefab;

    public override bool Activate(GameObject quemUsou)
    {
        if (logicPrefab == null)
        {
            Debug.LogError("O prefab da lógica da habilidade está NULO no ScriptableObject 'Voo Gracioso'!");
            return true;
        }

        VooGraciosoLogic logic = Instantiate(logicPrefab, quemUsou.transform);
        logic.StartEffect(quemUsou, jumpHeightModifier, staticAimDuration, bonusDamageMultiplier, bonusExplosionRadius);

        Debug.Log("Voo Gracioso ativado. Próxima flecha causa mais dano e explode.");
        return true;
    }
}