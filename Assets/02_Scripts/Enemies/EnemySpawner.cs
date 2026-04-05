using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval = 3f;
    public float spawnRadius = 1f;

    private float timer;

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnEnemy();
            timer = spawnInterval;
        }
    }

    void SpawnEnemy()
    {
        Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}