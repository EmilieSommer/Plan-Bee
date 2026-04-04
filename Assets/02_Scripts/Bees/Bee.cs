using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public abstract class Bee : MonoBehaviour
{
    public enum BeeState
    {
        Idle,
        Moving,
        Working,
        Returning,
        Dead
    }

    [Header("Base Stats")]
    public float maxHealth = 10f;
    protected float currentHealth;

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Steering")]
    public float acceleration = 8f;
    public float turnSpeed = 5f;
    public float slowRadius = 0.5f;

    [Header("Avoidance")]
    public float avoidanceLookAhead = 1f;
    public float avoidanceRadius = 0.5f;
    public float avoidanceStrength = 5f;

    [Header("Separation")]
    public float separationRadius = 0.6f;
    public float separationStrength = 2f;

    [Header("Roaming")]
    public float roamRadius = 3f;

    protected Vector2 moveDirection;
    protected Vector2 targetPosition;
    protected Vector2 currentVelocity;
    protected Vector2 homePosition;

    protected BeeState currentState;
    protected Rigidbody2D rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // 🔥 CRITICAL: prevents physics from freezing bees
        rb.bodyType = RigidbodyType2D.Kinematic;

        currentHealth = maxHealth;
        currentState = BeeState.Idle;

        homePosition = transform.position;

        PickRandomDirection();
    }

    protected virtual void Update()
    {
        if (currentState == BeeState.Dead)
            return;

        StateUpdate();
    }

    protected virtual void FixedUpdate()
    {
        if (currentState == BeeState.Dead)
            return;

        Vector2 desiredVelocity = Vector2.zero;

        if (currentState == BeeState.Moving || currentState == BeeState.Returning)
        {
            Vector2 steering = moveDirection * moveSpeed;

            // 🔥 Avoid bees BEFORE collision
            steering += GetForwardAvoidance();

            // 🔥 Keep spacing
            steering += GetSeparationForce() * separationStrength;

            desiredVelocity = steering;
        }

        // Smooth acceleration
        currentVelocity = Vector2.Lerp(
            currentVelocity,
            desiredVelocity,
            acceleration * Time.fixedDeltaTime
        );

        currentVelocity = Vector2.ClampMagnitude(currentVelocity, moveSpeed);

        // 🔥 MOVE USING KINEMATIC MOTION (NO FREEZE)
        rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);

        RotateTowardsMovement();
    }

    protected virtual void StateUpdate()
    {
        switch (currentState)
        {
            case BeeState.Idle:
                IdleBehavior();
                break;

            case BeeState.Moving:
                MoveToTarget();
                break;

            case BeeState.Working:
                WorkBehavior();
                break;

            case BeeState.Returning:
                MoveToTarget();
                ReturnBehavior();
                break;
        }
    }

    protected void MoveToTarget()
    {
        Vector2 toTarget = targetPosition - rb.position;
        float distance = toTarget.magnitude;

        if (distance < 0.05f)
        {
            currentVelocity = Vector2.zero;
            OnReachedTarget();
            return;
        }

        Vector2 desiredDirection = toTarget.normalized;

        moveDirection = Vector2.Lerp(
            moveDirection,
            desiredDirection,
            turnSpeed * Time.deltaTime
        ).normalized;

        float speedFactor = Mathf.Clamp01(distance / slowRadius);
        float adjustedSpeed = moveSpeed * Mathf.Lerp(0.3f, 1f, speedFactor);

        currentVelocity = moveDirection * adjustedSpeed;
    }

    // 🔥 Predictive avoidance (prevents collision freezing)
    protected Vector2 GetForwardAvoidance()
    {
        Vector2 forward = currentVelocity;

        if (forward.sqrMagnitude < 0.01f)
            forward = moveDirection;

        forward.Normalize();

        Vector2 futurePosition = (Vector2)transform.position + forward * avoidanceLookAhead;

        Collider2D[] hits = Physics2D.OverlapCircleAll(futurePosition, avoidanceRadius);

        Vector2 avoidance = Vector2.zero;

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            if (!hit.CompareTag("Bee")) continue;

            Vector2 diff = (Vector2)transform.position - (Vector2)hit.transform.position;
            float dist = diff.magnitude;

            if (dist > 0.01f)
                avoidance += diff.normalized / dist;
        }

        return avoidance.normalized * avoidanceStrength;
    }

    // 🔥 Keeps bees from clustering too tightly
    protected Vector2 GetSeparationForce()
    {
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, separationRadius);
        Vector2 force = Vector2.zero;

        foreach (var n in neighbors)
        {
            if (n.gameObject == gameObject) continue;
            if (!n.CompareTag("Bee")) continue;

            Vector2 diff = (Vector2)(transform.position - n.transform.position);
            float dist = diff.magnitude;

            if (dist > 0.01f)
                force += diff.normalized / dist;
        }

        return force;
    }

    protected void RotateTowardsMovement()
    {
        if (currentVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
            rb.rotation = angle;
        }
    }

    protected void PickRandomDirection()
    {
        moveDirection = Random.insideUnitCircle.normalized;
    }

    protected virtual void OnReachedTarget()
    {
        currentState = BeeState.Idle;
    }

    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        currentState = BeeState.Dead;
        rb.linearVelocity = Vector2.zero;
        Destroy(gameObject);
    }

    // 🐝 ROAMING BEHAVIOR
    protected virtual void IdleBehavior()
    {
        Vector2 randomOffset = Random.insideUnitCircle * roamRadius;
        targetPosition = homePosition + randomOffset;

        currentState = BeeState.Moving;
    }

    protected abstract void WorkBehavior();
    protected abstract void ReturnBehavior();
}