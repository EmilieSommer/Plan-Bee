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

    private float searchTimer = 0f;
    public float searchInterval = 0.5f;

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

        TryAcquireInitialTarget();
    }

    protected override void Update()
    {
        base.Update();

        if (zone == null)
            zone = assignedZone as HouseBeeZone;

        CheckForWorkContinuously();
    }

    // -------------------
    // INITIAL TARGET
    // -------------------
    void TryAcquireInitialTarget()
    {
        target = FindAvailablePollen();

        if (target != null)
        {
            target.isClaimed = true;
            targetPosition = target.transform.position;
            currentState = BeeState.Moving;
        }
    }

    // -------------------
    // IDLE
    // -------------------
    protected override void IdleBehavior()
    {
        if (hasStartedWorking)
            return;

        searchTimer -= Time.deltaTime;

        if (searchTimer <= 0f)
        {
            searchTimer = searchInterval;

            if (target == null)
            {
                target = FindAvailablePollen();

                if (target != null)
                {
                    target.isClaimed = true;
                    targetPosition = target.transform.position;
                    currentState = BeeState.Moving;
                    return;
                }
            }
        }

        // Smooth idle roaming like NurseBee
        base.IdleBehavior();

        // Keep roaming centered around house zone
        if (zone != null)
        {
            homePosition = zone.transform.position;
            roamRadius = zone.depositRadius;
        }
    }

    void CheckForWorkContinuously()
    {
        if (currentState == BeeState.Dead || currentState == BeeState.Working)
            return;

        if (target == null)
        {
            Pollen newTarget = FindAvailablePollen();

            if (newTarget != null)
            {
                target = newTarget;
                target.isClaimed = true;
                targetPosition = target.transform.position;
                currentState = BeeState.Moving;
            }
        }
    }

    // -------------------
    // WORK
    // -------------------
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

            if (workTimer <= 0f)
                Convert();

            return;
        }

        float dist = Vector2.Distance(transform.position, target.transform.position);

        if (dist <= workDistance + workBuffer)
        {
            hasStartedWorking = true;
            workTimer = convertTime;

            if (rb != null)
                rb.linearVelocity = Vector2.zero;
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
            currentState = BeeState.Working;
        else
            currentState = BeeState.Idle;
    }

    // -------------------
    // CONVERT
    // -------------------
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

    // -------------------
    // POLLEN SEARCH
    // -------------------
    Pollen FindAvailablePollen()
    {
        Pollen best = null;
        float bestDist = Mathf.Infinity;

        if (Pollen.allPollen == null || Pollen.allPollen.Count == 0)
            return null;

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

    protected override void Die()
    {
        base.Die();
    }

    public override void StopDragging()
    {
        hasStartedWorking = false;

        if (target != null)
            target.isClaimed = false;

        target = null;

        base.StopDragging();
    }
}