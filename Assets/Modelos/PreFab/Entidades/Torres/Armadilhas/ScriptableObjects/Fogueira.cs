using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class Fogueira : TrapLogicBase
{
    public float curaPorSegundo = 5f;
    public float taxaDeCura = 1.0f;

    private List<Collider> alvosNaArea = new List<Collider>();
    private float tempoDesdeUltimaCura = 0f;

    private void Start()
    {
        GetComponent<SphereCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Tower"))
        {
            if (!alvosNaArea.Contains(other))
            {
                alvosNaArea.Add(other);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (alvosNaArea.Contains(other))
        {
            alvosNaArea.Remove(other);
        }
    }

    void Update()
    {
        tempoDesdeUltimaCura += Time.deltaTime;
        if (tempoDesdeUltimaCura >= taxaDeCura)
        {
            CurarAlvos();
            tempoDesdeUltimaCura = 0f;
        }
    }

    private void CurarAlvos()
    {
        float curaTick = curaPorSegundo * taxaDeCura;

        for (int i = alvosNaArea.Count - 1; i >= 0; i--)
        {
            Collider alvo = alvosNaArea[i];
            if (alvo == null)
            {
                alvosNaArea.RemoveAt(i);
                continue;
            }

            if (alvo.CompareTag("Player"))
            {
                PlayerHealthSystem saudeJogador = alvo.GetComponent<PlayerHealthSystem>();
                if (saudeJogador != null)
                {
                    saudeJogador.Heal(curaTick);
                }
            }
            else if (alvo.CompareTag("Tower"))
            {
                TowerController saudeTorre = alvo.GetComponent<TowerController>();
                if (saudeTorre != null)
                {
                    saudeTorre.Heal(curaTick);
                }
            }
        }
    }
}