using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Manages the hive tile grid.
/// Handles placement, autotiling (same-type merging), and build-queue handoff.
/// </summary>
public class HiveGrid : MonoBehaviour
{
    public static HiveGrid Instance { get; private set; }

    [Header("Visuals")]
    [SerializeField] private HiveVisuals visuals;

    // ── State ─────────────────────────────────────────────────────────────────

    // tile type for every grid cell that exists
    private readonly Dictionary<Vector3Int, HiveTileType> types  = new();
    // construction status
    private readonly Dictionary<Vector3Int, bool>         marked = new();
    // cells that cannot be demolished or modified (e.g. the hive entrance)
    private readonly HashSet<Vector3Int> lockedCells = new();

    public event System.Action<Vector3Int> OnTileBuilt;
    public event System.Action<Vector3Int> OnTileMarked;

    static readonly Vector3Int[] Dirs =
        { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

    // ── Unity ─────────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (visuals == null) visuals = GetComponent<HiveVisuals>();
        
        // Scan tiles immediately in Awake so the Grid is ready before any bees run Start()
        ScanPaintedTiles();
    }

    // ── Starting hive ─────────────────────────────────────────────────────────

    /// <summary>
    /// Reads whatever was painted on the Built tilemap in the scene, identifies
    /// each cell's HiveTileType via the library's sprite reverse-lookup, and
    /// registers them as the starting hive. Then re-renders every cell so the
    /// autotile picks the correct edge variant for each.
    /// </summary>
    void ScanPaintedTiles()
    {
        if (visuals == null || visuals.BuiltTilemap == null || visuals.Library == null) return;

        var tilemap = visuals.BuiltTilemap;
        var library = visuals.Library;
        library.Init();

        // Collect first, render after — masks depend on neighbors being registered.
        var found = new List<(Vector3Int pos, HiveTileType type)>();

        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            var tileBase = tilemap.GetTile(pos);
            if (tileBase == null) continue;
            
            var sprite = tilemap.GetSprite(pos);
            var type = HiveTileType.None;
            
            if (sprite != null)
            {
                type = library.GetTypeFromSprite(sprite);
            }

            // Fallback: If the library didn't recognize the sprite (or if it's a custom tile), guess from the name!
            if (type == HiveTileType.None)
            {
                string tileName = tileBase.name.ToLower();
                string spriteName = sprite != null ? sprite.name.ToLower() : "";
                
                if (tileName.Contains("inside") || spriteName.Contains("inside") || tileName.Contains("tunnel")) type = HiveTileType.InsideHive;
                else if (tileName.Contains("brood") || spriteName.Contains("brood")) type = HiveTileType.Brood;
                else if (tileName.Contains("storage") || spriteName.Contains("storage") || tileName.Contains("honey")) type = HiveTileType.Storage;
                else type = HiveTileType.Hive; // Default to dirt walls!
            }

            types[pos]  = type;
            marked[pos] = false;
            found.Add((pos, type));
        }

        // Auto-detect the entrance: any InsideHive cell from the starting layout
        // that touches at least one empty neighbor is the entrance and is locked.
        foreach (var (pos, type) in found)
        {
            if (type != HiveTileType.InsideHive) continue;
            foreach (var d in Dirs)
            {
                if (!types.ContainsKey(pos + d)) { lockedCells.Add(pos); break; }
            }
        }

        // Now redraw each so the correct variant is chosen for its actual neighbors.
        foreach (var (pos, _) in found) visuals.RefreshAt(pos);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    private int GetTileCost(HiveTileType type)
    {
        switch (type)
        {
            case HiveTileType.InsideHive: return 5;
            case HiveTileType.Brood: return 10;
            case HiveTileType.Storage: return 10;
            default: return 0;
        }
    }

