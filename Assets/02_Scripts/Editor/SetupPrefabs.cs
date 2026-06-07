using UnityEngine;
using UnityEditor;

public class SetupPrefabs : EditorWindow
{
    [MenuItem("Plan Bee/Auto-Setup Bee & Enemy Prefabs")]
    public static void GeneratePrefabs()
    {
        EnsureFolder("Assets/03_Prefabs", "Bees");
        EnsureFolder("Assets/03_Prefabs", "Enemies");

        GameObject foragerPrefab = CreateBeePrefab("Forager bee.svg", "ForagerBee", Bee.BeeType.Forager, typeof(ForagerBee));
        GameObject nursePrefab = CreateBeePrefab("Nurse bee.svg", "NurseBee", Bee.BeeType.Nurse, typeof(NurseBee));
        GameObject housePrefab = CreateBeePrefab("worker bee.svg", "HouseBee", Bee.BeeType.House, typeof(HouseBee));
        CreateBeePrefab("Builder bee.svg", "BuilderBee", Bee.BeeType.Builder, typeof(BuilderBee));
        CreateBeePrefab("Drone.svg", "DroneBee", Bee.BeeType.Drone, typeof(DroneBee));
        
        GameObject queenPrefab = CreateBeePrefab("The Queen.svg", "QueenBee", Bee.BeeType.Queen, typeof(QueenBee));

        // Assign starting bees to Queen
        if (queenPrefab != null)
        {
            QueenBee q = queenPrefab.GetComponent<QueenBee>();
            if (q != null)
            {
                q.foragerPrefab = foragerPrefab;
                q.nursePrefab = nursePrefab;
                q.housePrefab = housePrefab;
                EditorUtility.SetDirty(queenPrefab);
                PrefabUtility.SavePrefabAsset(queenPrefab);
            }
        }

        CreateEnemyPrefab("Mite.png", "VarroaMite", EnemyType.VarroaMite, typeof(VarroaMites));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Successfully built all Bee and Enemy Prefabs in Assets/03_Prefabs!");
    }

    private static void EnsureFolder(string parentFolder, string newFolderName)
    {
        if (!AssetDatabase.IsValidFolder(parentFolder + "/" + newFolderName))
        {
            AssetDatabase.CreateFolder(parentFolder, newFolderName);
        }
    }

    private static GameObject CreateBeePrefab(string spriteName, string prefabName, Bee.BeeType type, System.Type scriptType)
    {
        string nameWithoutExtension = spriteName.Replace(".svg", "").Replace(".png", "");
        string[] guids = AssetDatabase.FindAssets(nameWithoutExtension);
        Sprite sprite = null;
        string foundPath = "";

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains(spriteName))
            {
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var asset in assets)
                {
                    if (asset is Sprite s)
                    {
                        sprite = s;
                        foundPath = path;
                        break;
                    }
                }
            }
            if (sprite != null) break;
        }

        if (sprite == null)
        {
            Debug.LogWarning($"Could not auto-load SVG Sprite for {prefabName}. Generating prefab anyway—please assign the SVG manually in the inspector!");
        }
        else
        {
            Debug.Log($"Found sprite for {prefabName} at {foundPath}");
        }

        GameObject go = new GameObject(prefabName);
        
        // Components
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        if (sprite != null) sr.sprite = sprite;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;

        Bee beeComp = (Bee)go.AddComponent(scriptType);
        beeComp.beeType = type;
        beeComp.beeName = prefabName;
        beeComp.maxHealth = (type == Bee.BeeType.Queen) ? 50f : 10f;
        beeComp.moveSpeed = (type == Bee.BeeType.Queen) ? 1.5f : 2.5f;

        // Save
        string savePath = $"Assets/03_Prefabs/Bees/{prefabName}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, savePath);
        DestroyImmediate(go);

        return prefab;
    }

    private static GameObject CreateEnemyPrefab(string spriteName, string prefabName, EnemyType type, System.Type scriptType)
    {
        string nameWithoutExtension = spriteName.Replace(".svg", "").Replace(".png", "");
        string[] guids = AssetDatabase.FindAssets(nameWithoutExtension);
        Sprite sprite = null;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains(spriteName))
            {
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var asset in assets)
                {
                    if (asset is Sprite s)
                    {
                        sprite = s;
                        break;
                    }
                }
            }
            if (sprite != null) break;
        }

        if (sprite == null)
        {
            Debug.LogWarning($"Could not auto-load PNG Sprite for {prefabName}. Generating prefab anyway—please assign the PNG manually in the inspector!");
        }

        GameObject go = new GameObject(prefabName);
        
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        if (sprite != null) sr.sprite = sprite;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;

        Enemy enemyComp = (Enemy)go.AddComponent(scriptType);
        enemyComp.enemyType = type;
        enemyComp.maxHealth = 15f;
        enemyComp.moveSpeed = 1.5f;

        string savePath = $"Assets/03_Prefabs/Enemies/{prefabName}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, savePath);
        DestroyImmediate(go);

        return prefab;
    }
}
