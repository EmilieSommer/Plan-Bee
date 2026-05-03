using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject varroaPrefab;
    public GameObject hiveBeetlePrefab;
    public GameObject waxMothPrefab;
    public GameObject mousePrefab;
    public GameObject antPrefab;
    public GameObject waspPrefab;
    public GameObject robberBeePrefab;
    public GameObject bearPrefab;
    public GameObject skunkPrefab;

    [Header("Base Spawn")]
    public float baseSpawnInterval = 3f;
    public float spawnRadius = 1f;

    [Header("Scaling")]
    public float minInterval = 0.5f;

    private float timer;

    private void Update()
    {
        float difficulty = DayCycleManager.Instance.DifficultyMultiplier;

        if (SeasonManager.Instance.currentSeason == Season.Winter)
            difficulty *= 1.3f;

        float interval = Mathf.Max(minInterval, baseSpawnInterval / difficulty);

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnEnemy();
            timer = interval;
        }
    }

    void SpawnEnemy()
    {
        EnemyType[] allowed = SeasonManager.Instance.GetAllowedEnemies();

        if (allowed == null || allowed.Length == 0)
            return;

        EnemyType type = allowed[Random.Range(0, allowed.Length)];

        GameObject prefab = GetPrefab(type);

        if (prefab == null) return;

        Vector2 spawnPos =
            (Vector2)transform.position +
            Random.insideUnitCircle * spawnRadius;

        Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    GameObject GetPrefab(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.VarroaMite: return varroaPrefab;
            case EnemyType.HiveBeetle: return hiveBeetlePrefab;
            case EnemyType.WaxMoth: return waxMothPrefab;
            case EnemyType.Mouse: return mousePrefab;
            case EnemyType.Ant: return antPrefab;
            case EnemyType.Wasp: return waspPrefab;
            case EnemyType.RobberBee: return robberBeePrefab;
            case EnemyType.Bear: return bearPrefab;
            case EnemyType.Skunk: return skunkPrefab;
        }

        return null;
    }
}