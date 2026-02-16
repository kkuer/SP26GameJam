using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner instance;
    public Transform refPoint;
    public float spawnRadius = 5f;

    public int currentLevel = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject smallEnemyPrefab;
    public GameObject mediumEnemyPrefab;

    // list of how many enemies to spawn per level 
    public int[] enemiesPerLevel_small;
    public int[] enemiesPerLevel_medium;

    public List<GameObject> spawnedEnemies = new List<GameObject>();

    private void Start()
    {
        SpawnEnemies(currentLevel);
    }

    public void SpawnEnemies(int level)
    {
        List<GameObject> newEnemies = new List<GameObject>();
        for (int i = 0; i < enemiesPerLevel_small[level]; i++)
        {
            Vector2 randomPointInCircle = Random.insideUnitCircle;
            Vector2 newPosition = new Vector2(refPoint.position.x, refPoint.position.y) + new Vector2(randomPointInCircle.x, randomPointInCircle.y) * spawnRadius;
            GameObject newEnemy = Instantiate(smallEnemyPrefab, newPosition, Quaternion.identity);
            newEnemies.Add(newEnemy);
        }
        for (int i = 0; i < enemiesPerLevel_medium[level]; i++)
        {
            Vector2 randomPointInCircle = Random.insideUnitCircle;
            Vector2 newPosition = new Vector2(refPoint.position.x, refPoint.position.y) + new Vector2(randomPointInCircle.x, randomPointInCircle.y) * spawnRadius;
            GameObject newEnemy = Instantiate(mediumEnemyPrefab, newPosition, Quaternion.identity);
            newEnemies.Add(newEnemy);
        }
        spawnedEnemies.AddRange(newEnemies);
    }

    public void ClearEnemies()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        spawnedEnemies.Clear();
    }
}
