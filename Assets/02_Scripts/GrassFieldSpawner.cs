using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class GrassFieldSpawner : MonoBehaviour
{
    [Header("Seasonal Base Tiles")]
    [Tooltip("Assign the base grass variations for each season")]
    public Sprite[] springBaseTiles;
    public Sprite[] summerBaseTiles;
    public Sprite[] autumnBaseTiles;
    public Sprite[] winterBaseTiles;

    [Header("Flowers (Peaks in Spring)")]
    public TileBase[] flowerTiles;
    [Range(0f, 3f)] public float flowerDensity = 0.8f;
    [Range(0f, 0.5f)] public float flowerRandomOffset = 0.3f;

    [Header("Grass Tufts & Snow")]
    public TileBase[] springTufts;
    public TileBase[] summerTufts;
    public TileBase[] autumnTufts;
    public TileBase[] winterSnowTufts;
    [Range(0f, 3f)] public float tuftDensity = 0.6f;
    [Range(0f, 0.5f)] public float tuftRandomOffset = 0.3f;

    [Header("Leaves")]
    public Sprite[] springSummerLeaves; // Green leaves
    public Sprite[] autumnLeaves;       // Orange/Red leaves
    [Tooltip("Crank this up to 5.0+ to get huge piles of leaves!")]
    [Range(0f, 15f)] public float leafDensity = 5.0f; // Increased max density for huge piles of leaves
    [Range(0f, 1f)] public float leafTransparency = 0.65f;
    [Range(0f, 0.5f)] public float leafRandomOffset = 0.4f;
    
    [Header("Global Settings")]
    [SerializeField] private int width = 40;
    [SerializeField] private int height = 40;
    [SerializeField] private int seed = 42;
    [SerializeField] private float fadeDuration = 2f;
    
    [Header("Sorting Orders")]
    [SerializeField] private int sortingOrderBase = -100;
    [SerializeField] private int sortingOrderFlowers = -99;
    [SerializeField] private int sortingOrderTufts = -98;
    [SerializeField] private int sortingOrderLeaves = -96;

    // Internal data to track created tilemaps and their target alphas
    private class SeasonLayerData
    {
        public TilemapRenderer renderer;
        public Tilemap tilemap;
        public float targetAlpha;
        public int baseSortingOrder;
    }

    private Dictionary<Season, List<SeasonLayerData>> seasonLayers = new Dictionary<Season, List<SeasonLayerData>>();
    private Season currentActiveSeason;
    private Coroutine fadeCoroutine;

    private void Start()
    {
        Grid grid = FindObjectOfType<Grid>();
        if (grid == null)
        {
            Debug.LogError("[GrassFieldSpawner] No Grid found in scene.");
            return;
        }

        // Generate grids for all 4 seasons
        GenerateSeason(grid, Season.Spring);
        GenerateSeason(grid, Season.Summer);
        GenerateSeason(grid, Season.Autumn);
        GenerateSeason(grid, Season.Winter);

        // Figure out starting season
        if (SeasonManager.Instance != null)
        {
            currentActiveSeason = SeasonManager.Instance.currentSeason;
            SeasonManager.OnSeasonChangedEvent += HandleSeasonChange;
        }
        else
        {
            currentActiveSeason = Season.Spring;
        }

        // Instantly snap opacity to the starting season
        SetSeasonAlpha(currentActiveSeason, 1f);
        foreach (Season s in System.Enum.GetValues(typeof(Season)))
        {
            if (s != currentActiveSeason)
            {
                SetSeasonAlpha(s, 0f);
            }
        }
    }

    private void OnDestroy()
    {
        if (SeasonManager.Instance != null)
        {
            SeasonManager.OnSeasonChangedEvent -= HandleSeasonChange;
        }
    }

    private void GenerateSeason(Grid grid, Season season)
    {
        Sprite[] baseSprites = season switch {
            Season.Spring => springBaseTiles,
            Season.Summer => summerBaseTiles,
            Season.Autumn => autumnBaseTiles,
            Season.Winter => winterBaseTiles,
            _ => summerBaseTiles
        };

        if (baseSprites == null || baseSprites.Length == 0) return;

        List<SeasonLayerData> layers = new List<SeasonLayerData>();
        
        GameObject seasonParent = new GameObject($"Grass_{season}");
        seasonParent.transform.SetParent(grid.transform, false);

        Tilemap baseTm;
        TilemapRenderer baseR;
        CreateTilemap(seasonParent, "Base", sortingOrderBase, out baseTm, out baseR);
        layers.Add(new SeasonLayerData { tilemap = baseTm, renderer = baseR, targetAlpha = 1f, baseSortingOrder = sortingOrderBase });

        // Calculate seasonal multipliers
        float seasonFlowerMod = season switch { Season.Spring => 1f, Season.Summer => 0.15f, _ => 0f };
        float seasonLeafMod = season switch { Season.Autumn => 1f, Season.Summer => 0.15f, Season.Spring => 0.05f, _ => 0f };
        float seasonTuftMod = season switch { Season.Winter => 0.3f, _ => 1f };

        TileBase[] currentTufts = season switch {
            Season.Spring => springTufts,
            Season.Summer => summerTufts,
            Season.Autumn => autumnTufts,
            Season.Winter => winterSnowTufts,
            _ => summerTufts
        };
        
        Sprite[] currentLeaves = season switch {
            Season.Spring => springSummerLeaves,
            Season.Summer => springSummerLeaves,
            Season.Autumn => autumnLeaves,
            _ => null
        };

        // Determine how many layers we need based on modified density
        float modifiedFlowerDensity = flowerDensity * seasonFlowerMod;
        float modifiedTuftDensity = tuftDensity * seasonTuftMod;
        float modifiedLeafDensity = leafDensity * seasonLeafMod;

        int flowerLayersCount = Mathf.CeilToInt(modifiedFlowerDensity);
        Tilemap[] flowerTms = new Tilemap[flowerLayersCount];
        for (int i = 0; i < flowerLayersCount; i++)
        {
            Tilemap tm; TilemapRenderer tr;
            int order = sortingOrderFlowers + i;
            CreateTilemap(seasonParent, $"Flowers_{i}", order, out tm, out tr);
            flowerTms[i] = tm;
            layers.Add(new SeasonLayerData { tilemap = tm, renderer = tr, targetAlpha = 1f, baseSortingOrder = order });
        }

        int tuftLayersCount = Mathf.CeilToInt(modifiedTuftDensity);
        Tilemap[] tuftTms = new Tilemap[tuftLayersCount];
        for (int i = 0; i < tuftLayersCount; i++)
        {
            Tilemap tm; TilemapRenderer tr;
            int order = sortingOrderTufts + i;
            CreateTilemap(seasonParent, $"Tufts_{i}", order, out tm, out tr);
            tuftTms[i] = tm;
            layers.Add(new SeasonLayerData { tilemap = tm, renderer = tr, targetAlpha = 1f, baseSortingOrder = order });
        }

        int leafLayersCount = Mathf.CeilToInt(modifiedLeafDensity);
        Tilemap[] leafTms = new Tilemap[leafLayersCount];
        for (int i = 0; i < leafLayersCount; i++)
        {
            Tilemap tm; TilemapRenderer tr;
            int order = sortingOrderLeaves + i;
            CreateTilemap(seasonParent, $"Leaves_{i}", order, out tm, out tr);
            leafTms[i] = tm;
            layers.Add(new SeasonLayerData { tilemap = tm, renderer = tr, targetAlpha = leafTransparency, baseSortingOrder = order });
        }

        Tile[] baseTiles = CreateTilesFromSprites(baseSprites);
        Tile[] leafTiles = CreateTilesFromSprites(currentLeaves);

        var rng = new System.Random(seed + (int)season);
        int halfW = width / 2;
        int halfH = height / 2;

        for (int x = -halfW; x < halfW; x++)
        {
            for (int y = -halfH; y < halfH; y++)
            {
                var pos = new Vector3Int(x, y, 0);
                
                // Base
                baseTm.SetTile(pos, baseTiles[rng.Next(baseTiles.Length)]);
                
                // Flowers
                float remainingFlowerDensity = modifiedFlowerDensity;
                if (flowerTiles != null && flowerTiles.Length > 0)
                {
                    for (int l = 0; l < flowerLayersCount; l++)
                    {
                        if (rng.NextDouble() < Mathf.Min(1f, remainingFlowerDensity))
                        {
                            var tile = flowerTiles[rng.Next(flowerTiles.Length)];
                            if (tile != null)
                            {
                                flowerTms[l].SetTile(pos, tile);
                                if (flowerRandomOffset > 0f)
                                {
                                    float offsetX = (float)(rng.NextDouble() * 2.0 - 1.0) * flowerRandomOffset;
                                    float offsetY = (float)(rng.NextDouble() * 2.0 - 1.0) * flowerRandomOffset;
                                    flowerTms[l].SetTransformMatrix(pos, Matrix4x4.Translate(new Vector3(offsetX, offsetY, 0)));
                                }
                            }
                        }
                        remainingFlowerDensity -= 1f;
                    }
                }

                // Tufts
                float remainingTuftDensity = modifiedTuftDensity;
                if (currentTufts != null && currentTufts.Length > 0)
                {
                    for (int l = 0; l < tuftLayersCount; l++)
                    {
                        if (rng.NextDouble() < Mathf.Min(1f, remainingTuftDensity))
                        {
                            var tile = currentTufts[rng.Next(currentTufts.Length)];
                            if (tile != null)
                            {
                                tuftTms[l].SetTile(pos, tile);
                                if (tuftRandomOffset > 0f)
                                {
                                    float offsetX = (float)(rng.NextDouble() * 2.0 - 1.0) * tuftRandomOffset;
                                    float offsetY = (float)(rng.NextDouble() * 2.0 - 1.0) * tuftRandomOffset;
                                    tuftTms[l].SetTransformMatrix(pos, Matrix4x4.Translate(new Vector3(offsetX, offsetY, 0)));
                                }
                            }
                        }
                        remainingTuftDensity -= 1f;
                    }
                }
                
                // Leaves
                float remainingLeafDensity = modifiedLeafDensity;
                if (leafTiles != null && leafTiles.Length > 0)
                {
                    for (int l = 0; l < leafLayersCount; l++)
                    {
                        if (rng.NextDouble() < Mathf.Min(1f, remainingLeafDensity))
                        {
                            var leafTile = leafTiles[rng.Next(leafTiles.Length)];
                            if (leafTile != null)
                            {
                                leafTms[l].SetTile(pos, leafTile);
                                if (leafRandomOffset > 0f)
                                {
                                    float offsetX = (float)(rng.NextDouble() * 2.0 - 1.0) * leafRandomOffset;
                                    float offsetY = (float)(rng.NextDouble() * 2.0 - 1.0) * leafRandomOffset;
                                    leafTms[l].SetTransformMatrix(pos, Matrix4x4.Translate(new Vector3(offsetX, offsetY, 0)));
                                }
                            }
                        }
                        remainingLeafDensity -= 1f;
                    }
                }
            }
        }

        seasonLayers[season] = layers;
    }

    private Tile[] CreateTilesFromSprites(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0) return null;
        Tile[] tiles = new Tile[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            tiles[i] = ScriptableObject.CreateInstance<Tile>();
            tiles[i].sprite = sprites[i];
        }
        return tiles;
    }

    private void CreateTilemap(GameObject parentObj, string name, int sortingOrder, out Tilemap tm, out TilemapRenderer r)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parentObj.transform, false);
        tm = go.AddComponent<Tilemap>();
        r = go.AddComponent<TilemapRenderer>();
        r.sortingOrder = sortingOrder;
    }

    private void HandleSeasonChange(Season newSeason)
    {
        if (newSeason == currentActiveSeason) return;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(CrossfadeSeasons(currentActiveSeason, newSeason));
        
        currentActiveSeason = newSeason;
    }

    private void SetSeasonAlpha(Season season, float multiplier)
    {
        if (!seasonLayers.ContainsKey(season)) return;
        foreach (var layer in seasonLayers[season])
        {
            Color c = layer.tilemap.color;
            c.a = layer.targetAlpha * multiplier;
            layer.tilemap.color = c;
        }
    }

    private void SetSeasonSortingOffset(Season season, int offset)
    {
        if (!seasonLayers.ContainsKey(season)) return;
        foreach (var layer in seasonLayers[season])
        {
            layer.renderer.sortingOrder = layer.baseSortingOrder + offset;
        }
    }

    private IEnumerator CrossfadeSeasons(Season oldSeason, Season newSeason)
    {
        // Prevent background bleed-through by pulling the old season backwards in sorting order
        // and keeping it fully solid, while fading the new season in perfectly ON TOP of it.
        SetSeasonSortingOffset(oldSeason, -20);
        SetSeasonSortingOffset(newSeason, 0);

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);

            SetSeasonAlpha(oldSeason, 1f); // KEEP old season fully visible
            SetSeasonAlpha(newSeason, t);  // FADE IN new season on top

            yield return null;
        }

        // Once the new season is 100% visible, hide the old season completely
        SetSeasonAlpha(oldSeason, 0f);
        SetSeasonAlpha(newSeason, 1f);
        
        // Restore old season sorting order
        SetSeasonSortingOffset(oldSeason, 0);
    }
}
