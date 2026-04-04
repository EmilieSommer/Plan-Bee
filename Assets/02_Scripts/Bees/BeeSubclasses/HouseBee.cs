using UnityEngine;

public class HouseBee : Bee
{
    [Header("Work")]
    public float workDistance = 1.2f;
    public float convertTime = 2f;

    [Header("Output")]
    public GameObject honeyPrefab;
    public int honeyAmount = 1;

    private HouseBeeZone zone;
    private Pollen target;

    private float workTimer;
    private bool isWorking = false;
    private bool hasStartedWorking = false;

    // Search retry
    private float searchTimer = 0f;
    public float searchInterval = 0.5f;

    protected override void Awake()
    {
        base.Awake();

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
        if (target == null)
        {
            ResetBee();
            return;
        }

        float dist = Vector2.Distance(transform.position, target.transform.position);

        // ✅ LOCK INTO WORK STATE ONCE IN RANGE
        if (dist <= workDistance)
        {
            if (!hasStartedWorking)
            {
                hasStartedWorking = true;
                isWorking = true;
                workTimer = convertTime;

                Debug.Log("Bee started working on: " + target.name);
            }

            workTimer -= Time.deltaTime;

            if (workTimer <= 0f)
            {
                Convert();
            }
        }
        else
        {
            // Move toward pollen WITHOUT constantly flipping state
            targetPosition = target.transform.position;

            if (currentState != BeeState.Moving)
            {
                currentState = BeeState.Moving;
            }

            // Reset work progress if we leave range
            hasStartedWorking = false;
            isWorking = false;
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
        isWorking = false;
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