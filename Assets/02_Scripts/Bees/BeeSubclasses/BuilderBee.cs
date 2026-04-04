using UnityEngine;

public class BuilderBee : Bee
{
    public float buildPower = 1f;

    private BuildZone targetZone;

    public void AssignBuildZone(BuildZone zone)
    {
        targetZone = zone;

        if (targetZone != null)
        {
            targetPosition = targetZone.transform.position;
            currentState = BeeState.Moving;
        }
    }

    protected override void WorkBehavior()
    {
        // If no target, do nothing
        if (targetZone == null)
            return;

        if (targetZone.IsBuilt())
        {
            currentState = BeeState.Idle;
            return;
        }

        // Always move toward the zone until inside
        targetPosition = targetZone.transform.position;
        currentState = BeeState.Moving;
    }

    protected override void ReturnBehavior()
    {
        // Not used
    }

    protected override void OnReachedTarget()
    {
        currentState = BeeState.Working;
    }
}