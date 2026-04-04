using UnityEngine;

public class HouseBee : Bee
{
    [Header("Conversion")]
    public float workDistanceThreshold = 1.5f;
    public int honeyPerConversion = 1;

    [Header("Idle")]
    public float idleWanderRadius = 2f;

    private HouseBeeZone assignedZone;
    private Pollen targetPollen;

    private float idleTimer = 0f;

    protected override void Awake()
    {
        base.Awake();

        assignedZone = FindObjectOfType<HouseBeeZone>();

        if (assignedZone != null)
        {
            homePosition = assignedZone.transform.position;
        }
    }

    protected override void IdleBehavior()
    {
        // Try to find pollen
        targetPollen = FindClosestPollen();

        if (targetPollen != null)
        {
            MoveToPollen(targetPollen);
            return;
        }

        // Otherwise wander
        NormalIdleMovement();
    }

    protected override void WorkBehavior()
    {
        // If pollen is gone
        if (targetPollen == null)
        {
            currentState = BeeState.Idle;
            return;
        }

        float dist = Vector2.Distance(transform.position, targetPollen.transform.position);

        if (dist > workDistanceThreshold)
        {
            MoveToPollen(targetPollen);
            return;
        }

        // CONVERT POLLEN
        ConvertPollen();
    }

    void ConvertPollen()
    {
        if (targetPollen == null) return;

        // Convert
        CurrencyManager.Instance.honey += honeyPerConversion;

        // Remove pollen object
        Destroy(targetPollen.gameObject);

        targetPollen = null;

        // Go back to idle
        currentState = BeeState.Idle;
    }

    protected override void ReturnBehavior()
    {
        // Not used
    }

    // ---------------------------
    // Movement Helpers
    // ---------------------------

    private void MoveToPollen(Pollen pollen)
    {
        if (pollen == null) return;

        targetPosition = pollen.transform.position;
        currentState = BeeState.Moving;
    }

    private void NormalIdleMovement()
    {
        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0f)
        {
            idleTimer = Random.Range(2f, 5f);

            Vector2 randomPoint;

            if (assignedZone != null)
            {
                randomPoint = assignedZone.GetDepositPoint();
            }
            else
            {
                randomPoint = (Vector2)transform.position + Random.insideUnitCircle * idleWanderRadius;
            }

            targetPosition = randomPoint;
            currentState = BeeState.Moving;
        }
    }

    private Pollen FindClosestPollen()
    {
        Pollen closest = null;
        float closestDist = Mathf.Infinity;

        foreach (Pollen pollen in Pollen.allPollen)
        {
            if (pollen == null) continue;

            float dist = Vector2.Distance(transform.position, pollen.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = pollen;
            }
        }

        return closest;
    }
}