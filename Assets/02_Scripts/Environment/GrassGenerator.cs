using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class GrassVariation
{
    public string name = "Grass Variant";
    public Sprite[] frames;
    public float animationSpeed = 5f;
}

public class GrassGenerator : MonoBehaviour
{
    [Header("Grass Variations")]
    [Tooltip("For each variation, expand the sprite sheet in your Project window and drag ALL of its frames into the 'frames' array.")]
    public GrassVariation[] grassVariations;

    [Header("Grid Size")]
    public int width = 100;
    public int height = 100;

    private class RuntimeAnimatedTile : TileBase
    {
        public Sprite[] sprites;
        public float speed;
        public float randomOffset;

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            if (sprites != null && sprites.Length > 0)
                tileData.sprite = sprites[0];
        }

        public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
        {
            if (sprites != null && sprites.Length > 0)
            {
                tileAnimationData.animatedSprites = sprites;
                tileAnimationData.animationSpeed = speed;
                // Add a slightly different start time per position so they don't all perfectly sync
                tileAnimationData.animationStartTime = randomOffset + (position.x * 0.1f) + (position.y * 0.1f);
                return true;
            }
            return false;
        }
    }

    void Start()
    {
        // Disable existing backgrounds automatically if they are common names
        GameObject oldBg = GameObject.Find("Background");
        if (oldBg != null) oldBg.SetActive(false);
        GameObject oldBg2 = GameObject.Find("BG");
        if (oldBg2 != null) oldBg2.SetActive(false);

        if (grassVariations == null || grassVariations.Length == 0)
        {
            Debug.LogWarning("GrassGenerator: No grass variations assigned!");
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

        // Convert sprite variations to animated tiles dynamically
        RuntimeAnimatedTile[] grassTiles = new RuntimeAnimatedTile[grassVariations.Length];
        for (int i = 0; i < grassVariations.Length; i++)
        {
            GrassVariation v = grassVariations[i];
            RuntimeAnimatedTile tile = ScriptableObject.CreateInstance<RuntimeAnimatedTile>();
            tile.sprites = v.frames;
            tile.speed = v.animationSpeed;
            tile.randomOffset = Random.Range(0f, 10f);
            grassTiles[i] = tile;
        }

        // Fill grid randomly
        int halfW = width / 2;
        int halfH = height / 2;

        for (int x = -halfW; x < halfW; x++)
        {
            for (int y = -halfH; y < halfH; y++)
            {
                RuntimeAnimatedTile randomTile = grassTiles[Random.Range(0, grassTiles.Length)];
                
                // Only place tiles that actually have frames assigned
                if (randomTile.sprites != null && randomTile.sprites.Length > 0)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), randomTile);
                }
            }
        }
    }
}