    /// <summary>
    /// Called by BuildPanel when the player clicks a grid cell.
    /// Returns true if the tile was accepted (adjacent to any existing tile).
    /// </summary>
    public bool TryMark(Vector3Int pos, HiveTileType type)
    {
        if (!CanBuildAt(pos, type))           return false;   // invalid location or already built

        int cost = GetTileCost(type);
        if (cost > 0)
        {
            if (CurrencyManager.Instance != null && !CurrencyManager.Instance.UseHoney(cost))
            {
                return false; // Not enough honey, popup is handled by CurrencyManager
            }
        }

        types[pos]  = type;
        marked[pos] = true;

        visuals?.SetMarked(pos, type);

        GameObject siteObj = new GameObject("TileConstruction_" + pos);
        siteObj.transform.position = CellToWorld(pos);
        
        BoxCollider2D col = siteObj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1, 1);
        col.isTrigger = true;
        
        ConstructionSite site = siteObj.AddComponent<ConstructionSite>();
        site.buildTime = 5f;
        site.isTileBuild = true;
        site.tilePos = pos;
        
        site.StartBuild();
        BuildManager.Instance.AddToQueue(site);

        OnTileMarked?.Invoke(pos);
        return true;
    }

    /// <summary>
    /// Spawns a Zone prefab and paints the visual tiles underneath it when construction finishes!
    /// </summary>
    public bool TryMarkZone(Vector3Int pos, GameObject prefab, HiveTileType visualType, int cost, float buildTime)
    {
        if (!CanBuildAt(pos, visualType)) return false;

        if (cost > 0)
        {
            if (CurrencyManager.Instance != null && !CurrencyManager.Instance.UseHoney(cost))
            {
                return false;
            }
        }

        types[pos]  = visualType;
        marked[pos] = true;
        visuals?.SetMarked(pos, visualType);

        GameObject siteObj = new GameObject("ZoneConstruction_" + pos);
        siteObj.transform.position = CellToWorld(pos);
        
        BoxCollider2D col = siteObj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1, 1);
        col.isTrigger = true;
        
        ConstructionSite site = siteObj.AddComponent<ConstructionSite>();
        site.buildTime = buildTime;
        site.isTileBuild = true; // Wait! If it's true, it just paints the tile. We need to instantiate the prefab!
        site.tilePos = pos;
        
        // Let's pass the prefab to the Grid to spawn it on completion.
        // We will store it in a dictionary to spawn when CompleteBuild is called.
        if (pendingZones == null) pendingZones = new System.Collections.Generic.Dictionary<Vector3Int, GameObject>();
        pendingZones[pos] = prefab;

        site.StartBuild();
        BuildManager.Instance.AddToQueue(site);

        OnTileMarked?.Invoke(pos);
        return true;
    }
    
    private System.Collections.Generic.Dictionary<Vector3Int, GameObject> pendingZones;

    public void ShowAllBuildableIndicators()
    {
        var validCells = new System.Collections.Generic.HashSet<Vector3Int>();
        foreach (var pos in types.Keys)
        {
            foreach (var d in Dirs)
            {
                var n = pos + d;
                // Check if any room can be built here
                if (CanBuildAt(n, HiveTileType.Brood)) validCells.Add(n);
            }
        }
        visuals?.ShowIndicators(validCells);
    }

