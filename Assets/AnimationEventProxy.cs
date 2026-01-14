using UnityEngine;

public class AnimationEventProxy : MonoBehaviour
{
    // Referência para o script do Samurai
    private MeleeCombatSystem meleeSystem;

    void Start()
    {
        // Tenta achar o script do Samurai no "pai"
        meleeSystem = GetComponentInParent<MeleeCombatSystem>();
    }

    // --- Funções do Samurai ---

    public void AnimEvent_Hit1()
    {
        if (meleeSystem != null && meleeSystem.enabled)
        {
            meleeSystem.AnimEvent_Hit1();
        }
    }

    public void AnimEvent_Hit2()
    {
        if (meleeSystem != null && meleeSystem.enabled)
        {
            meleeSystem.AnimEvent_Hit2();
        }
    }

    public void AnimEvent_Hit3()
    {
        if (meleeSystem != null && meleeSystem.enabled)
        {
            meleeSystem.AnimEvent_Hit3();
        }
    }

    public void AnimEvent_Hit4()
    {
        if (meleeSystem != null && meleeSystem.enabled)
        {
            meleeSystem.AnimEvent_Hit4();
        }
    }

    // --- !! NOVA FUNÇÃO DA CAÇADORA !! ---

    public void AnimEvent_FireBeam()
    {
        // Como o script da Caçadora é temporário, buscamos ele no "pai"
        // toda vez que o evento é chamado.
        CacadoraNoturnaLogic cacadoraLogic = GetComponentInParent<CacadoraNoturnaLogic>();

        if (cacadoraLogic != null && cacadoraLogic.enabled)
        {
          //cacadoraLogic.AnimEvent_FireBeam();
        }
        else
        {
            // Este log é importante. Se você o vir, algo ainda está errado.
            Debug.LogWarning("Proxy recebeu AnimEvent_FireBeam, mas não achou o 'CacadoraNoturnaLogic' no pai!");
        }
    }
}