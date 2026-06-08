using UnityEngine;
using UnityEditor;

public class FixPrefabs : MonoBehaviour
{
    [MenuItem("Plan Bee/Fix Prefab Offsets")]
    public static void FixOffsets()
    {
        string[] paths = new string[] {
            "Assets/03_Prefabs/BroodZone.prefab",
            "Assets/03_Prefabs/StorageZone.prefab",
            "Assets/03_Prefabs/TestPrefabs/ForagerEgg.prefab",
            "Assets/03_Prefabs/TestPrefabs/NurseEgg.prefab",
            "Assets/03_Prefabs/TestPrefabs/BuilderEgg.prefab",
            "Assets/03_Prefabs/TestPrefabs/HouseEgg.prefab",
            "Assets/03_Prefabs/TestPrefabs/DroneEgg.prefab"
        };

        foreach (string path in paths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                Transform[] transforms = prefab.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in transforms)
                {
                    // Zero out all physics colliders, zones, AND child graphics for eggs
                    if (t.GetComponent<Collider2D>() != null || t.GetComponent<Zone>() != null || prefab.name.Contains("Egg"))
                    {
                        if (t != prefab.transform) // only zero children, not the root if we don't want to break things, but wait, root is fine too if instantiated
                        {
                            t.localPosition = Vector3.zero;
                        }
                    }
                }
                EditorUtility.SetDirty(prefab);
                Debug.Log($"Fixed offsets in {prefab.name}");
            }
        }
        AssetDatabase.SaveAssets();
    }
}
