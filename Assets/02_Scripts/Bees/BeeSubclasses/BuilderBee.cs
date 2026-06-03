using UnityEngine;

public class BuilderBee : Bee
{
    [Header("Work Settings")]
    public float arrivalDistance = 0.5f;

    private ConstructionSite currentSite;

    protected override void Awake()
    {
        base.Awake();
        beeType = BeeType.Builder;
    }

    protected override bool HasWork()
    {
        return BuildManager.Instance != null && BuildManager.Instance.activeSite != null;
    }

    protected override void OnJobFound()
    {
        ConstructionSite site = BuildManager.Instance.activeSite;
        if (site == currentSite && currentState == BeeState.Moving) return;

        currentSite = site;
        MarkAsWorking();
        targetPosition = currentSite.transform.position;
        currentState = BeeState.Moving;
    }

    protected override void IdleBehavior()
    {
        if (!HasWork()) return;
        OnJobFound();
    }

    protected override void OnReachedTarget()
    {
        if (HasWork())
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

        currentSite = null;
        base.OnReachedTarget();
    }

    protected override void WorkBehavior()
    {
        if (!HasWork())
        {
            currentSite = null;
            currentState = BeeState.Idle;
            return;
        }

        ConstructionSite site = BuildManager.Instance.activeSite;

        if (site != currentSite)
        {
            currentSite = site;
            targetPosition = currentSite.transform.position;
            currentState = BeeState.Moving;
            return;
        }

        float dist = Vector2.Distance(transform.position, currentSite.transform.position);
        if (dist > arrivalDistance)
        {
            targetPosition = currentSite.transform.position;
            currentState = BeeState.Moving;
        }
    }

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

    protected override void ReturnBehavior() { }
    protected override void Die() => base.Die();
}