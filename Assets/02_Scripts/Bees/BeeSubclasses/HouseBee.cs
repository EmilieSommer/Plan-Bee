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

    protected override void Awake()
    {
        base.Awake();

        zone = FindObjectOfType<HouseBeeZone>();

        if (zone != null)
        {
            homePosition = zone.transform.position;
        }
    }

    protected override void IdleBehavior()
    {
        // ✅ ONLY pick new target if none
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

        // 🐝 No pollen → stay inside zone
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

        // Move closer if needed
        if (dist > workDistance)
        {
            targetPosition = target.transform.position;
            currentState = BeeState.Moving;
            return;
        }

        // Start working
        if (!isWorking)
        {
            isWorking = true;
            workTimer = convertTime;
        }

        workTimer -= Time.deltaTime;

        if (workTimer <= 0f)
        {
            Convert();
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

    void Convert()
    {
        if (target == null) return;

        // Spawn honey
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

        if (target != null)
        {
            target.isClaimed = false;
        }

        target = null;

        currentState = BeeState.Idle;
    }

    // -------------------
    // Helpers
    // -------------------

    Pollen FindAvailablePollen()
    {
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

    void StayInZone()
    {
        if (zone == null) return;

        float dist = Vector2.Distance(transform.position, zone.transform.position);

        // If outside → go back
        if (dist > zone.depositRadius)
        {
            targetPosition = zone.transform.position;
            currentState = BeeState.Moving;
        }
        else
        {
            // Small idle movement inside zone
            if (Random.value < 0.01f)
            {
                targetPosition = zone.GetDepositPoint();
                currentState = BeeState.Moving;
            }
        }
    }

    protected override void ReturnBehavior() { }
}