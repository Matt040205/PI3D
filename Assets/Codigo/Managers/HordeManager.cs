using UnityEngine;
using TMPro; // Adiciona a biblioteca para usar TextMeshPro
using System.Collections;
using System.Collections.Generic;

public class HordeManager : MonoBehaviour
{
    [Header("Configurações da Horda")]
    public int enemiesPerHordeMin = 5;
    public int enemiesPerHordeMax = 10;
    public int victoryHorde = 5;

    [Header("Dados dos Inimigos")]
    public EnemyDataSO[] enemyTypes;

    [Header("Configuração das Rotas")]
    public List<SpawnPath> spawnPaths;
    private int lastPathIndex = -1;

    [Header("Status da Horda")]
    public int currentHorde = 0;
    public int enemyLevel = 1;

    // Nova variável para o texto da horda na UI
    [Header("UI")]
    public TextMeshProUGUI hordeText;
    public TextMeshProUGUI hordeTextBuild;

    private List<GameObject> aliveEnemies = new List<GameObject>();
    private bool waveIsActive = false;
    private Transform playerTransform;

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
            if (aliveEnemies.Count == 0)
            {
                waveIsActive = false;
                Debug.Log("Horda " + currentHorde + " concluída!");
                if (currentHorde >= victoryHorde)
                {
                    Debug.Log("Parabéns! Você venceu o jogo!");
                    // Pode adicionar um evento de vitória aqui
                }
                else
                {
                    Invoke("StartNextHorde", 5f);
                }
            }
        }
    }

    // --- NOVO MÉTODO ---
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
        Debug.Log("Iniciando Horda " + currentHorde);
        SpawnEnemies();
        waveIsActive = true;

        // Chamada para atualizar a UI
        UpdateHordeUI();
    }

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
            int pathIndex = GetRandomPathIndex();
            SpawnPath selectedPath = spawnPaths[pathIndex];

            if (selectedPath.spawnPoint == null || selectedPath.patrolPoints == null || selectedPath.patrolPoints.Count == 0)
            {
                Debug.LogWarning("A rota '" + selectedPath.pathName + "' não está configurada corretamente. Pulando este spawn.");
                continue;
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