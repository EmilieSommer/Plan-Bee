using UnityEditor;
using UnityEngine;

public class ResizeResources : EditorWindow
{
    [MenuItem("Plan Bee/Resize Pollen & Honey")]
    public static void ResizePrefabs()
    {
        string[] prefabsToResize = {
            "Assets/Resources/Prefabs/Pollen.prefab",
            "Assets/Resources/Prefabs/Honey.prefab"
        };

        foreach (string path in prefabsToResize)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path))
                {
                    GameObject contentsRoot = editingScope.prefabContentsRoot;
                    contentsRoot.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
                    Debug.Log($"Resized {prefab.name} to 0.4 scale.");
                }
            }
            else
            {
                Debug.LogWarning($"Could not find prefab at {path}");
            }
        }
        
        AssetDatabase.SaveAssets();
    }
}
