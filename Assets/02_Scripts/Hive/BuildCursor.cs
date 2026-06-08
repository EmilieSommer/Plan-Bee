using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Displays a hover tile under the cursor when build mode is active.
/// Valid cells show a green hover; invalid cells show red.
/// Clicking an invalid cell briefly flashes the pressed sprite.
/// </summary>
public class BuildCursor : MonoBehaviour
{
    public static BuildCursor Instance { get; private set; }

    [SerializeField] private BuildPanel buildPanel;
    [SerializeField] private Tilemap hoverTilemap;

    [Header("Cursor Sprites")]
    [SerializeField] private Sprite validHoverSprite;
    [SerializeField] private Sprite invalidHoverSprite;
    [SerializeField] private Sprite invalidPressedSprite;

    private Tile _validTile;
    private Tile _invalidTile;
    private Tile _pressedTile;

    private Vector3Int _currentCell;
    private bool _hasCell;
    private Coroutine _flashRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        // Keep it underneath the bees (10), but above the other grid visuals (0-3)
        if (hoverTilemap != null) hoverTilemap.GetComponent<UnityEngine.Tilemaps.TilemapRenderer>().sortingOrder = 5;

        _validTile   = MakeTile(validHoverSprite);
        _invalidTile = MakeTile(invalidHoverSprite);
        _pressedTile = MakeTile(invalidPressedSprite);
    }

    void Update()
    {
        if (buildPanel == null || buildPanel.Selected == HiveTileType.None)
        {
            ClearHover();
            return;
        }

        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0f;
        Vector3Int cell = HiveGrid.Instance.WorldToCell(world);

        if (!_hasCell || cell != _currentCell)
        {
            if (_hasCell) hoverTilemap.SetTile(_currentCell, null);
            _currentCell = cell;
            _hasCell = true;
        }

        if (_flashRoutine == null)
        {
            bool valid = HiveGrid.Instance.CanBuildAt(cell, buildPanel.Selected);
            hoverTilemap.SetTile(cell, valid ? _validTile : _invalidTile);
        }
    }

    public void FlashInvalid(Vector3Int cell)
    {
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        _flashRoutine = StartCoroutine(DoFlash(cell));
    }

    IEnumerator DoFlash(Vector3Int cell)
    {
        hoverTilemap.SetTile(cell, _pressedTile);
        yield return new WaitForSeconds(0.15f);
        hoverTilemap.SetTile(cell, _invalidTile);
        _flashRoutine = null;
    }

    void ClearHover()
    {
        if (!_hasCell) return;
        hoverTilemap.SetTile(_currentCell, null);
        _hasCell = false;
    }

    void OnDisable() => ClearHover();

    static Tile MakeTile(Sprite sprite)
    {
        if (sprite == null) return null;
        var t = ScriptableObject.CreateInstance<Tile>();
        t.sprite = sprite;
        t.colliderType = Tile.ColliderType.None;
        return t;
    }
}
