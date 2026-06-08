using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class AutoFixEnemies
{
    static AutoFixEnemies()
    {
        EditorApplication.delayCall += DoFix;
    }

    [MenuItem("Tools/Fix Enemy Sprites")]
    public static void DoFix()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/03_Prefabs" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.Contains("Enemy") && !path.Contains("Wasp") && !path.Contains("Mite") && !path.Contains("Robber") && !path.Contains("Beetle") && !path.Contains("Moth") && !path.Contains("Ant") && !path.Contains("Egg"))
                continue;

            using (var editScope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                GameObject prefab = editScope.prefabContentsRoot;
                bool modified = false;

                // Remove 3D components
                var mf = prefab.GetComponent<MeshFilter>();
                if (mf != null) { Object.DestroyImmediate(mf, true); modified = true; }
                var mr = prefab.GetComponent<MeshRenderer>();
                if (mr != null) { Object.DestroyImmediate(mr, true); modified = true; }
                var bc = prefab.GetComponent<BoxCollider>();
                if (bc != null) { Object.DestroyImmediate(bc, true); modified = true; }

                // Add 2D components
                var sr = prefab.GetComponent<SpriteRenderer>();
                if (sr == null) { sr = prefab.AddComponent<SpriteRenderer>(); modified = true; }
                
                var bc2d = prefab.GetComponent<BoxCollider2D>();
                if (bc2d == null && prefab.GetComponent<Collider2D>() == null) { prefab.AddComponent<BoxCollider2D>(); modified = true; }

                // Assign sprite
                string searchName = prefab.name;
                if (searchName == "RobberBee") searchName = "Group 70"; // Swapped!
                if (searchName == "Wasp") searchName = "Group 29"; // Swapped!
                if (prefab.name.Contains("Egg")) searchName = "Egg";

                string[] spriteGuids = AssetDatabase.FindAssets(searchName + " t:Sprite");
                if (spriteGuids.Length == 0) spriteGuids = AssetDatabase.FindAssets(prefab.name + " t:Sprite");
                
                if (spriteGuids.Length > 0)
                {
                    Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(spriteGuids[0]));
                    if (s != null && sr.sprite != s)
                    {
                        sr.sprite = s;
                        sr.sortingOrder = 11; // Ensure it renders above background
                        
                        // Fix tiny scale for enemies
                        if (!prefab.name.Contains("Egg") && prefab.transform.localScale.x < 2f)
                        {
                            prefab.transform.localScale = new Vector3(3f, 3f, 1f);
                        }

                        modified = true;
                    }
                }

                if (modified)
                {
                    Debug.Log("Auto-Fixed Enemy Prefab: " + path);
                }
            }
        }
    }
}
