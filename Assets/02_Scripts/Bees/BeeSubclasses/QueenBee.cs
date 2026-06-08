using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class QueenBee : Bee
{
    public static QueenBee Instance;

    public GameObject gameOverCanvas;

    [Header("UI")]
    public Slider healthSlider;

    /*[Header("Regeneration")]
    public float regenRate = 0.5f;
    public float regenDelay = 3f;*/

    [Header("Defense")]
    public float protectionRadius = 8f;
    public float dangerMultiplier = 2f;

    private float regenTimer = 0f;

    protected override void Awake()
    {
        base.Awake();
        beeType = BeeType.Queen;
        Instance = this;
        
        // Automatically scale Queen to exactly 35/32 of a tile
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            float targetSize = 35f / 32f;
            float currentSize = sr.sprite.bounds.size.x;
            if (currentSize > 0)
            {
                float scale = targetSize / currentSize;
                transform.localScale = new Vector3(scale, scale, 1f);
            }
        }
        else
        {
            transform.localScale = Vector3.one;
        }

#if UNITY_EDITOR
        if (foragerPrefab == null)
            foragerPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/03_Prefabs/ForagerBee.prefab");
        if (nursePrefab == null)
            nursePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/03_Prefabs/NurseBee.prefab");
        if (housePrefab == null)
            housePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/03_Prefabs/HouseBee.prefab");
        if (builderPrefab == null)
            builderPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/03_Prefabs/TestPrefabs/BuilderBee.prefab");
#endif
    }

    [Header("Starting Bees")]
    public GameObject foragerPrefab;
    public GameObject nursePrefab;
    public GameObject housePrefab; // Worker
    public GameObject builderPrefab;

    protected override void Start()
    {
        // Snap to the Brood now that HiveGrid has finished waking up!
        AssignZone();

        // Queen starts frozen at home immediately
        MarkAsWorking();

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        SpawnStartingBees();
    }

    private void SpawnStartingBees()
    {
        if (foragerPrefab != null)
        {
            Instantiate(foragerPrefab, transform.position, Quaternion.identity);
        }
        
        if (nursePrefab != null)
        {
            Instantiate(nursePrefab, transform.position, Quaternion.identity);
        }

        if (housePrefab != null)
        {
            Instantiate(housePrefab, transform.position, Quaternion.identity);
        }

        if (builderPrefab != null)
        {
            Instantiate(builderPrefab, transform.position, Quaternion.identity);
        }
    }

    // ======================================================
    // JOB VALIDATION
    // ======================================================

    // Queen never leaves — she always "has work" so she never roams
    protected override bool HasWork() => true;

    protected override void OnJobFound()
    {
        // Always stay at home position
        MarkAsWorking();
        StopMovementInstant();
        currentState = BeeState.Idle;
    }

    // ======================================================
    // UPDATE
    // ======================================================

    protected override void Update()
    {
        base.Update();
        //HandleRegeneration();
        UpdateHealthUI();
    }

    // ======================================================
    // IDLE — Queen never roams, always stays put
    // ======================================================

    protected override void IdleBehavior()
    {
        StopMovementInstant();
        lockMovement = true;
    }

    // ======================================================
    // HEALTH UI
    // ======================================================

    void UpdateHealthUI()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth;
    }

    // ======================================================
    // REGENERATION
    // ======================================================

    /*void HandleRegeneration()
    {
        if (currentState == BeeState.Dead) return;

        if (currentHealth < maxHealth)
        {
            regenTimer += Time.deltaTime;

            if (regenTimer >= regenDelay)
            {
                currentHealth += regenRate * Time.deltaTime;
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }
        }
        else
        {
            regenTimer = 0f;
        }
    }*/

    // ======================================================
    // WORK / RETURN — Queen never moves
    // ======================================================

    protected override void WorkBehavior()
    {
        StopMovementInstant();
        currentState = BeeState.Idle;
    }

    protected override void ReturnBehavior()
    {
        StopMovementInstant();
        currentState = BeeState.Idle;
    }

    // ======================================================
    // DEATH
    // ======================================================

    protected override void Die()
    {
        if (GameLoopManager.Instance != null)
        {
            GameLoopManager.Instance.TriggerGameOver("The Queen has been killed by an enemy! The colony has fallen.");
        }
        else
        {
            if (gameOverCanvas != null)
                gameOverCanvas.SetActive(true);
            Time.timeScale = 0f;
        }

        currentState = BeeState.Dead;
        Destroy(gameObject);
    }

    // ======================================================
    // ZONE
    // ======================================================

    protected override void AssignZone()
    {
        // 1. Check if there is an actual NurseBeeZone prefab
        NurseBeeZone[] zones = FindObjectsOfType<NurseBeeZone>();
        if (zones.Length > 0)
        {
            NurseBeeZone best = zones[0];
            float bestDist = Vector2.Distance(transform.position, best.transform.position);
            foreach (var z in zones)
            {
                float d = Vector2.Distance(transform.position, z.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = z;
                }
            }
            assignedZone = best;
            homePosition = best.transform.position;
            transform.position = homePosition;
            best.RegisterBee(this);
            if (ZoneManager.Instance != null) ZoneManager.Instance.RegisterBee(this);
            return;
        }

        // 2. If no prefab exists (e.g. starting hand-painted hive), scan the Grid directly!
        HiveGrid grid = HiveGrid.Instance != null ? HiveGrid.Instance : FindObjectOfType<HiveGrid>();
        if (grid != null)
        {
            Vector3Int bestCell = Vector3Int.zero;
            float bestDist = float.MaxValue;
            bool found = false;

            foreach (var kvp in grid.GetAllTiles())
            {
                if (kvp.Value == HiveTileType.Brood)
                {
                    Vector3 cellWorldPos = grid.CellToWorld(kvp.Key);
                    float d = Vector2.Distance(transform.position, cellWorldPos);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        bestCell = kvp.Key;
                        found = true;
                    }
                }
            }

            if (found)
            {
                homePosition = grid.CellToWorld(bestCell);
                transform.position = homePosition; // Snap perfectly into the tile!
            }
        }
    }

    // Queen cannot be dragged
    public override void StopDragging()
    {
        isBeingDragged = false;
        StopMovementInstant();
        lockMovement = true;
        currentState = BeeState.Idle;
    }
}