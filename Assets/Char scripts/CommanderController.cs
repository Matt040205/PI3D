using UnityEngine;

public class CommanderController : MonoBehaviour
{
    [Header("Dados")]
    public CharacterBase characterData;

    // Valores em tempo real (modific�veis por upgrades)
    [HideInInspector] public float currentHealth;
    [HideInInspector] public int currentAmmo;

    void Start()
    {
        // Inicializa com valores do SO
        currentHealth = characterData.maxHealth;
        currentAmmo = characterData.magazineSize;

        // Aplica velocidade do SO
        //GetComponent<PlayerMovement>().moveSpeed = characterData.moveSpeed;
    }

}