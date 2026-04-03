using UnityEngine;

public class ForagerBee : Bee
{
    [Header("Forager Settings")]
    public float tripDuration = 5f;
    public float forageRadius = 5f;

    private float tripTimer;
    private Vector3 hivePosition;

    protected override void Awake()
    {
        base.Awake();
        hivePosition = transform.position;
    }

    protected override void IdleBehavior()
    {
        // Pick random outside location
        Vector2 randomOffset = Random.insideUnitCircle * forageRadius;
        targetPosition = hivePosition + new Vector3(randomOffset.x, randomOffset.y, 0);

        currentState = BeeState.Moving;
    }

    protected override void WorkBehavior()
    {
        tripTimer -= Time.deltaTime;

        if (tripTimer <= 0f)
        {
            targetPosition = hivePosition;
            currentState = BeeState.Returning;
        }
    }

    protected override void ReturnBehavior()
    {
        MoveToTarget();
    }

    protected override void OnReachedTarget()
    {
        if (currentState == BeeState.Moving)
        {
            tripTimer = tripDuration;
            currentState = BeeState.Working;
        }
        else if (currentState == BeeState.Returning)
        {
            currentState = BeeState.Idle;
        }
    }
}