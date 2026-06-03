using UnityEngine;
using System.Collections.Generic;

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

    // ======================================================
    // SITE SELECTION
    // ======================================================

    ConstructionSite GetAvailableSite()
    {
        if (BuildManager.Instance == null) return null;

        ConstructionSite active = BuildManager.Instance.activeSite;
        if (active == null) return null;

        // Build list of queued sites
        List<ConstructionSite> queuedSites = new List<ConstructionSite>();
        foreach (ConstructionSite site in BuildManager.Instance.GetQueue())
        {
            if (site != null) queuedSites.Add(site);
        }

        // No other sites — always go to active
        if (queuedSites.Count == 0) return active;

        int activeCount = active.GetBuilderCount();

        // Only send to secondary if active has strictly more builders than the secondary
        // meaning active is well covered and secondary needs help
        foreach (ConstructionSite site in queuedSites)
        {
            int siteCount = site.GetBuilderCount();

            // Secondary site only gets a bee if active has more bees than it
            if (activeCount > siteCount + 1)
                return site;
        }

        // Default — go to active
        return active;
    }

    // ======================================================
    // JOB VALIDATION
    // ======================================================

    protected override bool HasWork()
    {
        return BuildManager.Instance != null && GetAvailableSite() != null;
    }

    protected override void OnJobFound()
    {
        ConstructionSite site = GetAvailableSite();
        if (site == null) return;
        if (site == currentSite && currentState == BeeState.Moving) return;

        currentSite = site;
        MarkAsWorking();
        targetPosition = currentSite.transform.position;
        currentState = BeeState.Moving;
    }

    // ======================================================
    // IDLE
    // ======================================================

    protected override void IdleBehavior()
    {
        if (!HasWork()) return;
        OnJobFound();
    }

    // ======================================================
    // TARGET REACHED
    // ======================================================

    protected override void OnReachedTarget()
    {
        if (HasWork())
        {
            // Check if a less populated site is available
            ConstructionSite best = GetAvailableSite();
            if (best != null && best != currentSite)
            {
                currentSite = best;
                targetPosition = currentSite.transform.position;
                currentState = BeeState.Moving;
                return;
            }

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

    // ======================================================
    // WORK
    // ======================================================

    protected override void WorkBehavior()
    {
        if (!HasWork())
        {
            currentSite = null;
            currentState = BeeState.Idle;
            return;
        }

        // Periodically recheck if a less populated site is available
        ConstructionSite best = GetAvailableSite();
        if (best != null && best != currentSite)
        {
            currentSite = best;
            targetPosition = currentSite.transform.position;
            currentState = BeeState.Moving;
            return;
        }

        if (currentSite == null)
        {
            currentState = BeeState.Idle;
            return;
        }

        float dist = Vector2.Distance(transform.position, currentSite.transform.position);
        if (dist > arrivalDistance)
        {
            targetPosition = currentSite.transform.position;
            currentState = BeeState.Moving;
        }
    }

    // ======================================================
    // SLEEP
    // ======================================================

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