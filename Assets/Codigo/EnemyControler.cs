using UnityEngine;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    [Header("Status do Inimigo")]
    public float baseHP = 100f;
    public float baseATQ = 10f;
    public float baseSpeed = 3f;
    public int nivel = 1;

    private float currentHP;
    private float currentATQ;
    private float currentSpeed;

    [Header("Pontos de Patrulha")]
    public List<Transform> patrolPoints; // O Hordemanager vai preencher essa lista
    private int currentPointIndex = 0;
    private bool returningToPatrol = false;
    private Vector3 lastPatrolPosition;

    [Header("Perseguição")]
    public float chaseDistance = 50f;
    private Transform target;
    private float initialHP;

    private void OnEnable() // Usamos OnEnable para ser chamado quando o inimigo é ativado do pool
    {
        // Calcula e define os status baseados no nível
        currentHP = baseHP + (nivel * 10f);
        currentATQ = baseATQ + (nivel * 2f);
        currentSpeed = baseSpeed + (nivel * 0.5f);
        initialHP = currentHP;

        // Reseta o estado do inimigo
        currentPointIndex = 0;
        returningToPatrol = false;
        target = null;
    }

    private void Update()
    {
        if (target != null)
        {
            ChaseTarget();
        }
        else
        {
            Patrol();
        }
    }

    // O resto do código permanece o mesmo...

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Count == 0) return; // VERIFICAÇÃO ADICIONADA

        // ... o resto do código da patrulha
        if (returningToPatrol)
        {
            float distanceToLastPoint = Vector3.Distance(transform.position, lastPatrolPosition);
            if (distanceToLastPoint > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, lastPatrolPosition, currentSpeed * Time.deltaTime);
            }
            else
            {
                returningToPatrol = false;
            }
            return;
        }

        Transform currentPoint = patrolPoints[currentPointIndex];
        transform.position = Vector3.MoveTowards(transform.position, currentPoint.position, currentSpeed * Time.deltaTime);

        float distanceToPoint = Vector3.Distance(transform.position, currentPoint.position);
        if (distanceToPoint < 0.1f)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
        }
    }

    // ... e o resto do script

    private void ChaseTarget()
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget > chaseDistance)
        {
            ForgetTarget();
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, currentSpeed * Time.deltaTime);
    }

    public void TakeDamage(float damageAmount, Transform attacker)
    {
        currentHP -= damageAmount;

        if (target == null && (initialHP - currentHP) / initialHP >= 0.2f)
        {
            lastPatrolPosition = transform.position;
            target = attacker;
            returningToPatrol = false;
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void ForgetTarget()
    {
        target = null;
        returningToPatrol = true;
    }

    private void Die()
    {
        EnemyPoolManager.Instance.ReturnToPool(this.gameObject);
        Debug.Log("Inimigo " + name + " foi derrotado e retornado ao pool!");
    }
}