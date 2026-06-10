using UnityEngine;

public class HouseBee : Bee
{
    [Header("Work")]
    public float workDistance = 1.2f;
    public float convertTime = 2f;
    public float workBuffer = 0.2f;

    [Header("Output")]
    public GameObject honeyPrefab;
    public int honeyAmount = 1;

    private HouseBeeZone zone;
    private Pollen target;

    private float workTimer;
    private bool hasStartedWorking = false;

    public bool IsWorking => hasStartedWorking;

    protected override void Awake()
    {
        base.Awake();
        beeType = BeeType.House;

        // Resources.Load works in builds too (AssetDatabase is Editor-only).
        if (honeyPrefab == null)
            honeyPrefab = Resources.Load<GameObject>("Prefabs/Honey");
    }

    protected override void Start()
    {
        base.Start();

        zone = assignedZone as HouseBeeZone;

        if (zone != null)
            homePosition = zone.transform.position;
    }

    protected override void Update()
    {
        base.Update();

        if (zone == null)
            zone = assignedZone as HouseBeeZone;
    }

    // ======================================================
    // JOB VALIDATION
    // ======================================================

    protected override bool HasWork()
    {
        if (CurrencyManager.Instance != null && !CurrencyManager.Instance.HasHoneySpace())
            return false; // Cannot make more honey!

        if (target != null) return true;

        if (Pollen.allPollen == null || Pollen.allPollen.Count == 0) return false;

        foreach (Pollen p in Pollen.allPollen)
        {
            if (p != null && !p.isClaimed) return true;
        }

        return false;
    }

    protected override void OnJobFound()
    {
        if (target == null)
        {
            Pollen p = FindAvailablePollen();
            if (p != null)
            {
                target = p;
                target.isClaimed = true;
                targetPosition = target.transform.position;
                MarkAsWorking();
                currentState = BeeState.Moving;
            }
        }
        else
        {
            MarkAsWorking();
            targetPosition = target.transform.position;
            currentState = BeeState.Moving;
        }
    }

    // ======================================================
    // IDLE
    // ======================================================

    protected override void IdleBehavior()
    {
        if (hasStartedWorking) return;

        if (target == null)
        {
            Pollen p = FindAvailablePollen();
            if (p != null)
            {
                target = p;
                target.isClaimed = true;
                targetPosition = target.transform.position;
                MarkAsWorking();
                currentState = BeeState.Moving;
            }
        }
        else
        {
            targetPosition = target.transform.position;
            MarkAsWorking();
            currentState = BeeState.Moving;
        }
    }

    // ======================================================
    // WORK
    // ======================================================

    protected override void WorkBehavior()
    {
        if (target == null)
        {
            ResetBee();
            return;
        }

        if (hasStartedWorking)
        {
            workTimer -= Time.deltaTime;
            if (workTimer <= 0f) Convert();
            return;
        }

        float dist = Vector2.Distance(transform.position, target.transform.position);

        if (dist <= workDistance + workBuffer)
        {
            hasStartedWorking = true;
            workTimer = convertTime;
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
        else
        {
            targetPosition = target.transform.position;
            currentState = BeeState.Moving;
        }
    }

    protected override void OnReachedTarget()
    {
        if (target != null)
        {
            currentState = BeeState.Working;
            return;
        }

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

    // ======================================================
    // CONVERT
    // ======================================================

    void Convert()
    {
        if (target != null)
        {
            int convertedAmount = UnityEngine.Random.Range(2, 4);

            if (CurrencyManager.Instance != null && CurrencyManager.Instance.pollen > 0)
            {
                if (CurrencyManager.Instance.pollen < convertedAmount)
                    convertedAmount = CurrencyManager.Instance.pollen;

                CurrencyManager.Instance.UsePollen(convertedAmount);
            }

            Vector2 spawnPos = target.transform.position;
            Destroy(target.gameObject);

            if (honeyPrefab != null)
            {
                GameObject h = Instantiate(honeyPrefab, spawnPos, Quaternion.identity);
                h.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
                SpriteRenderer sr = h.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sortingOrder = 15; // Above bees (10)

                // Make it interactable!
                if (h.GetComponent<Collider2D>() == null)
                    h.AddComponent<CircleCollider2D>().radius = 0.5f;
                
                ClickableHoney clickScript = h.AddComponent<ClickableHoney>();
                clickScript.amount = convertedAmount;
            }
            else
            {
                Debug.LogWarning("Honey Prefab is NULL!");
                if (CurrencyManager.Instance != null)
                    CurrencyManager.Instance.AddHoney(convertedAmount);
            }
        }

        ResetBee();
    }

    void ResetBee()
    {
        hasStartedWorking = false;
        target = null;
        currentState = BeeState.Idle;
    }

    // ======================================================
    // POLLEN SEARCH
    // ======================================================

    Pollen FindAvailablePollen()
    {
        if (Pollen.allPollen == null || Pollen.allPollen.Count == 0) return null;

        Pollen best = null;
        float bestDist = Mathf.Infinity;

        foreach (Pollen p in Pollen.allPollen)
        {
            if (p == null || p.isClaimed) continue;

            float dist = Vector2.Distance(transform.position, p.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = p;
            }
        }

        return best;
    }

    protected override void ReturnBehavior() { }
    protected override void Die() => base.Die();

    public override void StopDragging()
    {
        hasStartedWorking = false;

        if (target != null)
            target.isClaimed = false;

        target = null;
        base.StopDragging();
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
}