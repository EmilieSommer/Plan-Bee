using UnityEngine;
using System.Collections.Generic;

public class EggSpawner : MonoBehaviour
{
    public static EggSpawner Instance;

    [Header("Zone")]
    public NurseBeeZone nurseZone;

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

        // Initialize dictionary
        eggDictionary = new Dictionary<EggType, GameObject>
        {
            { EggType.Builder, builderEggPrefab },
            { EggType.Nurse, nurseEggPrefab },
            { EggType.House, houseEggPrefab },
            { EggType.Forager, foragerEggPrefab },
            { EggType.Drone, droneEggPrefab }
        };
    }

    public void SpawnEgg(EggType eggType)
    {
        if (nurseZone == null)
        {
            Debug.LogWarning("NurseZone is not assigned in EggSpawner!");
            return;
        }

        if (!eggDictionary.ContainsKey(eggType))
        {
            Debug.LogWarning("Egg type not found: " + eggType);
            return;
        }

        GameObject prefab = eggDictionary[eggType];

        if (prefab == null)
        {
            Debug.LogWarning("Prefab not assigned for egg type: " + eggType);
            return;
        }

        Vector2 spawnPosition = nurseZone.GetRandomPoint();

        Instantiate(prefab, spawnPosition, Quaternion.identity);
    }
}