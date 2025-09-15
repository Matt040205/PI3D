// CuttingBladeAbility.cs
using UnityEngine;

// [CreateAssetMenu] nos permite criar essa "receita" como um arquivo no Unity.
[CreateAssetMenu(fileName = "Lâmina Cortante", menuName = "ExoBeasts/Habilidades/Lâmina Cortante")]
public class CuttingBladeAbility : Ability // Dizemos que esta é uma receita que usa nosso molde 'Ability'
{
    [Header("Ingredientes da Lâmina")]
    public float dashDistance = 7f;
    public float damage = 60f;

    // Aqui está o "Modo de Preparo" específico para esta habilidade.
    public override bool Activate(GameObject quemUsou)
    {
        // Pegamos componentes do personagem que usou a habilidade
        CharacterController controller = quemUsou.GetComponent<CharacterController>();
        Transform modelPivot = quemUsou.GetComponent<PlayerMovement>().GetModelPivot();

        // 1. A Lógica do Dash
        controller.Move(modelPivot.forward * dashDistance);

        // 2. A Lógica do Dano (procuramos inimigos perto)
        Collider[] inimigosAcertados = Physics.OverlapSphere(quemUsou.transform.position, 2f);
        foreach (var inimigo in inimigosAcertados)
        {
            EnemyHealthSystem vidaInimigo = inimigo.GetComponent<EnemyHealthSystem>();
            if (vidaInimigo != null)
            {
                // Para isto funcionar, seu script de vida do inimigo precisa nos
                // dizer se o dano foi fatal.
                bool inimigoMorreu = vidaInimigo.TakeDamage(damage);

                if (inimigoMorreu)
                {
                    Debug.Log("Inimigo abatido! Habilidade resetada.");
                    return false; // Retorna FALSE: NÃO entre em cooldown!
                }
                else
                {
                    // Se acertou mas não matou, entra em cooldown e para de procurar.
                    return true; // Retorna TRUE: entre em cooldown.
                }
            }
        }

        // Se o dash não acertou nenhum inimigo, a habilidade entra em cooldown normal.
        return true;
    }
}