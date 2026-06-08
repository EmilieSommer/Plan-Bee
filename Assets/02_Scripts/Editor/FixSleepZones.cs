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
}
