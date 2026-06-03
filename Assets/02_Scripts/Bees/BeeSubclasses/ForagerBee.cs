using UnityEngine;

public class ForagerBee : Bee
{
    [Header("Foraging")]
    public float foragingTime = 5f;
    public int pollenPerTrip = 5;

    [Header("Prefabs")]
    public GameObject pollenPrefab;

    [Header("Weather Effects")]
    public float rainSlowMultiplier = 2f;

    private float timer;
    private float baseMoveSpeed;

    private bool isOutForaging = false;
    private bool shelteringFromSnow = false;

    private HouseBeeZone currentZone;

    protected override void Awake()
    {
        base.Awake();
        beeType = BeeType.Forager;
        baseMoveSpeed = moveSpeed;
    }

    protected override void Start()
    {
        base.Start();
        StartForaging();
    }

    // ======================================================
    // JOB VALIDATION
    // ======================================================

    protected override bool HasWork()
    {
        return WinterSystem.Instance == null || !WinterSystem.Instance.isSnowing;
    }

    protected override void OnJobFound()
    {
        if (!isOutForaging && !shelteringFromSnow)
            StartForaging();
    }

    // ======================================================
    // UPDATE
    // ======================================================

    protected override void Update()
    {
        if (currentState == BeeState.Dead)
            return;

        if (WinterSystem.Instance != null && WinterSystem.Instance.isSnowing)
        {
            HandleSnow();
            if (currentState != BeeState.Working)
                base.Update();
            return;
        }

        if (shelteringFromSnow)
        {
            shelteringFromSnow = false;
            StartForaging();
            return;
        }

        // Apply weather slow on top of base speed
        float weatherSlow = IsRaining() ? rainSlowMultiplier : 1f;
        moveSpeed = baseMoveSpeed / weatherSlow;

        if (isOutForaging)
        {
            timer -= Time.deltaTime / weatherSlow;
            if (timer <= 0f)
                StartReturning();
            return;
        }

        base.Update();
    }

    bool IsRaining()
    {
        return RainSystem.Instance != null &&
               RainSystem.Instance.GetCurrentEmission() > 0f;
    }

    // ======================================================
    // SNOW
    // ======================================================

    void HandleSnow()
    {
        if (!shelteringFromSnow)
        {
            shelteringFromSnow = true;
            isOutForaging = false;

            SleepZone sleep = FindNearestSleepZone();

            if (sleep != null)
            {
                ReserveSleep(sleep);
                targetPosition = sleep.transform.position;
            }
            else
            {
                targetPosition = homePosition;
            }

            currentState = BeeState.Returning;
        }

        if (Vector2.Distance(transform.position, targetPosition) < 0.2f)
        {
            SleepZone sleep = GetSleepZoneAtPosition(rb.position);
            if (sleep != null) RegisterSleep(sleep);
            StopMovementInstant();
            currentState = BeeState.Working;
        }
    }

    // ======================================================
    // FORAGING CYCLE
    // ======================================================

    void StartForaging()
    {
        isOutForaging = true;
        shelteringFromSnow = false;
        timer = foragingTime;

        MarkAsWorking();

        currentZone = FindNearestHouseZone();

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        targetPosition = (Vector2)transform.position + randomDir * 20f;

        currentState = BeeState.Moving;
    }

    void StartReturning()
    {
        isOutForaging = false;
        currentZone = FindNearestHouseZone();
        targetPosition = currentZone != null ? currentZone.transform.position : homePosition;
        currentState = BeeState.Returning;
    }

    protected override void ReturnBehavior()
    {
        float dist = Vector2.Distance(transform.position, targetPosition);

        if (dist < 0.2f)
        {
            if (WinterSystem.Instance != null && WinterSystem.Instance.isSnowing)
            {
                currentState = BeeState.Working;
                return;
            }

            DepositPollen();
        }
    }

    void DepositPollen()
    {
        if (pollenPrefab != null && currentZone != null)
            Instantiate(pollenPrefab, currentZone.GetDepositPoint(), Quaternion.identity);

        CurrencyManager.Instance.pollen += pollenPerTrip;

        StartForaging();
    }

    HouseBeeZone FindNearestHouseZone()
    {
        HouseBeeZone[] zones = FindObjectsOfType<HouseBeeZone>();

        float closestDist = Mathf.Infinity;
        HouseBeeZone closest = null;

        foreach (var zone in zones)
        {
            if (!zone.IsActive) continue;

            float dist = Vector2.Distance(transform.position, zone.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = zone;
            }
        }

        return closest;
    }

    // ======================================================
    // MITE SLOW — affects baseMoveSpeed so weather calc respects it
    // ======================================================

    public void ApplyMiteSlow(float multiplier)
    {
        baseMoveSpeed *= multiplier;
    }

    public void RemoveMiteSlow(float restoredBaseSpeed)
    {
        baseMoveSpeed = restoredBaseSpeed;
    }

    // ======================================================
    // SPEED UPGRADE
    // ======================================================

    public override void UpgradeSpeed(float amount, float max)
    {
        baseMoveSpeed = Mathf.Min(baseMoveSpeed + amount, max);
    }

    // ======================================================
    // SLEEP
    // ======================================================

    protected override void GoHome()
    {
        SleepZone sleep = FindNearestSleepZone();

        if (sleep == null)
        {
            isAtHome = true;
            StopMovementInstant();
            currentState = BeeState.Idle;
            return;
        }

        ReserveSleep(sleep);

        float dist = Vector2.Distance(rb.position, sleep.transform.position);

        if (dist < 0.3f)
        {
            isAtHome = true;
            StopMovementInstant();
            currentState = BeeState.Idle;
            RegisterSleep(sleep);
        }
        else
        {
            targetPosition = sleep.transform.position;
            currentState = BeeState.Moving;
        }
    }

    protected override void OnReachedTarget()
    {
        SleepZone zone = GetSleepZoneAtPosition(rb.position);
        if (zone != null)
        {
            if (zone.HasSpace || zone.IsRegistered(this))
            {
                RegisterSleep(zone);
                isAtHome = true;
                StopMovementInstant();
                currentState = BeeState.Idle;
            }
            else
            {
                isAtHome = false;
                GoHome();
            }
            return;
        }

        base.OnReachedTarget();
    }

    protected override void WorkBehavior() { }
    protected override void Die() => base.Die();
}