using UnityEngine;

public class HouseBee : Bee
{
    [Header("Work")]
    public float workDistance = 1.2f;
    public float convertTime = 2f;
    public float workBuffer = 0.2f; // ✅ small buffer to prevent jitter

    [Header("Output")]
    public GameObject honeyPrefab;
    public int honeyAmount = 1;

    private HouseBeeZone zone;
    private Pollen target;

    private float workTimer;
    private bool hasStartedWorking = false;

    // Search retry
    private float searchTimer = 0f;
    public float searchInterval = 0.5f;

    public bool IsWorking => hasStartedWorking;

    protected override void Awake()
    {
        base.Awake();

        beeType = BeeType.House;

        zone = FindObjectOfType<HouseBeeZone>();

        if (zone != null)
        {
            homePosition = zone.transform.position;
        }
    }

    private System.Collections.IEnumerator Start()
    {
        yield return null; // allow pollen to register first
        currentState = BeeState.Idle;
    }

    // -------------------
    // STATES
    // -------------------

    protected override void IdleBehavior()
    {
        // 🔥 PRIORITY CHECK: If already working, don't interrupt
        if (currentState == BeeState.Working && hasStartedWorking)
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

        StayInZone();
    }

    protected override void WorkBehavior()
    {
        // ✅ ADDED (minimal): fully freeze logic while working
        if (hasStartedWorking)
        {
            workTimer -= Time.deltaTime;

            if (workTimer <= 0f)
            {
                Convert();
            }

            return;
        }

        if (target == null)
        {
            ResetBee();
            return;
        }

        float dist = Vector2.Distance(transform.position, target.transform.position);

        if (dist <= workDistance + workBuffer)
        {
            hasStartedWorking = true;
            workTimer = convertTime;

            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            Debug.Log("Bee started working on: " + target.name);
        }
        else
        {
            if (!hasStartedWorking)
            {
                targetPosition = target.transform.position;
                currentState = BeeState.Moving;
                return;
            }
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
            currentState = BeeState.Idle;
        }
    }

    // -------------------
    // ACTIONS
    // -------------------

    void Convert()
    {
        if (target == null) return;

        if (honeyPrefab != null)
        {
            Instantiate(honeyPrefab, transform.position, Quaternion.identity);
        }

        Destroy(target.gameObject);

        ResetBee();
    }

    void ResetBee()
    {
        hasStartedWorking = false;

        if (target != null)
        {
            target.isClaimed = false;
        }

        target = null;

        currentState = BeeState.Idle;
    }

    // -------------------
    // HELPERS
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

    void StayInZone()
    {
        if (zone == null) return;

        float dist = Vector2.Distance(transform.position, zone.transform.position);

        if (dist > zone.depositRadius)
        {
            targetPosition = zone.transform.position;
            currentState = BeeState.Moving;
        }
        else
        {
            if (Random.value < 0.01f)
            {
                targetPosition = zone.GetDepositPoint();
                currentState = BeeState.Moving;
            }
        }
    }

    protected override void ReturnBehavior() { }
}