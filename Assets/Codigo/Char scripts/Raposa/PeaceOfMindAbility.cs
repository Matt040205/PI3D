using UnityEngine;

[CreateAssetMenu(fileName = "Paz de Espírito", menuName = "ExoBeasts/Personagens/Raposa/Habilidade/Paz de Espírito")]
public class PeaceOfMindAbility : Ability
{
    [Header("Ingredientes da Cura")]
    public float totalHeal = 80f;
    public float duration = 3f;

    public override bool Activate(GameObject quemUsou)
    {
        PeaceOfMindLogic ajudante = quemUsou.AddComponent<PeaceOfMindLogic>();
        ajudante.StartEffect(totalHeal, duration);
        return true;
    }
}