#if UNITY_EDITOR
    private string GetExpectedSpriteName(bool wT, bool wB, bool wL, bool wR)
    {
        int count = (wT ? 1 : 0) + (wB ? 1 : 0) + (wL ? 1 : 0) + (wR ? 1 : 0);
        if (count == 0) return "Center";
        if (count == 4) return "Isolated";
        if (count == 1) {
            if (wT) return "Wall_Top";
            if (wB) return "Wall_Bottom";
            if (wL) return "Wall_Left";
            if (wR) return "Wall_Right";
        }
        if (count == 2) {
            if (wT && wB) return "Tunnel_Horizontal";
            if (wL && wR) return "Tunnel_Vertical";
            if (wT && wL) return "Corner_TopLeft";
            if (wT && wR) return "Corner_TopRight";
            if (wB && wL) return "Corner_BottomLeft";
            if (wB && wR) return "Corner_BottomRight";
        }
        if (count == 3) {
            if (!wB) return "DeadEnd_Top";   // Walls T, L, R
            if (!wT) return "DeadEnd_Bottom";// Walls B, L, R
            if (!wR) return "DeadEnd_Left";  // Walls T, B, L
            if (!wL) return "DeadEnd_Right"; // Walls T, B, R
        }
        return "Center";
    }

    [ContextMenu("1. Fix Tile Library")]
    private void FixLibrary()
    {
        var lib = GetComponent<HiveVisuals>()?.Library;
        if (lib == null) { Debug.LogError("HiveVisuals or Library missing!"); return; }

        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/05_Tiles" });
        var sprites = new System.Collections.Generic.List<Sprite>();
        foreach (var g in guids) sprites.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(UnityEditor.AssetDatabase.GUIDToAssetPath(g)));

        foreach (var set in lib.sets)
        {
            if (set.border == null || set.border.Length != 16) set.border = new Sprite[16];
            if (set.overlay == null || set.overlay.Length != 16) set.overlay = new Sprite[16];

            string hint = set.type.ToString();
            if (hint == "InsideHive") hint = "Inside";

            for (int i = 0; i < 16; i++)
            {
                // Border: Bit=1 means OPENING. Bit=0 means WALL.
                bool bWt = (i & 1) == 0;
                bool bWb = (i & 2) == 0;
                bool bWl = (i & 4) == 0;
                bool bWr = (i & 8) == 0;
                string borderName = GetExpectedSpriteName(bWt, bWb, bWl, bWr);

                // Overlay: Bit=1 means WALL. Bit=0 means OPENING.
                bool oWt = (i & 1) != 0;
                bool oWb = (i & 2) != 0;
                bool oWl = (i & 4) != 0;
                bool oWr = (i & 8) != 0;
                string overlayName = GetExpectedSpriteName(oWt, oWb, oWl, oWr);

                set.border[i] = sprites.Find(s => s.name == borderName && UnityEditor.AssetDatabase.GetAssetPath(s).Contains(hint) && !UnityEditor.AssetDatabase.GetAssetPath(s).ToLower().Contains("overlay"));
                set.overlay[i] = sprites.Find(s => (s.name == overlayName || s.name.EndsWith("_" + overlayName)) && UnityEditor.AssetDatabase.GetAssetPath(s).Contains(hint) && UnityEditor.AssetDatabase.GetAssetPath(s).ToLower().Contains("overlay"));
            }
        }
        UnityEditor.EditorUtility.SetDirty(lib);
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log("Library Fixed! The DeadEnds and Corners are now perfectly aligned! Now run Generate Starting Hive.");
    }

    [ContextMenu("2. Setup Overlay Tilemaps")]
    private void SetupOverlayTilemaps()
    {
        var visuals = GetComponent<HiveVisuals>();
        if (visuals == null) return;

        if (visuals.BuiltTilemap != null)
        {
            var builtRenderer = visuals.BuiltTilemap.GetComponent<UnityEngine.Tilemaps.TilemapRenderer>();
            if (builtRenderer != null) builtRenderer.sortingOrder = 0;
        }

        var overlayMaps = new UnityEngine.Tilemaps.Tilemap[4];
        for (int i = 0; i < 4; i++)
        {
            string layerName = "OverlayTilemap_" + i;
            var overlayObj = transform.Find(layerName);
            if (overlayObj == null)
            {
                var go = new GameObject(layerName);
                go.transform.SetParent(this.transform);
                go.transform.localPosition = Vector3.zero;
                go.AddComponent<UnityEngine.Tilemaps.Tilemap>();
                var renderer = go.AddComponent<UnityEngine.Tilemaps.TilemapRenderer>();
                renderer.sortingOrder = 10 + i; // 10, 11, 12, 13
                overlayObj = go.transform;
            }
            else
            {
                var renderer = overlayObj.GetComponent<UnityEngine.Tilemaps.TilemapRenderer>();
                if (renderer != null) renderer.sortingOrder = 10 + i;
            }
            overlayMaps[i] = overlayObj.GetComponent<UnityEngine.Tilemaps.Tilemap>();
        }

        var serializedObject = new UnityEditor.SerializedObject(visuals);
        var arrayProp = serializedObject.FindProperty("overlayTilemaps");
        arrayProp.arraySize = 4;
        for (int i = 0; i < 4; i++)
        {
            arrayProp.GetArrayElementAtIndex(i).objectReferenceValue = overlayMaps[i];
        }
        serializedObject.ApplyModifiedProperties();
        Debug.Log("4 Overlay Tilemaps setup and assigned to HiveVisuals!");
    }

    private UnityEngine.Tilemaps.Tile GetOrSavePersistentTile(Sprite sprite, HiveTileType type)
    {
        string path = UnityEditor.AssetDatabase.GetAssetPath(sprite);
        string dir = System.IO.Path.GetDirectoryName(path);
        string tilePath = System.IO.Path.Combine(dir, sprite.name + "_GeneratedTile.asset");
        
        var tile = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Tilemaps.Tile>(tilePath);
        if (tile == null)
        {
            tile = UnityEngine.ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
            tile.sprite = sprite;

            if (type == HiveTileType.Hive || type == HiveTileType.Solid)
                tile.colliderType = UnityEngine.Tilemaps.Tile.ColliderType.Grid;
            else
                tile.colliderType = UnityEngine.Tilemaps.Tile.ColliderType.None;

            UnityEditor.AssetDatabase.CreateAsset(tile, tilePath);
        }
        else
        {
            // Update existing tile in case it was generated with wrong collider before
            var expectedCollider = (type == HiveTileType.Hive || type == HiveTileType.Solid) 
                ? UnityEngine.Tilemaps.Tile.ColliderType.Grid 
                : UnityEngine.Tilemaps.Tile.ColliderType.None;
                
            if (tile.colliderType != expectedCollider)
            {
                tile.colliderType = expectedCollider;
                UnityEditor.EditorUtility.SetDirty(tile);
            }
        }
        return tile;
    }

    [ContextMenu("3. Generate Starting Hive")]
    private void GenerateStartingHive()
    {
        Debug.Log("Starting generation...");
        if (visuals == null) visuals = GetComponent<HiveVisuals>();
        if (visuals == null) { Debug.LogError("Visuals is null!"); return; }
        if (visuals.Library == null) { Debug.LogError("Library is null!"); return; }
        if (visuals.BuiltTilemap == null) { Debug.LogError("BuiltTilemap is null!"); return; }

        var builtMap = visuals.BuiltTilemap;
        var overlayMaps = visuals.OverlayTilemaps;
        var library = visuals.Library;
        
        library.Init();
        builtMap.ClearAllTiles();
        if (overlayMaps != null)
        {
            foreach (var map in overlayMaps) if (map != null) map.ClearAllTiles();
        }

        var tempGrid = new System.Collections.Generic.Dictionary<Vector3Int, HiveTileType>();

        // Rounded Hive (exact 9x9 circle matching image)
        for (int x = -4; x <= 4; x++)
        {
            for (int y = -4; y <= 4; y++)
            {
                if (Vector2.Distance(Vector2.zero, new Vector2(x, y)) <= 4.5f)
                {
                    tempGrid[new Vector3Int(x, y, 0)] = HiveTileType.Hive;
                }
            }
        }

        // 1-wide Tunnel (InsideHive) extending from the top down to the center
        for (int y = 1; y <= 4; y++)
        {
            tempGrid[new Vector3Int(0, y, 0)] = HiveTileType.InsideHive;
        }

        // 2x2 Brood Chamber exactly where the tunnel meets
        tempGrid[new Vector3Int(0, 0, 0)] = HiveTileType.Brood;
        tempGrid[new Vector3Int(1, 0, 0)] = HiveTileType.Brood;
        tempGrid[new Vector3Int(0, -1, 0)] = HiveTileType.Brood;
        tempGrid[new Vector3Int(1, -1, 0)] = HiveTileType.Brood;

        // 2x2 Storage Zone next to Brood Chamber (4 slots total)
        tempGrid[new Vector3Int(2, 0, 0)] = HiveTileType.Storage;
        tempGrid[new Vector3Int(2, -1, 0)] = HiveTileType.Storage;
        tempGrid[new Vector3Int(3, 0, 0)] = HiveTileType.Storage;
        tempGrid[new Vector3Int(3, -1, 0)] = HiveTileType.Storage;

        foreach (var kvp in tempGrid)
        {
            var pos = kvp.Key;
            var type = kvp.Value;

            int borderMask = 0;
            for (int i = 0; i < 4; i++)
            {
                var nType = tempGrid.ContainsKey(pos + Dirs[i]) ? tempGrid[pos + Dirs[i]] : HiveTileType.None;
                bool counterpart;
                if (type == HiveTileType.Hive) {
                    counterpart = (nType == HiveTileType.None);
                } else {
                    counterpart = (nType == HiveTileType.Hive || nType == HiveTileType.None);
                }
                if (!counterpart) borderMask |= (1 << i);
            }

            Sprite bSprite = library.GetBorderSprite(type, borderMask);
            if (bSprite != null)
            {
                var t = GetOrSavePersistentTile(bSprite, type);
                builtMap.SetTile(pos, t);
            }

            if (overlayMaps != null && type != HiveTileType.None)
            {
                // Group neighbors by their room type
                System.Collections.Generic.Dictionary<HiveTileType, int> neighborMasks = new System.Collections.Generic.Dictionary<HiveTileType, int>();
                for (int i = 0; i < 4; i++)
                {
                    var nType = tempGrid.ContainsKey(pos + Dirs[i]) ? tempGrid[pos + Dirs[i]] : HiveTileType.None;
                    if (nType != HiveTileType.None && nType != HiveTileType.Hive && nType != type)
                    {
                        if (!neighborMasks.ContainsKey(nType)) neighborMasks[nType] = 0;
                        neighborMasks[nType] |= (1 << i);
                    }
                }

                int layerIndex = 0;
                foreach (var nKvp in neighborMasks)
                {
                    var nType = nKvp.Key;
                    var mask = nKvp.Value;
                    var overlaySprite = library.GetOverlaySprite(nType, mask); // Fetch from neighbor's library!

                    if (overlaySprite != null && layerIndex < overlayMaps.Length)
                    {
                        if (overlayMaps[layerIndex] != null)
                        {
                            var ot = GetOrSavePersistentTile(overlaySprite, nType);
                            overlayMaps[layerIndex].SetTile(pos, ot);
                        }
                        layerIndex++;
                    }
                }
            }
        }

        UnityEditor.EditorUtility.SetDirty(builtMap);
        if (overlayMaps != null)
        {
            foreach (var map in overlayMaps) if (map != null) UnityEditor.EditorUtility.SetDirty(map);
        }

#if UNITY_EDITOR
        // Clear existing zones
        var existingZones = FindObjectsOfType<Zone>();
        foreach (var z in existingZones) DestroyImmediate(z.gameObject);

        // Instantiate prefabs
        var broodPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/03_Prefabs/BroodZone.prefab");
        var storagePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/03_Prefabs/StorageZone.prefab");
        
        if (broodPrefab != null)
        {
            var go1 = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(broodPrefab);
            go1.transform.position = builtMap.GetCellCenterWorld(new Vector3Int(0, 0, 0));
            var go2 = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(broodPrefab);
            go2.transform.position = builtMap.GetCellCenterWorld(new Vector3Int(1, 0, 0));
            var go3 = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(broodPrefab);
            go3.transform.position = builtMap.GetCellCenterWorld(new Vector3Int(0, -1, 0));
            var go4 = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(broodPrefab);
            go4.transform.position = builtMap.GetCellCenterWorld(new Vector3Int(1, -1, 0));
        }

        if (storagePrefab != null)
        {
            var go1 = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(storagePrefab);
            go1.transform.position = builtMap.GetCellCenterWorld(new Vector3Int(2, 0, 0));
            var go2 = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(storagePrefab);
            go2.transform.position = builtMap.GetCellCenterWorld(new Vector3Int(2, -1, 0));
            var go3 = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(storagePrefab);
            go3.transform.position = builtMap.GetCellCenterWorld(new Vector3Int(3, 0, 0));
            var go4 = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(storagePrefab);
            go4.transform.position = builtMap.GetCellCenterWorld(new Vector3Int(3, -1, 0));
        }

        var existingQueen = FindObjectOfType<QueenBee>();
        if (existingQueen == null)
        {
            var queenPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/03_Prefabs/Bees/QueenBee.prefab");
            if (queenPrefab != null)
            {
                var q = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(queenPrefab);
                q.transform.position = builtMap.GetCellCenterWorld(new Vector3Int(0, 0, 0));
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
#endif

        Debug.Log("Starting hive generated perfectly!");
    }
#endif

    /// <summary>
    /// Called when a builder bee finishes constructing a tile.
    /// </summary>
    public void CompleteBuild(Vector3Int pos)
    {
        if (!types.ContainsKey(pos)) return;

        marked[pos] = false;
        visuals?.ClearMarked(pos);
        Place(pos, types[pos]);
        
        if (pendingZones != null && pendingZones.ContainsKey(pos))
        {
            if (pendingZones[pos] != null)
            {
                Instantiate(pendingZones[pos], CellToWorld(pos), Quaternion.identity);
            }
            pendingZones.Remove(pos);
        }

        OnTileBuilt?.Invoke(pos);
    }

    public bool HasTile(Vector3Int pos)    => types.ContainsKey(pos);
    public bool IsMarked(Vector3Int pos)   => marked.TryGetValue(pos, out var m) && m;
    public bool IsLocked(Vector3Int pos)   => lockedCells.Contains(pos);
    public bool CanBuildAt(Vector3Int pos, HiveTileType toolType)
    {
        if (lockedCells.Contains(pos)) return false;

        var currentType = GetType(pos);

        if (toolType == HiveTileType.Hive)
        {
            if (currentType != HiveTileType.None) return false; // Dirt only on empty space
            return IsAdjacentToAny(pos);
        }
        else
        {
            // Rooms can ONLY be built by digging out existing Hive dirt
            if (currentType != HiveTileType.Hive) return false;
            if (IsMarked(pos)) return false; // already marked

            // Must touch an existing room (not dirt or empty) to expand the hive interior
            foreach (var d in Dirs)
            {
                var nType = GetType(pos + d);
                if (nType != HiveTileType.None && nType != HiveTileType.Hive)
                    return true;
            }
            return false;
        }
    }

    public void ShowBuildIndicators(HiveTileType toolType)
    {
        var validCells = new System.Collections.Generic.HashSet<Vector3Int>();
        
        foreach (var pos in types.Keys)
        {
            if (CanBuildAt(pos, toolType)) validCells.Add(pos);
            
            foreach (var d in Dirs)
            {
                var n = pos + d;
                if (CanBuildAt(n, toolType)) validCells.Add(n);
            }
        }
        visuals?.ShowIndicators(validCells);
    }

    public void HideBuildIndicators()
    {
        visuals?.ClearIndicators();
    }

    public HiveTileType GetType(Vector3Int pos) =>
        types.TryGetValue(pos, out var t) ? t : HiveTileType.None;

    public Vector3Int WorldToCell(Vector3 world) =>
        GetComponent<Grid>()?.WorldToCell(world) ?? Vector3Int.zero;

    public Vector3 CellToWorld(Vector3Int cell) =>
        GetComponent<Grid>()?.GetCellCenterWorld(cell) ?? Vector3.zero;

    public bool IsBlockingAt(Vector3 worldPos)
    {
        var cell = WorldToCell(worldPos);
        if (!types.TryGetValue(cell, out var t)) return false;
        if (marked.TryGetValue(cell, out var m) && m) return false;
        return t == HiveTileType.Solid || t == HiveTileType.Hive;
    }

    public IEnumerable<KeyValuePair<Vector3Int, HiveTileType>> GetAllTiles() => types;

    // ── Internals ─────────────────────────────────────────────────────────────

    void Place(Vector3Int pos, HiveTileType type)
    {
        if (!types.ContainsKey(pos)) types[pos] = type;
        if (!marked.ContainsKey(pos)) marked[pos] = false;

        visuals?.SetBuilt(pos, type);
    }

    bool IsAdjacentToAny(Vector3Int pos)
    {
        if (types.Count == 0) return true;
        foreach (var d in Dirs)
            if (types.ContainsKey(pos + d)) return true;
        return false;
    }
}
