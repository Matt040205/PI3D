// PassiveAbility.cs
using UnityEngine;

// Um molde para habilidades que não são ativadas, mas aplicam um efeito.
public abstract class PassivaAbility : ScriptableObject
{
    public string abilityName;
    [TextArea] public string description;
    public Sprite icon;

    // Método chamado quando a passiva é equipada/inicia o jogo
    public abstract void OnEquip(GameObject owner);

    // Método chamado quando a passiva é desequipada/termina o jogo
    public abstract void OnUnequip(GameObject owner);
}