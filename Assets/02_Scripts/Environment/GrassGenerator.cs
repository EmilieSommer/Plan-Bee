using UnityEngine;
using UnityEngine.Tilemaps;

public class GrassGenerator : MonoBehaviour
{
    [Header("Grass Sprites")]
    public Sprite[] grassSprites;

    [Header("Grid Size")]
    public int width = 100;
    public int height = 100;

    void Start()
    {
        // Disable existing backgrounds automatically if they are common names
        GameObject oldBg = GameObject.Find("Background");
        if (oldBg != null) oldBg.SetActive(false);
        GameObject oldBg2 = GameObject.Find("BG");
        if (oldBg2 != null) oldBg2.SetActive(false);

        if (grassSprites == null || grassSprites.Length == 0)
        {
            Debug.LogWarning("GrassGenerator: No grass sprites assigned!");
            return;
        }

        // Setup a huge Grid and Tilemap
        GameObject gridObj = new GameObject("GrassGrid");
        gridObj.transform.SetParent(transform);
        Grid grid = gridObj.AddComponent<Grid>();
        grid.cellSize = Vector3.one;

        GameObject tilemapObj = new GameObject("GrassTilemap");
        tilemapObj.transform.SetParent(gridObj.transform);
        Tilemap tilemap = tilemapObj.AddComponent<Tilemap>();
        TilemapRenderer tr = tilemapObj.AddComponent<TilemapRenderer>();
        tr.sortingOrder = -20; // Ensure it stays behind everything (BuiltTilemap is -1)

        // Convert sprites to tiles dynamically
        Tile[] grassTiles = new Tile[grassSprites.Length];
        for (int i = 0; i < grassSprites.Length; i++)
        {
            grassTiles[i] = ScriptableObject.CreateInstance<Tile>();
            grassTiles[i].sprite = grassSprites[i];
        }

        // Fill grid randomly
        int halfW = width / 2;
        int halfH = height / 2;

        for (int x = -halfW; x < halfW; x++)
        {
            for (int y = -halfH; y < halfH; y++)
            {
                Tile randomTile = grassTiles[Random.Range(0, grassTiles.Length)];
                tilemap.SetTile(new Vector3Int(x, y, 0), randomTile);
            }
        }
    }
}
