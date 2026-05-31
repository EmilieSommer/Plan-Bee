using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

/// <summary>
/// Lets the player paint Rule Tiles onto the world tilemap at runtime.
/// Activate via the UI (call Activate()), then:
///   Left-click  — place tile (costs pollen)
///   Right-click — remove tile (refunds pollen)
///   Escape      — cancel mode
///
/// Assign in Inspector:
///   worldTilemap  — the Tilemap to paint on
///   ruleTile      — the StoneFloor_RuleTile (or any RuleTile)
///   ghostRenderer — a SpriteRenderer child for the placement preview
/// </summary>
public class WorldTilePainter : MonoBehaviour
{
    public static WorldTilePainter Instance;

    [Header("Tilemap")]
    [SerializeField] private Tilemap worldTilemap;

    [Header("Rule Tile")]
    [SerializeField] private RuleTile ruleTile;

    [Header("Cost")]
    [SerializeField] private int pollenCost = 5;

    [Header("Preview")]
    [SerializeField] private SpriteRenderer ghostRenderer;

    [Header("Toggle Key (for testing)")]
    [SerializeField] private KeyCode toggleKey = KeyCode.B;

    private bool _active;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            Toggle();

        if (!_active) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;
        Vector3Int cell = worldTilemap.WorldToCell(worldPos);

        UpdateGhost(cell);

        // Cancel
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1) && !HasTileAt(cell))
        {
            Deactivate();
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // Right-click on an existing tile → remove + refund
        if (Input.GetMouseButtonDown(1))
        {
            if (HasTileAt(cell))
            {
                worldTilemap.SetTile(cell, null);
                CurrencyManager.Instance?.AddPollen(pollenCost);
            }
            return;
        }

        // Left-click → place
        if (Input.GetMouseButtonDown(0))
        {
            if (HasTileAt(cell)) return;
            if (CurrencyManager.Instance != null && !CurrencyManager.Instance.UsePollen(pollenCost)) return;

            worldTilemap.SetTile(cell, ruleTile);
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Activate()
    {
        _active = true;
        if (ghostRenderer != null)
        {
            ghostRenderer.sprite  = ruleTile != null ? ruleTile.m_DefaultSprite : null;
            ghostRenderer.enabled = true;
        }
    }

    public void Deactivate()
    {
        _active = false;
        if (ghostRenderer != null)
            ghostRenderer.enabled = false;
    }

    public void Toggle() => SetActive(!_active);

    public void SetActive(bool value)
    {
        if (value) Activate(); else Deactivate();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    void UpdateGhost(Vector3Int cell)
    {
        if (ghostRenderer == null) return;
        ghostRenderer.transform.position = worldTilemap.GetCellCenterWorld(cell);

        // Tint red if occupied or unaffordable, white otherwise
        bool blocked = HasTileAt(cell) ||
                       (CurrencyManager.Instance != null &&
                        CurrencyManager.Instance.pollen < pollenCost);

        ghostRenderer.color = blocked
            ? new Color(1f, 0.3f, 0.3f, 0.5f)
            : new Color(1f, 1f, 1f, 0.5f);
    }

    bool HasTileAt(Vector3Int cell) => worldTilemap.HasTile(cell);
}
