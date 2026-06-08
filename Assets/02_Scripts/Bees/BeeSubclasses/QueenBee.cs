using UnityEngine;
using UnityEngine.UI;

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
        base.Start();

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
        if (foragerPrefab != null) Instantiate(foragerPrefab, transform.position, Quaternion.identity);
        if (nursePrefab != null) Instantiate(nursePrefab, transform.position, Quaternion.identity);
        if (housePrefab != null) Instantiate(housePrefab, transform.position, Quaternion.identity);
        if (builderPrefab != null) Instantiate(builderPrefab, transform.position, Quaternion.identity);
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
        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);

        Time.timeScale = 0f;
        currentState = BeeState.Dead;
        Destroy(gameObject);
    }

    // ======================================================
    // ZONE
    // ======================================================

    protected override void AssignZone()
    {
        QueenZone queenZone = FindObjectOfType<QueenZone>();
        if (queenZone == null) return;

        assignedZone = queenZone;
        homePosition = queenZone.transform.position;
        queenZone.RegisterBee(this);

        if (ZoneManager.Instance != null)
            ZoneManager.Instance.RegisterBee(this);
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