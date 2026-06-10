using UnityEngine;
using System.Collections.Generic;

public class EggSpawner : MonoBehaviour
{
    public static EggSpawner Instance;

    [Header("Egg Prefabs")]
    public GameObject builderEggPrefab;
    public GameObject nurseEggPrefab;
    public GameObject houseEggPrefab;
    public GameObject foragerEggPrefab;
    public GameObject droneEggPrefab;

    private Dictionary<EggType, GameObject> eggDictionary;

    private void Awake()
    {
        Instance = this;

        eggDictionary = new Dictionary<EggType, GameObject>
        {
            { EggType.Builder, builderEggPrefab },
            { EggType.Nurse, nurseEggPrefab },
            { EggType.House, houseEggPrefab },
            { EggType.Forager, foragerEggPrefab },
            { EggType.Drone, droneEggPrefab }
        };
    }

    // ✅ Now requires a zone
    public Egg SpawnEgg(EggType eggType, NurseBeeZone zone)
    {
        if (zone == null)
        {
            Debug.LogError("SpawnEgg called with NULL zone!");
            return null;
        }

        if (!eggDictionary.ContainsKey(eggType))
        {
            Debug.LogWarning("Egg type not found: " + eggType);
            return null;
        }

        GameObject prefab = eggDictionary[eggType];

        if (prefab == null)
        {
            Debug.LogWarning("Prefab not assigned for egg type: " + eggType);
            return null;
        }

        Vector2 spawnPosition = zone.GetRandomPoint();

        Debug.Log("Spawning in zone: " + zone.name);

        GameObject obj = Instantiate(prefab, spawnPosition, Quaternion.identity);

        Egg spawned = obj.GetComponent<Egg>();
        if (spawned != null) spawned.beeType = eggType.ToBeeType();
        return spawned;
    }
}