using UnityEngine;

public class NurseBee : Bee
{
    [Header("Work Settings")]
    public float workDistanceThreshold = 1.5f;
    public float maxTendingTime = 30f;

    private Egg assignedEgg;
    private bool isTending = false;
    private float tendingTimer = 0f;

    private float orbitAngle = 0f;

    protected override void Start()
    {
        base.Start();
        // Scale down Nurse bees so they are physically smaller and fit better around eggs
        transform.localScale *= 0.65f;
    }

    protected override void Update()
    {
        base.Update();

        if (assignedEgg != null && !IsEggValid(assignedEgg))
            ClearAssignment();
    }

    // ======================================================
    // JOB VALIDATION
    // ======================================================

    protected override bool HasWork()
    {
        if (IsEggValid(assignedEgg)) return true;
        if (Egg.allEggs == null) return false;

        foreach (Egg egg in Egg.allEggs)
        {
            if (IsEggValid(egg) && !egg.HasNurse() && !egg.IsReserved())
                return true;
        }

        return false;
    }

    protected override void OnJobFound()
    {
        if (!IsEggValid(assignedEgg))
        {
            Egg egg = FindClosestEgg();
            if (egg != null)
            {
                assignedEgg = egg;
                MarkAsWorking();
                MoveToEgg(assignedEgg);
            }
        }
        else
        {
            MarkAsWorking();
            MoveToEgg(assignedEgg);
        }
    }

    // ======================================================
    // VALIDATION
    // ======================================================

    private bool IsEggValid(Egg egg)
    {
        return egg != null && !egg.IsHatched();
    }

    private void ClearAssignment()
    {
        if (assignedEgg != null)
        {
            assignedEgg.RemoveNurse(this);
            assignedEgg.ClearReservation(this);
        }

        assignedEgg = null;
        isTending = false;
        tendingTimer = 0f;
        currentState = BeeState.Idle;
    }

    // ======================================================
    // IDLE
    // ======================================================

    protected override void IdleBehavior()
    {
        if (IsEggValid(assignedEgg))
        {
            MoveToEgg(assignedEgg);
            return;
        }

        Egg egg = FindClosestEgg();
        if (egg != null)
        {
            assignedEgg = egg;
            MarkAsWorking();
            MoveToEgg(assignedEgg);
        }
    }

    // ======================================================
    // WORK
    // ======================================================

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
            tendingTimer = 0f;
            MoveToEgg(assignedEgg);
            return;
        }

        if (!isTending)
        {
            if (assignedEgg.TryAssignNurse(this))
            {
                isTending = true;
                tendingTimer = 0f;
            }
            else
            {
                assignedEgg.ClearReservation(this);
                ClearAssignment();
                return;
            }
        }

        tendingTimer += Time.deltaTime;

        // Safety escape — if egg hasn't hatched after max time, give up
        if (tendingTimer >= maxTendingTime)
        {
            ClearAssignment();
            return;
        }

        OrbitEgg();
    }

    // ======================================================
    // TARGET REACHED
    // ======================================================

    protected override void OnReachedTarget()
    {
        if (IsEggValid(assignedEgg))
        {
            currentState = BeeState.Working;
        }
        else
        {
            ClearAssignment();
            base.OnReachedTarget();
        }
    }

    // ======================================================
    // MOVEMENT
    // ======================================================

    private void MoveToEgg(Egg egg)
    {
        if (!IsEggValid(egg)) return;

        Vector2 offset = Random.insideUnitCircle.normalized * 0.6f;
        targetPosition = (Vector2)egg.transform.position + offset;
        currentState = BeeState.Moving;
    }

    private Egg FindClosestEgg()
    {
        if (Egg.allEggs == null) return null;

        Egg closest = null;
        float closestDist = Mathf.Infinity;

        foreach (Egg egg in Egg.allEggs)
        {
            if (!IsEggValid(egg)) continue;
            if (egg.HasNurse() || egg.IsReserved()) continue;

            float dist = Vector2.Distance(transform.position, egg.transform.position);

            if (dist < closestDist)
            {
                if (egg.TryReserve(this))
                {
                    if (closest != null)
                        closest.ClearReservation(this);

                    closestDist = dist;
                    closest = egg;
                }
            }
        }

        return closest;
    }

    // ======================================================
    // ORBIT
    // ======================================================

    private void OrbitEgg()
    {
        if (!IsEggValid(assignedEgg))
        {
            ClearAssignment();
            return;
        }

        orbitAngle += Time.deltaTime * 2f * workSpeedMultiplier;

        Vector2 offset = new Vector2(
            Mathf.Cos(orbitAngle),
            Mathf.Sin(orbitAngle)
        ) * 0.7f;

        // Move directly — bypasses the state machine movement system
        rb.MovePosition((Vector2)assignedEgg.transform.position + offset);

        // Face direction of orbit
        Vector2 tangent = new Vector2(-Mathf.Sin(orbitAngle), Mathf.Cos(orbitAngle));
        rb.rotation = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;

        // Stay in Working so WorkBehavior keeps being called
        currentState = BeeState.Working;
    }

    // ======================================================
    // RETURN / DEATH
    // ======================================================

    protected override void ReturnBehavior()
    {
        ClearAssignment();
        currentState = BeeState.Idle;
    }

    protected override void Die() => base.Die();
}