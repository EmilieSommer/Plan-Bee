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
    }

    protected override void Start()
    {
        AssignZone();

        // Queen starts frozen at home immediately
        MarkAsWorking();

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
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