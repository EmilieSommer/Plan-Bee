using UnityEngine;

public class HouseBee : Bee
{
    protected override void IdleBehavior()
    {
        currentState = BeeState.Working;
    }

    protected override void WorkBehavior()
    {
        // Later: convert pollen to honey
    }

    protected override void ReturnBehavior()
    {
        // Not used
    }
}