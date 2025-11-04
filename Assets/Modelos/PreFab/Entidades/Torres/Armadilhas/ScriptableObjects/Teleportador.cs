using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class Teleportador : TrapLogicBase
{
    private static List<Teleportador> portais = new List<Teleportador>();
    private const int MAX_PORTAIS = 2;

    private Teleportador portalLigado;
    private bool podeTeleportar = true;
    private float cooldownTeleporte = 1f;
    private float entradaOffset = 1.5f;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;

        if (portais.Count >= MAX_PORTAIS)
        {
            Debug.LogWarning("Não é possível colocar mais de 2 teleportadores. Destruindo este.");
            Destroy(gameObject);
            return;
        }

        portais.Add(this);
        LigarPortais();
    }

    void OnDestroy()
    {
        portais.Remove(this);
        if (portalLigado != null)
        {
            portalLigado.portalLigado = null;
        }
        LigarPortais();
    }

    public static int GetPortalCount()
    {
        return portais.Count;
    }

    private static void LigarPortais()
    {
        if (portais.Count == 1)
        {
            portais[0].portalLigado = null;
        }
        else if (portais.Count == 2)
        {
            portais[0].portalLigado = portais[1];
            portais[1].portalLigado = portais[0];
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (podeTeleportar && portalLigado != null)
        {
            if (other.CompareTag("Player") || other.CompareTag("Enemy"))
            {
                Vector3 posicaoDestino = portalLigado.transform.position + (portalLigado.transform.forward * entradaOffset);

                CharacterController cc = other.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.enabled = false;
                    other.transform.position = posicaoDestino;
                    cc.enabled = true;
                }
                else
                {
                    other.transform.position = posicaoDestino;
                }

                StartCoroutine(IniciarCooldown());
                StartCoroutine(portalLigado.IniciarCooldown());
            }
        }
    }

    private System.Collections.IEnumerator IniciarCooldown()
    {
        podeTeleportar = false;
        yield return new WaitForSeconds(cooldownTeleporte);
        podeTeleportar = true;
    }
}