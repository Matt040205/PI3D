// AssaultBehavior.cs
using UnityEngine;

/// <summary>
/// Aumenta a velocidade de ataque temporariamente a cada acerto crítico, de forma cumulativa.
/// </summary>
public class AssaultBehavior : TowerBehavior
{
    [Header("Configurações do Assalto")]
    [Tooltip("Bônus de vel. de ataque por acúmulo (0.1 = 10%).")]
    public float attackSpeedBonus = 0.1f;
    public int maxStacks = 3;
    [Tooltip("Por quanto tempo o buff dura, em segundos.")]
    public float buffDuration = 5f;

    private int currentStacks = 0;
    private float buffTimer = 0f;

    public override void Initialize(TowerController owner)
    {
        base.Initialize(owner);
        if (towerController != null)
        {
            // Se conecta ao evento que criamos para acertos críticos
            towerController.OnCriticalHit += HandleCriticalHit;
        }
    }

    // O método Update é chamado a cada frame, perfeito para um temporizador.
    private void Update()
    {
        // Se o buff está ativo (timer > 0)...
        if (buffTimer > 0)
        {
            // ...diminuímos o tempo restante.
            buffTimer -= Time.deltaTime;

            // Se o tempo acabou, resetamos tudo.
            if (buffTimer <= 0)
            {
                ResetBuff();
            }
        }
    }

    private void HandleCriticalHit(EnemyHealthSystem target)
    {
        // Se ainda não atingimos o máximo de acúmulos...
        if (currentStacks < maxStacks)
        {
            // ...aumentamos um acúmulo...
            currentStacks++;
            // ...e pedimos ao TowerController para adicionar o bônus.
            towerController.AddAttackSpeedBonus(attackSpeedBonus);
            Debug.Log($"ASSALTO! Acúmulo {currentStacks}/{maxStacks}. Bônus de vel. de ataque aumentado!");
        }

        // A cada novo crítico, reiniciamos a duração total do buff.
        buffTimer = buffDuration;
    }

    private void ResetBuff()
    {
        Debug.Log("Buff de Assalto expirou. Removendo bônus.");
        // Pedimos ao TowerController para remover o bônus total que foi aplicado.
        // (currentStacks * attackSpeedBonus) calcula o bônus total, e o sinal de menos o remove.
        towerController.AddAttackSpeedBonus(-(currentStacks * attackSpeedBonus));

        // Zeramos nossas variáveis de controle.
        currentStacks = 0;
        buffTimer = 0f;
    }

    // Este método é importante para evitar erros se a torre for destruída ou vendida com o buff ativo.
    private void OnDestroy()
    {
        if (towerController != null)
        {
            // Desconecta do evento para não causar erros.
            towerController.OnCriticalHit -= HandleCriticalHit;

            // Se o buff ainda estava ativo, garante que ele seja removido.
            if (currentStacks > 0)
            {
                ResetBuff();
            }
        }
    }
}