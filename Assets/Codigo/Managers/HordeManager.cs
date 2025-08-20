using UnityEngine;
using System.Collections.Generic;

public class HordeManager : MonoBehaviour
{
    [Header("Configurações da Horda")]
    public int enemiesPerHordeMin = 5;
    public int enemiesPerHordeMax = 10;
    public int victoryHorde = 5;

    [Header("Dados dos Inimigos")]
    public EnemyDataSO[] enemyTypes;

    // MODIFICADO: Substituímos as listas antigas por uma única lista de rotas.
    [Header("Configuração das Rotas")]
    public List<SpawnPath> spawnPaths;
    private int lastPathIndex = -1;

    [Header("Status da Horda")]
    public int currentHorde = 0;
    public int enemyLevel = 1;

    private List<GameObject> aliveEnemies = new List<GameObject>();
    private bool waveIsActive = false;

    void Start()
    {
        if (EnemyPoolManager.Instance == null)
        {
            Debug.LogError("EnemyPoolManager não encontrado na cena!");
            return;
        }
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

    // MODIFICADO: A lógica de Spawn agora usa a lista de SpawnPath
    void SpawnEnemies()
    {
        if (spawnPaths == null || spawnPaths.Count == 0)
        {
            Debug.LogError("Nenhuma rota (SpawnPath) configurada!");
            return;
        }
        if (enemyTypes.Length == 0)
        {
            Debug.LogError("Faltam tipos de inimigos configurados!");
            return;
        }

        int enemiesToSpawn = Random.Range(enemiesPerHordeMin, enemiesPerHordeMax + 1);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            // 1. Seleciona uma rota aleatória (caminho A, caminho B, etc.)
            int pathIndex = GetRandomPathIndex();
            SpawnPath selectedPath = spawnPaths[pathIndex];

            // Validação para garantir que a rota selecionada está bem configurada
            if (selectedPath.spawnPoint == null || selectedPath.patrolPoints == null || selectedPath.patrolPoints.Count == 0)
            {
                Debug.LogWarning("A rota '" + selectedPath.pathName + "' não está configurada corretamente. Pulando este spawn.");
                continue; // Pula para o próximo inimigo
            }

            // 2. Seleciona um tipo de inimigo aleatório
            int enemyTypeIndex = Random.Range(0, enemyTypes.Length);
            EnemyDataSO enemyData = enemyTypes[enemyTypeIndex];

            // 3. Pega um inimigo do pool e o posiciona
            GameObject newEnemy = EnemyPoolManager.Instance.GetPooledEnemy();
            newEnemy.transform.position = selectedPath.spawnPoint.position;
            newEnemy.transform.rotation = selectedPath.spawnPoint.rotation;

            // 4. Configura o inimigo com os dados da ROTA SELECIONADA
            EnemyController enemyController = newEnemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.enemyData = enemyData;
                enemyController.SetPatrolPoints(selectedPath.patrolPoints); // <-- A MUDANÇA PRINCIPAL!
                enemyController.nivel = enemyLevel;
            }

            EnemyHealthSystem healthSystem = newEnemy.GetComponent<EnemyHealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.enemyData = enemyData;
                healthSystem.InitializeHealth(enemyLevel);
            }

            aliveEnemies.Add(newEnemy);
        }
    }

    // MODIFICADO: Renomeado para refletir que estamos escolhendo uma rota, não só um ponto.
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