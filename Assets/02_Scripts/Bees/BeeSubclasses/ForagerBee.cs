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

    private Zone currentZone;

    protected override void Awake()
    {
        base.Awake();
        beeType = BeeType.Forager;
        baseMoveSpeed = moveSpeed;

        // Resources.Load works in builds too (AssetDatabase is Editor-only).
        if (pollenPrefab == null)
            pollenPrefab = Resources.Load<GameObject>("Prefabs/Pollen");
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
        if (CurrencyManager.Instance != null && !CurrencyManager.Instance.HasPollenSpace())
            return; // Wait until there is space for pollen!

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

        currentZone = FindNearestStorageZone();

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        targetPosition = (Vector2)transform.position + randomDir * 20f;
        
        Debug.Log($"[{beeName}] StartForaging: targetPosition={targetPosition}. Path count={(currentPath != null ? currentPath.Count : 0)}");

        currentState = BeeState.Moving;
    }

    void StartReturning()
    {
        isOutForaging = false;

        currentZone = FindNearestStorageZone();
        targetPosition = currentZone != null ? currentZone.transform.position : homePosition;
        
        Debug.Log($"[{beeName}] StartReturning: targetPosition={targetPosition}. Zone={(currentZone != null ? currentZone.name : "null")}. Path count={(currentPath != null ? currentPath.Count : 0)}");
        
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

            Debug.Log($"[{beeName}] Reached hive! Depositing pollen.");
            DepositPollen();
        }
        else
        {
            MoveToTarget();
            if (currentVelocity.sqrMagnitude == 0)
            {
                // Fallback: If pathfinding fails, fly in a straight line back!
                Vector2 dir = (targetPosition - (Vector2)transform.position).normalized;
                rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
                RotateTowardsMovement(dir);
            }
        }
    }

    void RotateTowardsMovement(Vector2 dir)
    {
        if (dir.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }

    void DepositPollen()
    {
        Debug.Log($"[{beeName}] DepositPollen called. prefab: {(pollenPrefab != null ? "EXISTS" : "NULL")}, zone: {(currentZone != null ? currentZone.name : "NULL")}");

        Vector2 pos = transform.position;
        if (pollenPrefab != null && currentZone != null)
        {
            pos = currentZone.GetDepositPoint();
            GameObject p = Instantiate(pollenPrefab, pos, Quaternion.identity);
            p.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            SpriteRenderer sr = p.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = 4; // Above hive (0-3), below bees (10)
            
            Debug.Log($"[{beeName}] Instantiated Pollen!");
        }

        if (CurrencyManager.Instance != null)
        {
            int amount = UnityEngine.Random.Range(2, 4); // 2 to 3
            CurrencyManager.Instance.AddPollen(amount);
        }

        StartForaging();
    }

    Zone FindNearestStorageZone()
    {
        Zone[] zones = FindObjectsOfType<Zone>();

        float closestDist = Mathf.Infinity;
        Zone closest = null;

        foreach (var zone in zones)
        {
            if (!zone.isStorageZone) continue;

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