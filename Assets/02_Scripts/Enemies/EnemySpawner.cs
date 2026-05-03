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

    [Header("Base Spawn")]
    public float baseSpawnInterval = 3f;

    [Header("Limits")]
    public float minInterval = 0.5f;

    private float timer;

    private void Update()
    {
        SeasonProfile profile = SeasonManager.Instance.GetCurrentProfile();

        float difficulty = DayCycleManager.Instance.DifficultyMultiplier;
        float seasonMultiplier = profile != null ? profile.spawnMultiplier : 1f;

        float interval = Mathf.Max(minInterval, baseSpawnInterval / (difficulty * seasonMultiplier));

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnEnemy(profile);
            timer = interval;
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