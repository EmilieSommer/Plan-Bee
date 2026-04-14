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

    protected override void WorkBehavior() { }
    protected override void ReturnBehavior() { }
}