using UnityEngine;

public class BuilderBee : Bee
{
    protected override void Update()
    {
        base.Update();

        if (BuildManager.Instance == null) return;

        ConstructionSite activeSite = BuildManager.Instance.activeSite;
        if (activeSite == null) return;

        targetPosition = activeSite.transform.position;

        if (currentState != BeeState.Moving)
        {
            currentState = BeeState.Moving;
        }
    }

    protected override void Die()
    {
        base.Die(); // VERY IMPORTANT
    }

    protected override void GoHome()
    {
        SleepZone sleep = FindNearestSleepZone();

        Vector2 destination = sleep != null
            ? (Vector2)sleep.transform.position
            : homePosition;

        float dist = Vector2.Distance(rb.position, destination);

        if (dist < 0.3f)
        {
            isAtHome = true;
            StopMovementInstant();
            currentState = BeeState.Idle;
        }
        else
        {
            targetPosition = destination;
            currentState = BeeState.Moving;
        }
    }

    protected override void WorkBehavior() { }
    protected override void ReturnBehavior() { }
}