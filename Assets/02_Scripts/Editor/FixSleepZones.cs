using UnityEngine;
using UnityEditor;

public class FixSleepZones : MonoBehaviour
{
    [MenuItem("Plan Bee/Fix Sleep Zones on Prefabs")]
    public static void FixSleepZonesOnPrefabs()
    {
        string[] prefabPaths = new string[]
        {
            "Assets/03_Prefabs/BroodZone.prefab",
            "Assets/03_Prefabs/StorageZone.prefab",
            "Assets/03_Prefabs/InsidehiveZone.prefab"
        };

        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                if (prefab.GetComponent<SleepZone>() == null)
                {
                    prefab.AddComponent<SleepZone>();
                    Debug.Log($"Added SleepZone to {prefab.name}");
                }
                EditorUtility.SetDirty(prefab);
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Finished fixing SleepZones.");
    }
    [MenuItem("Plan Bee/Set Scene Sleep Zones Capacity to 2")]
    public static void SetSceneSleepZonesCapacity()
    {
        SleepZone[] allZones = FindObjectsOfType<SleepZone>(true);
        int changed = 0;
        foreach (var zone in allZones)
        {
            if (zone.capacityPerZone != 2)
            {
                Undo.RecordObject(zone, "Change Sleep Zone Capacity");
                zone.capacityPerZone = 2;
                changed++;
                EditorUtility.SetDirty(zone);
            }
        }
        
        // Also update prefabs so future ones have 2
        string[] prefabPaths = new string[]
        {
            "Assets/03_Prefabs/BroodZone.prefab",
            "Assets/03_Prefabs/StorageZone.prefab",
            "Assets/03_Prefabs/InsidehiveZone.prefab",
            "Assets/03_Prefabs/TestPrefabs/SleepZone.prefab"
        };
        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                SleepZone sz = prefab.GetComponent<SleepZone>();
                if (sz != null && sz.capacityPerZone != 2)
                {
                    sz.capacityPerZone = 2;
                    EditorUtility.SetDirty(prefab);
                }
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"Updated {changed} SleepZones in the scene to capacity 2, and updated the prefabs.");
    }

    [MenuItem("Plan Bee/Assign Walk Frames to Bees")]
    public static void AssignFrames()
    {
        string[] prefabs = new string[] {
            "Assets/03_Prefabs/TestPrefabs/BuilderBee.prefab",
            "Assets/03_Prefabs/TestPrefabs/DroneBee.prefab",
            "Assets/03_Prefabs/TestPrefabs/ForagerBee.prefab",
            "Assets/03_Prefabs/TestPrefabs/HouseBee.prefab",
            "Assets/03_Prefabs/TestPrefabs/NurseBee.prefab",
            "Assets/03_Prefabs/Bees/QueenBee.prefab"
        };
        
        string[] spriteSheets = new string[] {
            "Assets/04_Sprites/Bees/PNGS/Builder bee_WalkSheet.png",
            "Assets/04_Sprites/Bees/PNGS/Drone_WalkSheet.png",
            "Assets/04_Sprites/Bees/PNGS/Forager bee_WalkSheet.png",
            "Assets/04_Sprites/Bees/PNGS/worker bee_WalkSheet.png",
            "Assets/04_Sprites/Bees/PNGS/Nurse bee_WalkSheet.png",
            "Assets/04_Sprites/Bees/PNGS/The Queen_WalkSheet.png"
        };

        for (int i = 0; i < prefabs.Length; i++)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabs[i]);
            if (prefab != null)
            {
                Bee bee = prefab.GetComponent<Bee>();
                if (bee != null)
                {
                    Object[] assets = AssetDatabase.LoadAllAssetsAtPath(spriteSheets[i]);
                    // Filter out only Sprite objects, ignoring the Texture2D itself
                    System.Collections.Generic.List<Sprite> sprites = new System.Collections.Generic.List<Sprite>();
                    foreach(var a in assets)
                    {
                        if (a is Sprite) sprites.Add((Sprite)a);
                    }

                    if (sprites.Count > 0)
                    {
                        // Try to order by name so Frame_0, Frame_1 is preserved
                        sprites.Sort((a, b) => a.name.CompareTo(b.name));
                        
                        Undo.RecordObject(bee, "Assign Walk Frames");
                        bee.walkFrames = sprites.ToArray();
                        PrefabUtility.SavePrefabAsset(prefab);
                        Debug.Log($"Assigned {sprites.Count} sprites to {prefab.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"No sprites found at {spriteSheets[i]}. Is it set to Multiple in the Sprite Editor?");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Could not find prefab: {prefabs[i]}");
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Finished assigning Walk Frames!");
    }
}
