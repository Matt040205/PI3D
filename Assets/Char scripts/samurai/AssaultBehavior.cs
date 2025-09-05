// AssaultBehavior.cs
using UnityEngine;

/// <summary>
/// Aumenta a velocidade de ataque temporariamente a cada acerto cr�tico, de forma cumulativa.
/// </summary>
public class AssaultBehavior : TowerBehavior
{
    [Header("Configura��es do Assalto")]
    [Tooltip("B�nus de vel. de ataque por ac�mulo (0.1 = 10%).")]
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
            // Se conecta ao evento que criamos para acertos cr�ticos
            towerController.OnCriticalHit += HandleCriticalHit;
        }
    }

    // O m�todo Update � chamado a cada frame, perfeito para um temporizador.
    private void Update()
    {
        // Se o buff est� ativo (timer > 0)...
        if (buffTimer > 0)
        {
            // ...diminu�mos o tempo restante.
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
        // Se ainda n�o atingimos o m�ximo de ac�mulos...
        if (currentStacks < maxStacks)
        {
            // ...aumentamos um ac�mulo...
            currentStacks++;
            // ...e pedimos ao TowerController para adicionar o b�nus.
            towerController.AddAttackSpeedBonus(attackSpeedBonus);
            Debug.Log($"ASSALTO! Ac�mulo {currentStacks}/{maxStacks}. B�nus de vel. de ataque aumentado!");
        }

        // A cada novo cr�tico, reiniciamos a dura��o total do buff.
        buffTimer = buffDuration;
    }

    private void ResetBuff()
    {
        Debug.Log("Buff de Assalto expirou. Removendo b�nus.");
        // Pedimos ao TowerController para remover o b�nus total que foi aplicado.
        // (currentStacks * attackSpeedBonus) calcula o b�nus total, e o sinal de menos o remove.
        towerController.AddAttackSpeedBonus(-(currentStacks * attackSpeedBonus));

        // Zeramos nossas vari�veis de controle.
        currentStacks = 0;
        buffTimer = 0f;
    }

    // Este m�todo � importante para evitar erros se a torre for destru�da ou vendida com o buff ativo.
    private void OnDestroy()
    {
        if (towerController != null)
        {
            // Desconecta do evento para n�o causar erros.
            towerController.OnCriticalHit -= HandleCriticalHit;

            // Se o buff ainda estava ativo, garante que ele seja removido.
            if (currentStacks > 0)
            {
                ResetBuff();
            }
        }
    }
}