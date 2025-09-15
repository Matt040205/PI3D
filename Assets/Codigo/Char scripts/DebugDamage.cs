using UnityEngine;

public class DebugDamage : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            GetComponent<PlayerHealthSystem>().TakeDamage(10);
            Debug.Log("Dano aplicado!");
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            GetComponent<PlayerHealthSystem>().currentHealth += 20;
            Debug.Log("Cura aplicada!");
        }
    }
}