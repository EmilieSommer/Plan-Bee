using UnityEditor;
using UnityEngine;

public class AssignEnemySprites
{
    [MenuItem("Tools/Assign Enemy Sprites")]
    public static void Execute()
    {
        Sprite miteSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/04_Sprites/Bees/PNGS/Mite.png");
        Sprite group70 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/04_Sprites/Bees/PNGS/Group 70.png");
        Sprite group29 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/04_Sprites/Bees/PNGS/Group 29.png");

        AssignTo("Assets/03_Prefabs/TestPrefabs/VarroaMite.prefab", miteSprite);
        AssignTo("Assets/03_Prefabs/Enemies/VarroaMite.prefab", miteSprite);
        
        // Guessing Wasp/RobberBee from the groups
        AssignTo("Assets/03_Prefabs/TestPrefabs/Wasp.prefab", group70);
        AssignTo("Assets/03_Prefabs/TestPrefabs/RobberBee.prefab", group29);
        
        AssetDatabase.SaveAssets();
        Debug.Log("Finished assigning enemy sprites!");
    }
    
    static void AssignTo(string path, Sprite spr)
    {
        if (spr == null) { Debug.LogWarning("Sprite was null for " + path); return; }
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab != null)
        {
            SpriteRenderer sr = prefab.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = spr;
                
                // Let's also adjust the size slightly to match bees (which are 25/32)
                float targetSize = 25f / 32f;
                float currentSize = spr.bounds.size.x;
                if (currentSize > 0)
                {
                    float scale = targetSize / currentSize;
                    sr.transform.localScale = new Vector3(scale, scale, 1f);
                }
                
                EditorUtility.SetDirty(prefab);
                PrefabUtility.SavePrefabAsset(prefab);
                Debug.Log("Successfully assigned sprite to " + path);
            }
        }
    }
}
