using UnityEngine;
using UnityEngine.Tilemaps;

public class GrassFieldSpawner : MonoBehaviour
{
    [SerializeField] private TileBase[] baseTiles;
    [SerializeField] private TileBase[] overlayTiles;
    [SerializeField] private int width = 40;
    [SerializeField] private int height = 40;
    [Range(0f, 1f)] [SerializeField] private float overlayDensity = 0.15f;
    [SerializeField] private int seed = 42;
    [SerializeField] private int sortingOrderBase = -10;
    [SerializeField] private int sortingOrderOverlay = -9;

    private void Start()
    {
        Grid grid = FindObjectOfType<Grid>();
        if (grid == null)
        {
            Debug.LogError("[GrassFieldSpawner] No Grid found in scene.");
            return;
        }
        if (baseTiles == null || baseTiles.Length == 0)
        {
            Debug.LogError("[GrassFieldSpawner] baseTiles array is empty.");
            return;
        }
        if (overlayTiles == null || overlayTiles.Length == 0)
        {
            Debug.LogError("[GrassFieldSpawner] overlayTiles array is empty.");
            return;
        }

        Tilemap baseTm = CreateTilemap(grid, "GrassBaseTilemap", sortingOrderBase);
        Tilemap overlayTm = CreateTilemap(grid, "GrassOverlayTilemap", sortingOrderOverlay);

        var rng = new System.Random(seed);
        int halfW = width / 2;
        int halfH = height / 2;

        for (int x = -halfW; x < halfW; x++)
        {
            for (int y = -halfH; y < halfH; y++)
            {
                var pos = new Vector3Int(x, y, 0);
                var baseTile = baseTiles[rng.Next(baseTiles.Length)];
                baseTm.SetTile(pos, baseTile);
                if (rng.NextDouble() < overlayDensity)
                {
                    var overlayTile = overlayTiles[rng.Next(overlayTiles.Length)];
                    overlayTm.SetTile(pos, overlayTile);
                }
            }
        }
    }

    private Tilemap CreateTilemap(Grid parentGrid, string name, int sortingOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parentGrid.transform, false);
        var tm = go.AddComponent<Tilemap>();
        var r = go.AddComponent<TilemapRenderer>();
        r.sortingOrder = sortingOrder;
        return tm;
    }
}
