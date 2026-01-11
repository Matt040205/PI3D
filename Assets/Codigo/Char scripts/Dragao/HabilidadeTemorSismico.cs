using UnityEngine;
using FMODUnity;

[CreateAssetMenu(fileName = "Temor Sismico", menuName = "ExoBeasts/Personagens/Dragao/Habilidade/Temor Sismico")]
public class HabilidadeTemorSismico : Ability
{
    [Header("Configurações")]
    public float range = 15f;
    public float angle = 45f; // Ângulo do cone
    public float damage = 100f;
    public float knockUpDuration = 2f;
    public float knockUpForce = 12f;

    [Header("Lógica")]
    public TemorSismicoLogic logicPrefab;

    [Header("FMOD")]
    [EventRef]
    public string sfxSlam = "event:/SFX/SeismicSlam";

    public override bool Activate(GameObject quemUsou)
    {
        if (logicPrefab == null) return true;

        CommanderAbilityController controller = quemUsou.GetComponent<CommanderAbilityController>();

        if (!string.IsNullOrEmpty(sfxSlam))
        {
            RuntimeManager.PlayOneShot(sfxSlam, quemUsou.transform.position);
        }

        TemorSismicoLogic logic = Instantiate(logicPrefab, quemUsou.transform.position, quemUsou.transform.rotation);
        logic.Setup(quemUsou, range, angle, damage, knockUpDuration, knockUpForce, controller, this);

        return true;
    }
}