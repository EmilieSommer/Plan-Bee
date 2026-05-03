using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Simple scene setup to spawn test bees and display the hive visually.
/// </summary>
public class SceneSetup : MonoBehaviour
{
    [SerializeField] private int foragerCount = 3;
    [SerializeField] private int nurseCount = 2;
    [SerializeField] private int builderCount = 1;
    [SerializeField] private int droneCount = 1;
    [SerializeField] private int houseBeeCount = 2;

    [SerializeField] private Vector2 spawnAreaCenter = Vector2.zero;
    [SerializeField] private float spawnAreaRadius = 3f;

    private static SceneSetup instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void EnsureSetupExists()
    {
        if (instance != null) return;

        GameObject setupObject = new GameObject("SceneSetup");
        SceneSetup setup = setupObject.AddComponent<SceneSetup>();
        instance = setup;
    }

    private void Start()
    {
        InitializeManagers();
        SetupHiveVisuals();
        SpawnBees();
    }

    void InitializeManagers()
    {
        // Ensure GameManager exists
        if (GameManager.Instance == null)
        {
            GameObject gmObject = new GameObject("GameManager");
            gmObject.AddComponent<GameManager>();
        }

        // Ensure HiveManager exists
        if (HiveManager.Instance == null)
        {
            GameObject hmObject = new GameObject("HiveManager");
            hmObject.AddComponent<HiveManager>();
        }

        // Ensure ZoneManager exists
        if (ZoneManager.Instance == null)
        {
            GameObject zmObject = new GameObject("ZoneManager");
            zmObject.AddComponent<ZoneManager>();
        }

        // Ensure CurrencyManager exists
        if (CurrencyManager.Instance == null)
        {
            GameObject cmObject = new GameObject("CurrencyManager");
            cmObject.AddComponent<CurrencyManager>();
            cmObject.GetComponent<CurrencyManager>().pollen = 500;
            cmObject.GetComponent<CurrencyManager>().honey = 500;
        }

        // Ensure BuildManager exists
        if (BuildManager.Instance == null)
        {
            GameObject bmObject = new GameObject("BuildManager");
            bmObject.AddComponent<BuildManager>();
        }

        Debug.Log("✓ All managers initialized");
    }

    void SpawnBees()
    {
        SpawnBeeType(Bee.BeeType.Forager, foragerCount);
        SpawnBeeType(Bee.BeeType.Nurse, nurseCount);
        SpawnBeeType(Bee.BeeType.Builder, builderCount);
        SpawnBeeType(Bee.BeeType.Drone, droneCount);
        SpawnBeeType(Bee.BeeType.House, houseBeeCount);
        Debug.Log("✓ Spawned bees!");
    }

    void SpawnBeeType(Bee.BeeType type, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 randomPos = Random.insideUnitCircle * spawnAreaRadius + spawnAreaCenter;
            GameObject go = new GameObject($"{type} {i+1}");
            go.transform.position = new Vector3(randomPos.x, randomPos.y, 0);

            // Add required components
            CircleCollider2D collider = go.AddComponent<CircleCollider2D>();
            collider.radius = 0.3f;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.bodyType = RigidbodyType2D.Kinematic;

            go.AddComponent<SpriteRenderer>();

            // Add the specific bee subclass
            switch (type)
            {
                case Bee.BeeType.Forager:
                    go.AddComponent<ForagerBee>();
                    break;
                case Bee.BeeType.Nurse:
                    go.AddComponent<NurseBee>();
                    break;
                case Bee.BeeType.Builder:
                    go.AddComponent<BuilderBee>();
                    break;
                case Bee.BeeType.Drone:
                    go.AddComponent<DroneBee>();
                    break;
                case Bee.BeeType.House:
                    go.AddComponent<HouseBee>();
                    break;
            }

            go.tag = "Bee";
        }
    }

    void SetupHiveVisuals()
    {
        // Camera
        if (Camera.main == null)
        {
            GameObject camObject = new GameObject("Main Camera");
            Camera cam = camObject.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            camObject.AddComponent<CameraController>();
            camObject.transform.position = new Vector3(0, 0, -10);
        }

        // Skip if HiveGrid already exists in scene
        if (HiveGrid.Instance != null) return;

        // Grid root
        GameObject gridGO = new GameObject("HiveGrid");
        var grid = gridGO.AddComponent<Grid>();
        grid.cellSize = Vector3.one;

        // Built tilemap
        GameObject builtGO = new GameObject("BuiltTilemap");
        builtGO.transform.SetParent(gridGO.transform, false);
        var builtTilemap = builtGO.AddComponent<Tilemap>();
        var builtRenderer = builtGO.AddComponent<TilemapRenderer>();
        builtRenderer.sortingOrder = -1;

        // Marked tilemap (overlay)
        GameObject markedGO = new GameObject("MarkedTilemap");
        markedGO.transform.SetParent(gridGO.transform, false);
        var markedTilemap = markedGO.AddComponent<Tilemap>();
        var markedRenderer = markedGO.AddComponent<TilemapRenderer>();
        markedRenderer.sortingOrder = 0;

        // HiveGrid component
        var hiveGrid = gridGO.AddComponent<HiveGrid>();

        // Wire up tilemaps via reflection (avoids needing serialized fields at runtime)
        var t = typeof(HiveGrid);
        void SetField(string name, object val)
        {
            var f = t.GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            f?.SetValue(hiveGrid, val);
        }
        SetField("builtTilemap",  builtTilemap);
        SetField("markedTilemap", markedTilemap);

        Debug.Log("✓ HiveGrid tilemap created. Assign HiveTileLibrary in the Inspector or via Tools → Plan Bee → Populate Tile Library.");
    }
}
