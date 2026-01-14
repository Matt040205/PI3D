using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class HordeManager : MonoBehaviour
{
    [Header("Configurações da Horda")]
    public int enemiesPerHordeMin = 5;
    public int enemiesPerHordeMax = 10;
    public int victoryHorde = 5;

    [Header("Spawn de Inimigos")]
    public float spawnInterval = 1f;
    public int enemiesPerInterval = 1;

    [Header("Dados dos Inimigos")]
    public EnemyDataSO[] enemyTypes;

    [Header("Configuração das Rotas")]
    public List<SpawnPath> spawnPaths;
    private int lastPathIndex = -1;

    [Header("Status da Horda")]
    public int currentHorde = 0;
    public int enemyLevel = 1;

    [Header("UI")]
    public TextMeshProUGUI hordeText;
    public TextMeshProUGUI hordeTextBuild;

    private List<GameObject> aliveEnemies = new List<GameObject>();
    private bool waveIsActive = false;
    private Transform playerTransform;

    private int enemiesToSpawnTotal;
    private int enemiesSpawnedCount = 0;
    private Coroutine spawnCoroutine;

    void Start()
    {
        if (EnemyPoolManager.Instance == null)
        {
            Debug.LogError("EnemyPoolManager não encontrado na cena!");
            return;
        }

        StartCoroutine(FindPlayerAndBeginHorde());
    }

    private IEnumerator FindPlayerAndBeginHorde()
    {
        yield return null;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("HordeManager não conseguiu encontrar o Player! A IA do inimigo pode falhar.");
        }

        StartNextHorde();
    }

    void Update()
    {
        if (waveIsActive)
        {
            CheckForRemainingEnemies();

            bool allEnemiesSpawned = enemiesSpawnedCount >= enemiesToSpawnTotal;
            bool allAliveEnemiesDefeated = aliveEnemies.Count == 0;

            if (allEnemiesSpawned && allAliveEnemiesDefeated)
            {
                waveIsActive = false;

                if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);

                Debug.Log("Horda " + currentHorde + " concluída!");
                if (currentHorde >= victoryHorde)
                {
                    SceneManager.LoadScene("Win");
                }
                else
                {
                    Invoke("StartNextHorde", 5f);
                }
            }
        }
    }

    private void UpdateHordeUI()
    {
        if (hordeText != null && hordeTextBuild != null)
        {
            hordeTextBuild.text = $"{currentHorde}/{victoryHorde}";
            hordeText.text = $"{currentHorde}/{victoryHorde}";
        }
    }

    void StartNextHorde()
    {
        currentHorde++;
        enemyLevel = currentHorde;

        enemiesToSpawnTotal = Random.Range(enemiesPerHordeMin, enemiesPerHordeMax + 1);
        enemiesSpawnedCount = 0;

        if (enemiesToSpawnTotal > 0)
        {
            if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
            spawnCoroutine = StartCoroutine(SpawnEnemiesOverTime());
        }

        waveIsActive = true;
        UpdateHordeUI();
    }

    private IEnumerator SpawnEnemiesOverTime()
    {
        if (spawnPaths == null || spawnPaths.Count == 0)
        {
            Debug.LogError("Nenhuma rota (SpawnPath) configurada! Impossível spawnar inimigos.");
            yield break;
        }
        if (enemyTypes.Length == 0)
        {
            Debug.LogError("Faltam tipos de inimigos configurados! Impossível spawnar inimigos.");
            yield break;
        }

        while (enemiesSpawnedCount < enemiesToSpawnTotal)
        {
            int enemiesThisInterval = Mathf.Min(enemiesPerInterval, enemiesToSpawnTotal - enemiesSpawnedCount);

            for (int i = 0; i < enemiesThisInterval; i++)
            {
                SpawnSingleEnemy();
            }

            enemiesSpawnedCount += enemiesThisInterval;

            if (enemiesSpawnedCount < enemiesToSpawnTotal)
            {
                yield return new WaitForSeconds(spawnInterval);
            }
        }

    }

    void SpawnSingleEnemy()
    {
        int pathIndex = GetRandomPathIndex();
        SpawnPath selectedPath = spawnPaths[pathIndex];

        if (selectedPath.spawnPoint == null || selectedPath.patrolPoints == null || selectedPath.patrolPoints.Count == 0)
        {
            Debug.LogWarning("A rota '" + selectedPath.pathName + "' não está configurada corretamente. Inimigo não spawnado.");
            return;
        }

        int enemyTypeIndex = Random.Range(0, enemyTypes.Length);
        EnemyDataSO enemyData = enemyTypes[enemyTypeIndex];

        GameObject newEnemy = EnemyPoolManager.Instance.GetPooledEnemy();
        newEnemy.transform.position = selectedPath.spawnPoint.position;
        newEnemy.transform.rotation = selectedPath.spawnPoint.rotation;

        EnemyController enemyController = newEnemy.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.InitializeEnemy(playerTransform, selectedPath.patrolPoints, enemyData, enemyLevel);
        }

        aliveEnemies.Add(newEnemy);
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

    int GetRandomPathIndex()
    {
        if (spawnPaths.Count <= 1) return 0;
        int newIndex;
        do
        {
            newIndex = Random.Range(0, spawnPaths.Count);
        } while (newIndex == lastPathIndex);
        lastPathIndex = newIndex;
        return newIndex;
    }
}