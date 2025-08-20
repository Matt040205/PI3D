using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Enemy/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    [Header("Status Básicos")]
    public float baseHP = 100f;
    public float baseATQ = 10f;
    public float moveSpeed = 3f;
    public float attackSpeed = 1f;

    [Header("Escala por Nível")]
    public float hpPerLevel = 10f;
    public float atqPerLevel = 2f;
    public float speedPerLevel = 0.5f;

    [Header("Recompensas")]
    public int geoditasOnDeath = 1;
    [Range(0f, 1f)]
    public float etherDropChance = 0.1f;

    // Métodos para calcular status baseado no nível
    public float GetHealth(int level) => baseHP + (level * hpPerLevel);
    public float GetDamage(int level) => baseATQ + (level * atqPerLevel);
    public float GetMoveSpeed(int level) => moveSpeed + (level * speedPerLevel);
}