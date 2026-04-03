using UnityEngine;

public class DroneBee : Bee
{
    public float patrolRadius = 4f;
    private Vector3 startPosition;

    protected override void Awake()
    {
        base.Awake();
        startPosition = transform.position;
    }

    protected override void IdleBehavior()
    {
        Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
        targetPosition = startPosition + new Vector3(randomOffset.x, randomOffset.y, 0);

        currentState = BeeState.Moving;
    }

    protected override void WorkBehavior()
    {
        // Later: attack enemies
    }

    protected override void ReturnBehavior()
    {
        // Not used
    }
}