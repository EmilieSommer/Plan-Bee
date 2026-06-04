#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class PrefabCleaner : MonoBehaviour
{
    [MenuItem("Plan Bee/3. Clean Zone Prefabs (Remove Old Sprites & Fix Colliders)")]
    public static void CleanPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        int cleanedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                // Only clean prefabs that have Zone scripts on them
                if (prefab.GetComponent<Zone>() != null || 
                    prefab.GetComponent("NurseBeeZone") != null || 
                    prefab.GetComponent("HouseBeeZone") != null || 
                    prefab.GetComponent("SleepZone") != null || 
                    prefab.GetComponent("DroneZone") != null ||
                    prefab.name.Contains("Zone"))
                {
                    bool modified = false;

                    // 1. Destroy ALL SpriteRenderers (even on children!)
                    SpriteRenderer[] srs = prefab.GetComponentsInChildren<SpriteRenderer>(true);
                    foreach (var sr in srs)
                    {
                        DestroyImmediate(sr, true);
                        modified = true;
                    }

                    // 2. Fix ALL BoxCollider2Ds to exactly 1x1 size
                    BoxCollider2D[] cols = prefab.GetComponentsInChildren<BoxCollider2D>(true);
                    foreach (var col in cols)
                    {
                        if (col.size != new Vector2(1, 1) || col.offset != Vector2.zero)
                        {
                            col.size = new Vector2(1, 1);
                            col.offset = Vector2.zero;
                            modified = true;
                        }
                    }

                    if (modified)
                    {
                        EditorUtility.SetDirty(prefab);
                        cleanedCount++;
                        Debug.Log("Cleaned Sprites & Fixed Colliders on: " + prefab.name);
                    }
                }
            }
        }

        if (cleanedCount > 0)
        {
            AssetDatabase.SaveAssets();
            Debug.Log("<color=green><b>SUCCESS!</b> Deep Cleaned " + cleanedCount + " Zone Prefabs.</color> Old tiles are gone, and colliders are now perfectly 1x1!");
        }
        else
        {
            Debug.Log("No prefabs needed cleaning! (They might already be clean).");
        }
    }
}
#endif
