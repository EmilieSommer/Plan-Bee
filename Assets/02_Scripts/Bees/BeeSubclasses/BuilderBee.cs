using UnityEngine;

public class BuilderBee : Bee
{
    public float roamRadius = 3f;
    private Vector3 startPosition;

    protected override void Awake()
    {
        base.Awake();
        startPosition = transform.position;
    }

    protected override void IdleBehavior()
    {
        Vector2 randomOffset = Random.insideUnitCircle * roamRadius;
        targetPosition = startPosition + new Vector3(randomOffset.x, randomOffset.y, 0);

        currentState = BeeState.Moving;
    }

    protected override void WorkBehavior()
    {
        // Later: build tiles
    }

    protected override void ReturnBehavior()
    {
        // Not used
    }
}