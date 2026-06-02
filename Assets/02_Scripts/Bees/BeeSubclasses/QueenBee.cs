using UnityEngine;
using UnityEngine.UI;

public class QueenBee : Bee
{
    public static QueenBee Instance;

    public GameObject gameOverCanvas;

    [Header("UI")]
    public Slider healthSlider;

    [Header("Regeneration")]
    public float regenRate = 0.5f;
    public float regenDelay = 3f;

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

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    protected override void Update()
    {
        base.Update();

        HandleRegeneration();
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthSlider == null) return;

        healthSlider.value = currentHealth;
    }

    void HandleRegeneration()
    {
        if (currentState == BeeState.Dead)
            return;

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
    }

    protected override void WorkBehavior()
    {
        currentState = BeeState.Idle;
    }

    protected override void ReturnBehavior()
    {
        currentState = BeeState.Idle;
    }

    protected override void Die()
    {
        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);

        Time.timeScale = 0f;

        currentState = BeeState.Dead;
        Destroy(gameObject);
    }

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
}