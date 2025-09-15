// PeaceOfMindLogic.cs
using UnityEngine;
using System.Collections;

// Este é um componente temporário que existirá no jogador
// apenas enquanto a cura estiver ativa.
public class PeaceOfMindLogic : MonoBehaviour
{
    private PlayerHealthSystem healthSystem;

    // Método de inicialização que a "receita" vai chamar
    public void StartEffect(float totalHeal, float duration)
    {
        healthSystem = GetComponent<PlayerHealthSystem>();
        if (healthSystem != null)
        {
            StartCoroutine(HealCoroutine(totalHeal, duration));
        }
        else
        {
            // Se não encontrar o sistema de vida, se destrói para não ficar "solto"
            Destroy(this);
        }
    }

    private IEnumerator HealCoroutine(float totalHeal, float duration)
    {
        float healPerSecond = totalHeal / duration;
        float timeLeft = duration;

        while (timeLeft > 0)
        {
            healthSystem.Heal(healPerSecond * Time.deltaTime);
            timeLeft -= Time.deltaTime;
            yield return null; // Espera o próximo frame
        }

        // A tarefa acabou. Agora o ajudante se autodestrói.
        Destroy(this);
    }
}