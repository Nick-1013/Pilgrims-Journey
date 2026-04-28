using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // ---------------- ARRAY ----------------
    [Header("Enemy Prefabs (Array)")]
    public GameObject[] enemyPrefabs;

    // ---------------- LIST ----------------
    [Header("Active Enemies (List)")]
    public List<GameObject> activeEnemies = new List<GameObject>();

    // ---------------- SETTINGS ----------------
    [Header("Spawn Settings")]
    public int enemiesPerWave = 5;
    public float spawnRadius = 5f;

    private int currentWave = 0;
    private GameManagerScript gameManager;

    // ---------------- AWAKE ----------------
    void Awake()
    {
        gameManager = FindFirstObjectByType<GameManagerScript>();

        Debug.Log("Spawner Awake: Initializing system...");

        if (enemyPrefabs.Length == 0)
        {
            Debug.LogError("No enemy prefabs assigned!");
        }
    }

    void Start()
    {
        SpawnWave();
    }

    // ---------------- SPAWN WAVE ----------------
    void SpawnWave()
    {
        currentWave++;

        Debug.Log("Spawning Wave: " + currentWave);

        // ---------------- FOR LOOP ----------------
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        // ---------------- RANDOM.RANGE ----------------
        int randomIndex = Random.Range(0, enemyPrefabs.Length);

        Vector2 randomPosition = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;

        GameObject enemy = Instantiate(enemyPrefabs[randomIndex], randomPosition, Quaternion.identity);

        activeEnemies.Add(enemy);

        // IMPORTANT: tell GameManager a new enemy exists
        if (gameManager != null)
            gameManager.RegisterEnemySpawn();
    }

    // ---------------- FOREACH LOOP ----------------
    public void RemoveDeadEnemies()
    {
        // Clean null entries (destroyed enemies)
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
            {
                activeEnemies.RemoveAt(i);
            }
        }

        // Example: Loop through remaining enemies
        foreach (GameObject enemy in activeEnemies)
        {
            Debug.Log("Enemy still alive: " + enemy.name);
        }
    }
}