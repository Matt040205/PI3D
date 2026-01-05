using UnityEngine;
using UnityEngine.AI; // Necessário para acessar o NavMeshAgent dos inimigos

public class NuvemDeTintaLogic : MonoBehaviour
{
    [Header("Configurações Visuais")]
    [Tooltip("O Prefab do Sistema de Partículas (VFX) da fumaça colorida")]
    public GameObject vfxFumacaPrefab;

    [Header("Configurações de Debuff")]
    [Tooltip("Porcentagem de velocidade que o inimigo vai manter (0.7 = 70% da velocidade, ou seja, 30% de slow)")]
    public float slowFactor = 0.6f;

    private float _duration;

    // Chamado pela granada quando ela explode
    public void Setup(float duration)
    {
        _duration = duration;

        // 1. Spawna o VFX de fumaça como filho deste objeto
        if (vfxFumacaPrefab != null)
        {
            GameObject vfx = Instantiate(vfxFumacaPrefab, transform.position, Quaternion.identity, transform);
            // Garante que o VFX também seja destruído quando a nuvem sumir
        }

        // 2. Agenda a destruição da nuvem
        Destroy(gameObject, _duration);
    }

    // --- LÓGICA DE ENTRADA (Aplica Slow e Cegueira) ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // 1. Aplica Lentidão (Assumindo que usa NavMeshAgent)
            NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                // Guarda a velocidade original em algum lugar se precisar, 
                // ou apenas reduz a velocidade atual
                agent.speed *= slowFactor;
            }

            // 2. Aplica Cegueira (Envia mensagem para o script do inimigo)
            // O inimigo precisa ter uma função "SetBlinded(bool)" ou similar
            other.SendMessage("SetBlinded", true, SendMessageOptions.DontRequireReceiver);

            Debug.Log($"{other.name} entrou na fumaça (Lento e Cego)");
        }
    }

    // --- LÓGICA DE SAÍDA (Remove Slow e Cegueira) ---
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // 1. Remove Lentidão (Restaura velocidade)
            NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                // Restaura dividindo pelo fator (Matemática inversa)
                agent.speed /= slowFactor;
            }

            // 2. Remove Cegueira
            other.SendMessage("SetBlinded", false, SendMessageOptions.DontRequireReceiver);

            Debug.Log($"{other.name} saiu da fumaça (Normal)");
        }
    }

    // Caso a nuvem suma com o inimigo dentro, precisamos restaurar os status
    private void OnDestroy()
    {
        // Cria uma esfera final para pegar quem ainda está dentro e limpar os status
        // (Isso evita que o inimigo fique lento para sempre se a nuvem sumir)
        Collider[] hits = Physics.OverlapSphere(transform.position, transform.localScale.x / 2); // Raio aproximado
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                NavMeshAgent agent = hit.GetComponent<NavMeshAgent>();
                if (agent != null) agent.speed /= slowFactor; // Restaura

                hit.SendMessage("SetBlinded", false, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}