using UnityEngine;

public class NurseBee : Bee
{
    protected override void IdleBehavior()
    {
        currentState = BeeState.Working;
    }

    protected override void WorkBehavior()
    {
        // Placeholder logic for now
        // Later this will reduce egg timers
    }

    protected override void ReturnBehavior()
    {
        // Nurses don't return anywhere
    }
}