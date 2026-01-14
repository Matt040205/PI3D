using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class Espinhos : TrapLogicBase
{
    public float dano = 25f;
    public float tempoRecarga = 3f;
    public float tempoAtivo = 1f;

    private List<EnemyHealthSystem> inimigosNaArea = new List<EnemyHealthSystem>();
    public Animator animatorEspinhos;

    void Start()
    {
        GetComponent<BoxCollider>().isTrigger = true;
        StartCoroutine(CicloEspinhos());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealthSystem saudeInimigo = other.GetComponent<EnemyHealthSystem>();
            if (saudeInimigo != null && !inimigosNaArea.Contains(saudeInimigo))
            {
                inimigosNaArea.Add(saudeInimigo);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealthSystem saudeInimigo = other.GetComponent<EnemyHealthSystem>();
            if (saudeInimigo != null && inimigosNaArea.Contains(saudeInimigo))
            {
                inimigosNaArea.Remove(saudeInimigo);
            }
        }
    }

    private System.Collections.IEnumerator CicloEspinhos()
    {
        while (true)
        {
            yield return new WaitForSeconds(tempoRecarga);

            if (animatorEspinhos != null)
            {
                animatorEspinhos.SetTrigger("Ativar");
            }

            AplicarDano();

            yield return new WaitForSeconds(tempoAtivo);

            if (animatorEspinhos != null)
            {
                animatorEspinhos.SetTrigger("Desativar");
            }
        }
    }

    private void AplicarDano()
    {
        for (int i = inimigosNaArea.Count - 1; i >= 0; i--)
        {
            if (inimigosNaArea[i] == null)
            {
                inimigosNaArea.RemoveAt(i);
                continue;
            }
            inimigosNaArea[i].TakeDamage(dano);
        }
    }
}