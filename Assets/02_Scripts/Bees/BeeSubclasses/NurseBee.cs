using UnityEngine;

public class NurseBee : Bee
{
    [Header("Work Settings")]
    public float workDistanceThreshold = 1.5f;

    [Header("Idle Settings")]
    public float idleWanderRadius = 2f;
    public float idleMoveCooldownMin = 2f;
    public float idleMoveCooldownMax = 5f;

    private float idleMoveTimer = 0f;
    private Egg assignedEgg;

    private bool isTending = false;
    private float orbitAngle = 0f;

    protected override void Start()
    {
        base.Start(); // this already assigns zone + home
    }

    protected override void Update()
    {
        base.Update();
        CheckForWorkContinuously();
    }

    // ------------------------
    // IDLE
    // ------------------------
    protected override void IdleBehavior()
    {
        if (assignedEgg != null && !assignedEgg.IsHatched())
        {
            MoveToEgg(assignedEgg);
            return;
        }

        if (assignedEgg != null)
        {
            assignedEgg.RemoveNurse();
            assignedEgg = null;
        }

        isTending = false;

        assignedEgg = FindClosestEgg();

        if (assignedEgg != null)
        {
            MoveToEgg(assignedEgg);
        }
        else
        {
            NormalIdleMovement();
        }
    }

    // ------------------------
    // WORK CHECK
    // ------------------------
    void CheckForWorkContinuously()
    {
        if (currentState == BeeState.Dead || isTending)
            return;

        if (assignedEgg == null || assignedEgg.IsHatched() || assignedEgg.HasNurse())
        {
            assignedEgg = FindClosestEgg();

            if (assignedEgg != null)
            {
                targetPosition = assignedEgg.transform.position;
                currentState = BeeState.Moving;
            }
        }
    }

    // ------------------------
    // WORK
    // ------------------------
    protected override void WorkBehavior()
    {
        if (assignedEgg == null || assignedEgg.IsHatched())
        {
            if (assignedEgg != null)
            {
                assignedEgg.RemoveNurse();
            }

            assignedEgg = null;
            isTending = false;
            currentState = BeeState.Idle;
            return;
        }

        float dist = Vector2.Distance(transform.position, assignedEgg.transform.position);

        if (dist > workDistanceThreshold)
        {
            MoveToEgg(assignedEgg);
            return;
        }

        if (!isTending)
        {
            if (assignedEgg.TryAssignNurse(this))
            {
                isTending = true;
            }
            else
            {
                assignedEgg = null;
                isTending = false;
                currentState = BeeState.Idle;
                return;
            }
        }

        OrbitEgg();
    }

    // ------------------------
    // RETURN (not used)
    // ------------------------
    protected override void ReturnBehavior()
    {
    }

    // ------------------------
    // TARGET REACHED
    // ------------------------
    protected override void OnReachedTarget()
    {
        if (assignedEgg != null && !assignedEgg.IsHatched())
        {
            currentState = BeeState.Working;
        }
        else
        {
            assignedEgg = null;
            isTending = false;
            currentState = BeeState.Idle;
        }
    }

    // ------------------------
    // MOVEMENT HELPERS
    // ------------------------
    private void MoveToEgg(Egg egg)
    {
        if (egg == null) return;

        Vector2 offset = Random.insideUnitCircle.normalized * 0.3f;
        targetPosition = (Vector2)egg.transform.position + offset;

        currentState = BeeState.Moving;
    }

    private Egg FindClosestEgg()
    {
        Egg closest = null;
        float closestDist = Mathf.Infinity;

        if (Egg.allEggs == null) return null;

        foreach (Egg egg in Egg.allEggs)
        {
            if (egg == null || egg.IsHatched())
                continue;

            if (egg.HasNurse())
                continue;

            float dist = Vector2.Distance(transform.position, egg.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = egg;
            }
        }

        return closest;
    }

    private void NormalIdleMovement()
    {
        idleMoveTimer -= Time.deltaTime;

        if (idleMoveTimer <= 0f)
        {
            idleMoveTimer = Random.Range(idleMoveCooldownMin, idleMoveCooldownMax);

            Vector2 randomPoint = Vector2.zero;

            if (HasValidZone())
            {
                // 🧠 NOW USE BASE ZONE
                randomPoint = assignedZone.transform.position +
                              (Vector3)Random.insideUnitCircle * idleWanderRadius;
            }
            else
            {
                randomPoint = (Vector2)transform.position + Random.insideUnitCircle * idleWanderRadius;
            }

            targetPosition = randomPoint;
            currentState = BeeState.Moving;
        }
    }

    private void OrbitEgg()
    {
        if (assignedEgg == null) return;

        orbitAngle += Time.deltaTime * 2f;

        float radius = 0.4f;

        Vector2 offset = new Vector2(
            Mathf.Cos(orbitAngle),
            Mathf.Sin(orbitAngle)
        ) * radius;

        targetPosition = (Vector2)assignedEgg.transform.position + offset;

        currentState = BeeState.Moving;
    }

    // ------------------------
    // DEATH
    // ------------------------
    protected override void Die()
    {
        // optional: particles, sound, etc.

        base.Die(); // IMPORTANT: keeps Zone + Hive cleanup
    }
}