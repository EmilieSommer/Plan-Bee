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
        // Already holding a valid target
        if (target != null) return true;

        // Check if any unclaimed pollen exists
        if (Pollen.allPollen == null || Pollen.allPollen.Count == 0) return false;

        foreach (Pollen p in Pollen.allPollen)
        {
            if (p != null && !p.isClaimed) return true;
        }

        return false;
    }

    protected override void OnJobFound()
    {
        // Heartbeat found work while idle — try to grab a target
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
            // Had a target but drifted idle — go back to it
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

        // Try to find pollen — if none, job system handles going home
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
        }
        else
        {
            // Let base handle home freeze if no work
            base.OnReachedTarget();
        }
    }

    // ======================================================
    // CONVERT
    // ======================================================

    void Convert()
    {
        if (target == null) return;

        if (honeyPrefab != null)
            Instantiate(honeyPrefab, transform.position, Quaternion.identity);

        Destroy(target.gameObject);
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

    protected override void GoHome()
    {
        SleepZone sleep = FindNearestSleepZone();

        Vector2 destination = sleep != null
            ? (Vector2)sleep.transform.position
            : homePosition;

        float dist = Vector2.Distance(rb.position, destination);

        if (dist < 0.3f)
        {
            isAtHome = true;
            StopMovementInstant();
            currentState = BeeState.Idle;
        }
        else
        {
            targetPosition = destination;
            currentState = BeeState.Moving;
        }
    }
}