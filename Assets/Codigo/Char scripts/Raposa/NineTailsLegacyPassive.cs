using UnityEngine;

[CreateAssetMenu(fileName = "Legado das Nove Caudas", menuName = "ExoBeasts/Personagens/Raposa/Passiva/Legado das Nove Caudas")]
public class NineTailsLegacyPassive : PassivaAbility
{
    [Header("Ingredientes da Passiva")]
    public float attackSpeedBonus = 0.15f; // 15%

    public override void OnEquip(GameObject owner)
    {
        // Debug.Log("Passiva equipada: " + abilityName);
    }

    public override void OnUnequip(GameObject owner)
    {
        Debug.Log("Passiva desequipada: " + abilityName);
    }
}