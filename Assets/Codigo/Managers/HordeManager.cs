using UnityEngine;
using System.Collections.Generic;

public class HordeManager : MonoBehaviour
{
    [Header("Configurações da Horda")]
    public int enemiesPerHordeMin = 5;
    public int enemiesPerHordeMax = 10;
    public int victoryHorde = 5;

    [Header("Pontos de Spawn")]
    public List<Transform> spawnPoints;
    private int lastSpawnPointIndex = -1;

    [Header("Pontos de Patrulha")]
    public List<Transform> patrolPoints; // NOVA LISTA DE PONTOS DE PATRULHA

    [Header("Status da Horda")]
    public int currentHorde = 0;
    public int enemyLevel = 1;

    private List<GameObject> aliveEnemies = new List<GameObject>();
    private bool waveIsActive = false;

    void Start()
    {
        StartNextHorde();
    }

    void Update()
    {
        if (waveIsActive)
        {
            CheckForRemainingEnemies();

            if (aliveEnemies.Count == 0)
            {
                waveIsActive = false;
                Debug.Log("Horda " + currentHorde + " concluída!");

                if (currentHorde >= victoryHorde)
                {
                    Debug.Log("Parabéns! Você venceu o jogo!");
                }
                else
                {
                    Invoke("StartNextHorde", 5f);
                }
            }
        }
    }

    void CheckForRemainingEnemies()
    {
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] == null || !aliveEnemies[i].activeInHierarchy)
            {
                aliveEnemies.RemoveAt(i);
            }
        }
    }

    void StartNextHorde()
    {
        currentHorde++;
        Debug.Log("Iniciando Horda " + currentHorde);

        enemyLevel = currentHorde;
        SpawnEnemies();
        waveIsActive = true;
    }

    void SpawnEnemies()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogError("Faltam pontos de spawn!");
            return;
        }

        if (patrolPoints.Count == 0) // Verifica se os pontos de patrulha existem
        {
            Debug.LogError("Faltam pontos de patrulha!");
            return;
        }

        int enemiesToSpawn = Random.Range(enemiesPerHordeMin, enemiesPerHordeMax + 1);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            int spawnPointIndex = GetRandomSpawnPointIndex();
            Transform spawnPoint = spawnPoints[spawnPointIndex];

            GameObject newEnemy = EnemyPoolManager.Instance.GetPooledEnemy();
            newEnemy.transform.position = spawnPoint.position;
            newEnemy.transform.rotation = spawnPoint.rotation;

            EnemyController enemyController = newEnemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                // AQUI ESTÁ A MUDANÇA: passa a nova lista de pontos de patrulha
                enemyController.patrolPoints = this.patrolPoints;
                enemyController.nivel = enemyLevel;
            }

            aliveEnemies.Add(newEnemy);
        }
    }

    int GetRandomSpawnPointIndex()
    {
        int newIndex;
        do
        {
            newIndex = Random.Range(0, spawnPoints.Count);
        } while (newIndex == lastSpawnPointIndex && spawnPoints.Count > 1);

        lastSpawnPointIndex = newIndex;
        return newIndex;
    }
}