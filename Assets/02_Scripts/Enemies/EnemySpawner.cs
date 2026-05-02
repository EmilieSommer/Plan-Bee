using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    [Header("Base Spawn")]
    public float baseSpawnInterval = 3f;
    public float spawnRadius = 1f;

    [Header("Scaling")]
    public float minInterval = 0.5f;
    public float spawnIncreasePerDay = 0.15f;

    private float timer;

    private void Update()
    {
        float difficulty = DayCycleManager.Instance.DifficultyMultiplier;

        float interval = Mathf.Max(
            minInterval,
            baseSpawnInterval / difficulty
        );

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnEnemy();
            timer = interval;
        }
    }

    void SpawnEnemy()
    {
        Vector2 spawnPos =
            (Vector2)transform.position +
            Random.insideUnitCircle * spawnRadius;

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}