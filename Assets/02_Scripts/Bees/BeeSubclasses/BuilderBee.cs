using UnityEngine;

public class BuilderBee : Bee
{
    private static ConstructionSite activeSite;

    public static void SetActiveSite(ConstructionSite site)
    {
        activeSite = site;
    }

    protected override void Update()
    {
        base.Update();

        if (activeSite == null) return;

        // Move toward construction site
        targetPosition = activeSite.transform.position;

        if (currentState != BeeState.Moving)
        {
            currentState = BeeState.Moving;
        }
    }

    protected override void Die()
    {
        // custom behavior (e.g. particles)

        base.Die(); // VERY IMPORTANT
    }

    protected override void WorkBehavior() { }
    protected override void ReturnBehavior() { }
}