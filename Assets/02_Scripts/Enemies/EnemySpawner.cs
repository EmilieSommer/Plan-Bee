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

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Grace Period")]
    public int gracePeriodDays = 3; // No enemies for the first 3 days!

    [Header("Spawn Interval")]
    public float minSpawnTime = 60f;
    public float maxSpawnTime = 180f;

    private float timer;

    private void Start()
    {
        timer = Random.Range(minSpawnTime, maxSpawnTime);
    }

    private void Update()
    {
        if (DayCycleManager.Instance != null && DayCycleManager.Instance.currentDay <= gracePeriodDays)
        {
            return; // Grace period active!
        }

        SeasonProfile profile = SeasonManager.Instance.GetCurrentProfile();
        float difficulty = DayCycleManager.Instance != null ? DayCycleManager.Instance.DifficultyMultiplier : 1f;
        float seasonMultiplier = profile != null ? profile.spawnMultiplier : 1f;

        timer -= Time.deltaTime * difficulty * seasonMultiplier;

        if (timer <= 0f)
        {
            SpawnEnemy(profile);
            timer = Random.Range(minSpawnTime, maxSpawnTime);
        }
    }

    void SpawnEnemy(SeasonProfile profile)
    {
        if (profile == null || profile.enemies == null || profile.enemies.Length == 0)
            return;

        EnemyType type = GetWeightedEnemy(profile);

        GameObject prefab = GetPrefab(type);
        if (prefab == null) return;

        Vector2 spawnPos = GetRandomSpawnPosition();

        Instantiate(prefab, spawnPos, Quaternion.identity);

        // WARNING removed per user request
    }

    Vector2 GetRandomSpawnPosition()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return transform.position;

        return spawnPoints[Random.Range(0, spawnPoints.Length)].position;
    }

    EnemyType GetWeightedEnemy(SeasonProfile profile)
    {
        float total = 0f;

        foreach (var e in profile.enemies)
            total += e.weight;

        float r = Random.Range(0, total);
        float current = 0f;

        foreach (var e in profile.enemies)
        {
            current += e.weight;
            if (r <= current)
                return e.type;
        }

        return profile.enemies[0].type;
    }

    GameObject GetPrefab(EnemyType type)
    {
        return type switch
        {
            EnemyType.VarroaMite => varroaPrefab,
            EnemyType.HiveBeetle => hiveBeetlePrefab,
            EnemyType.WaxMoth => waxMothPrefab,
            EnemyType.Mouse => mousePrefab,
            EnemyType.Ant => antPrefab,
            EnemyType.Wasp => waspPrefab,
            EnemyType.RobberBee => robberBeePrefab,
            EnemyType.Bear => bearPrefab,
            EnemyType.Skunk => skunkPrefab,
            _ => null
        };
    }
}