using UnityEditor;
using UnityEngine;

public static class FixBeePrefabs
{
    [MenuItem("Plan-Bee/Fix Bee Prefabs (Animator & Scale)")]
    public static void Execute()
    {
        string[] prefabs = new string[] 
        {
            "Assets/03_Prefabs/Bees/HouseBee.prefab",
            "Assets/03_Prefabs/Bees/ForagerBee.prefab",
            "Assets/03_Prefabs/Bees/NurseBee.prefab",
            "Assets/03_Prefabs/Bees/DroneBee.prefab",
            "Assets/03_Prefabs/Bees/BuilderBee.prefab",
            "Assets/03_Prefabs/Bees/QueenBee.prefab"
        };
        
        var controller = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>("Assets/06_Animations/BeeController.controller");
        if (controller == null)
        {
            Debug.LogError("Could not find BeeController!");

            return;
        }

        foreach (string path in prefabs)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                
                // 1. Add animator if missing
                Animator anim = instance.GetComponent<Animator>();
                if (anim == null) anim = instance.AddComponent<Animator>();
                anim.runtimeAnimatorController = controller;
                
                // 2. Set scale to 1
                instance.transform.localScale = Vector3.one;

                // For QueenBee, she scales herself in Awake() but let's reset her base scale just in case
                
                PrefabUtility.SaveAsPrefabAsset(instance, path);
                Object.DestroyImmediate(instance);
                Debug.Log("Fixed " + path);
            }
        }
        
        Debug.Log("All prefabs fixed.");

    }
}
