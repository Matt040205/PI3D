// NineTailsDanceLogic.cs
using UnityEngine;
using System.Collections;

// Componente temporário que gerencia o estado de fúria da ultimate.
public class NineTailsDanceLogic : MonoBehaviour
{
    // Referências para os sistemas que vamos manipular
    private PlayerCombatManager combatManager;
    private PlayerShooting shootingSystem;
    private MeleeCombatSystem meleeSystem;

    // Método de inicialização chamado pela "receita"
    public void StartEffect(float duration)
    {
        // Pega as referências dos componentes no jogador
        combatManager = GetComponent<PlayerCombatManager>();
        shootingSystem = GetComponent<PlayerShooting>();
        meleeSystem = GetComponent<MeleeCombatSystem>();

        // Garante que todas as referências foram encontradas antes de iniciar
        if (combatManager != null && shootingSystem != null && meleeSystem != null)
        {
            StartCoroutine(UltimateCoroutine(duration));
        }
        else
        {
            Debug.LogError("Não foi possível encontrar todos os componentes necessários para a Ultimate. Abortando.");
            Destroy(this); // Se autodestrói se algo estiver faltando
        }
    }

    private IEnumerator UltimateCoroutine(float duration)
    {
        // --- INÍCIO DA ULTIMATE ---
        Debug.Log("ULTIMATE ATIVADA: Dança das Nove Caudas!");

        // 1. Desativa o gerenciador de combate para assumirmos o controle manual
        combatManager.enabled = false;

        // 2. Troca para o modo de combate corpo a corpo
        shootingSystem.enabled = false;
        meleeSystem.enabled = true;

        // 3. Concede imunidade (se você tiver esse sistema)
        // GetComponent<StatusController>().isImmune = true;

        // 4. Espera a duração da ultimate acabar
        yield return new WaitForSeconds(duration);

        // --- FIM DA ULTIMATE ---
        Debug.Log("Ultimate finalizada.");

        // 5. Reverte a imunidade
        // GetComponent<StatusController>().isImmune = false;

        // 6. Reativa o gerenciador de combate para ele reassumir o controle
        // O gerenciador vai automaticamente colocar o modo de combate correto
        // baseado no CharacterBase do personagem.
        combatManager.enabled = true;

        // 7. O ajudante cumpriu sua missão. Ele se autodestrói.
        Destroy(this);
    }
}