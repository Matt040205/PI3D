using UnityEngine;
using System.Collections.Generic;

public class Piche : TrapLogicBase
{
    public float percentualDesaceleracao = 0.5f;
    public float multiplicadorDanoVulneravel = 1.5f;
    public float duracaoVulneravelAposSair = 3f;

    private Dictionary<EnemyController, Coroutine> inimigosAfetados = new Dictionary<EnemyController, Coroutine>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyController inimigo = other.GetComponent<EnemyController>();
            EnemyHealthSystem saudeInimigo = other.GetComponent<EnemyHealthSystem>();

            if (inimigo == null || saudeInimigo == null) return;

            if (inimigosAfetados.ContainsKey(inimigo))
            {
                StopCoroutine(inimigosAfetados[inimigo]);
                inimigosAfetados.Remove(inimigo);
            }

            inimigo.AplicarDesaceleracao(percentualDesaceleracao);
            saudeInimigo.AplicarVulnerabilidade(multiplicadorDanoVulneravel);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyController inimigo = other.GetComponent<EnemyController>();
            EnemyHealthSystem saudeInimigo = other.GetComponent<EnemyHealthSystem>();

            if (inimigo == null || saudeInimigo == null) return;

            inimigo.RemoverDesaceleracao();

            Coroutine rotinaVulneravel = StartCoroutine(ManterVulnerabilidadeTemporaria(saudeInimigo));
            inimigosAfetados[inimigo] = rotinaVulneravel;
        }
    }

    private System.Collections.IEnumerator ManterVulnerabilidadeTemporaria(EnemyHealthSystem saude)
    {
        if (saude == null) yield break;
        yield return new WaitForSeconds(duracaoVulneravelAposSair);
        if (saude != null)
        {
            saude.RemoverVulnerabilidade();
        }
    }
}