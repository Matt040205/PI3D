// TowerController.cs (Com depuração extensiva)
using UnityEngine;
using System;
using System.Linq;

public class TowerController : MonoBehaviour
{
    [Header("Referências")]
    public CharacterBase towerData;
    public Transform partToRotate;

    [Header("Configurações de IA")]
    [SerializeField] private string enemyTag = "Enemy";

    public event Action<Transform> OnAttack;

    private float currentDamage;
    private float currentAttackSpeed;
    private float currentRange;
    private int[] currentPathLevels;

    private Transform targetEnemy;
    private float fireCountdown = 0f;

    void Start()
    {
        Debug.Log("--- DEBUG: Torre '" + gameObject.name + "' iniciou o script TowerController. ---");

        if (towerData == null)
        {
            Debug.LogError("DEBUG FALHA: TowerData está NULO na torre '" + gameObject.name + "'! A torre não vai funcionar.", this.gameObject);
            this.enabled = false;
            return;
        }

        currentDamage = towerData.damage;
        currentAttackSpeed = towerData.attackSpeed;
        currentRange = towerData.meleeRange;
        currentPathLevels = new int[towerData.upgradePaths.Count];
        Debug.Log("DEBUG: Status da torre '" + towerData.name + "' carregados: Dano=" + currentDamage + ", Alcance=" + currentRange + ", Vel. Ataque=" + currentAttackSpeed);

        InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }

    void Update()
    {
        if (targetEnemy == null) return;

        if (partToRotate != null)
        {
            RotateTowardsTarget();
        }
        else
        {
            // Este aviso só aparecerá uma vez por frame
            Debug.LogWarning("DEBUG AVISO: A variável 'Part To Rotate' não está definida na torre '" + gameObject.name + "'. A torre não consegue mirar.", this.gameObject);
        }

        fireCountdown -= Time.deltaTime;
        if (fireCountdown <= 0f)
        {
            fireCountdown = 1f / currentAttackSpeed;
            Shoot();
        }
    }

    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        if (enemies.Length == 0)
        {
            targetEnemy = null;
            return; // Sai se não houver inimigos
        }

        Transform nearestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy.transform;
            }
        }

        if (nearestEnemy != null && shortestDistance <= currentRange)
        {
            if (targetEnemy != nearestEnemy)
            {
                Debug.Log("DEBUG: Torre '" + gameObject.name + "' encontrou um novo alvo: " + nearestEnemy.name + " a " + shortestDistance + "m de distância.");
                targetEnemy = nearestEnemy;
            }
        }
        else
        {
            if (targetEnemy != null)
            {
                Debug.Log("DEBUG: Torre '" + gameObject.name + "' perdeu o alvo (fora de alcance).");
                targetEnemy = null;
            }
        }
    }

    void RotateTowardsTarget()
    {
        Vector3 direction = targetEnemy.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * 10f).eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);
    }

    void Shoot()
    {
        Debug.Log("--- DEBUG: Torre '" + gameObject.name + "' ATIROU no alvo '" + targetEnemy.name + "'! ---");

        EnemyHealthSystem healthSystem = targetEnemy.GetComponent<EnemyHealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.TakeDamage(currentDamage);
        }

        OnAttack?.Invoke(targetEnemy);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, currentRange);
    }

    // O resto do código de upgrades...
    // (cole aqui o resto do seu TowerController se ele for diferente)
}