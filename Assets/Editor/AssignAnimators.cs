using UnityEditor;
using UnityEngine;

public class AssignAnimators
{
    [MenuItem("Tools/Plan Bee/Assign Animators To Bees")]
    public static void Execute()
    {
        string[] prefabPaths = new string[] {
            "Assets/03_Prefabs/Bees/HouseBee.prefab",
            "Assets/03_Prefabs/TestPrefabs/HouseBee.prefab"
        };
        RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/06_Animations/BeeController.controller");

        if (controller == null)
        {
            Debug.LogError("BeeController not found!");
            return;
        }

        int count = 0;
        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            Bee beeComp = prefab.GetComponent<Bee>();
            if (beeComp != null)
            {
                Animator anim = prefab.GetComponent<Animator>();
                if (anim == null)
                {
                    anim = prefab.AddComponent<Animator>();
                }
                
                anim.runtimeAnimatorController = controller;
                EditorUtility.SetDirty(prefab);
                PrefabUtility.SavePrefabAsset(prefab);
                count++;
            }
        }
        
        Debug.Log($"Successfully assigned Animator and Controller to {count} bee prefabs!");
    }
}
