using UnityEngine;

[System.Serializable]
public class SeasonProfile
{
    public Season season;

    public EnemySpawnEntry[] enemies;

    public float spawnMultiplier = 1f;

    public GameObject weatherParticles;

    public GameObject scenery; // Add this
}