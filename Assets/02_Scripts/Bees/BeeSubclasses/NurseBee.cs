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
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        // 🧠 HARD SAFETY RESET (fixes ALL stuck cases)
        if (!IsEggValid(assignedEgg))
        {
            ClearAssignment();
        }

        CheckForWorkContinuously();
    }

    // ------------------------
    // VALIDATION
    // ------------------------
    private bool IsEggValid(Egg egg)
    {
        return egg != null && !egg.IsHatched();
    }

    private void ClearAssignment()
    {
        if (assignedEgg != null)
        {
            assignedEgg.RemoveNurse();
        }

        assignedEgg = null;
        isTending = false;
        currentState = BeeState.Idle;
    }

    // ------------------------
    // IDLE / SEARCH
    // ------------------------
    protected override void IdleBehavior()
    {
        if (IsEggValid(assignedEgg))
        {
            MoveToEgg(assignedEgg);
            return;
        }

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

    void CheckForWorkContinuously()
    {
        if (currentState == BeeState.Dead || isTending)
            return;

        if (!IsEggValid(assignedEgg))
        {
            assignedEgg = FindClosestEgg();

            if (assignedEgg != null)
            {
                currentState = BeeState.Moving;
                targetPosition = assignedEgg.transform.position;
            }
        }
    }

    // ------------------------
    // WORK
    // ------------------------
    protected override void WorkBehavior()
    {
        if (!IsEggValid(assignedEgg))
        {
            ClearAssignment();
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
                ClearAssignment();
                return;
            }
        }

        OrbitEgg();
    }

    // ------------------------
    // TARGET REACHED
    // ------------------------
    protected override void OnReachedTarget()
    {
        if (IsEggValid(assignedEgg))
        {
            currentState = BeeState.Working;
        }
        else
        {
            ClearAssignment();
        }
    }

    // ------------------------
    // MOVEMENT
    // ------------------------
    private void MoveToEgg(Egg egg)
    {
        if (!IsEggValid(egg)) return;

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
            if (!IsEggValid(egg)) continue;
            if (egg.HasNurse()) continue;

            float dist = Vector2.Distance(transform.position, egg.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = egg;
            }
        }

        return closest;
    }

    // ------------------------
    // IDLE MOVEMENT
    // ------------------------
    private void NormalIdleMovement()
    {
        idleMoveTimer -= Time.deltaTime;

        if (idleMoveTimer <= 0f)
        {
            idleMoveTimer = Random.Range(idleMoveCooldownMin, idleMoveCooldownMax);

            Vector2 randomPoint;

            if (HasValidZone())
            {
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

    // ------------------------
    // ORBIT
    // ------------------------
    private void OrbitEgg()
    {
        if (!IsEggValid(assignedEgg))
        {
            ClearAssignment();
            return;
        }

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
        base.Die();
    }

    protected override void ReturnBehavior()
    {
        ClearAssignment();
        currentState = BeeState.Idle;
    }
}