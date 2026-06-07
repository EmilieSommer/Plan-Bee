using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class AddStorageZones : ScriptableObject
{
    [MenuItem("Plan Bee/Add 2 Storage Zones")]
    public static void AddStorage()
    {
        HiveVisuals visuals = FindObjectOfType<HiveVisuals>();
        if (visuals == null || visuals.BuiltTilemap == null)
        {
            Debug.LogError("Could not find HiveVisuals in the scene!");
            return;
        }

        Vector3Int pos1 = new Vector3Int(2, 0, 0);
        Vector3Int pos2 = new Vector3Int(2, -1, 0);

        // Load Prefab
        GameObject storagePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/03_Prefabs/StorageZone.prefab");
        if (storagePrefab == null)
        {
            Debug.LogError("Could not find StorageZone.prefab!");
            return;
        }

        visuals.Library.Init();
        Sprite sprite = visuals.Library.GetBorderSprite(HiveTileType.Storage, 15);
        if (sprite != null)
        {
            Tile t = ScriptableObject.CreateInstance<Tile>();
            t.sprite = sprite;
            visuals.BuiltTilemap.SetTile(pos1, t);
            visuals.BuiltTilemap.SetTile(pos2, t);
        }

        // Instantiate Prefabs
        GameObject g1 = (GameObject)PrefabUtility.InstantiatePrefab(storagePrefab);
        g1.transform.position = visuals.BuiltTilemap.GetCellCenterWorld(pos1);
        
        GameObject g2 = (GameObject)PrefabUtility.InstantiatePrefab(storagePrefab);
        g2.transform.position = visuals.BuiltTilemap.GetCellCenterWorld(pos2);

        visuals.RefreshAt(pos1);
        visuals.RefreshAt(pos2);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("Successfully added 2 Storage Zones to your starting hive!");
    }
}
