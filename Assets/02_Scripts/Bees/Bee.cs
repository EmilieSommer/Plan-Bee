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

    [Header("Stuck Detection")]
    public float stuckCheckInterval = 0.5f;
    public float stuckMoveThreshold = 0.05f;

    protected Vector2 moveDirection;
    protected Vector2 targetPosition;
    protected Vector2 currentVelocity;
    protected Vector2 homePosition;

    protected BeeState currentState;
    protected Rigidbody2D rb;

    // 🔥 Stuck detection
    private float stuckTimer;
    private Vector2 lastPosition;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        rb.bodyType = RigidbodyType2D.Kinematic;

        currentHealth = maxHealth;
        currentState = BeeState.Idle;

        homePosition = transform.position;

        PickRandomDirection();

        lastPosition = rb.position;
    }

    protected virtual void Update()
    {
        if (currentState == BeeState.Dead)
            return;

        CheckIfStuck();

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

            // Avoid bees
            steering += GetForwardAvoidance();

            // Separation
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

    // ------------------------
    // MOVEMENT
    // ------------------------

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

    // ------------------------
    // AVOIDANCE
    // ------------------------

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

        if (avoidance != Vector2.zero)
            return avoidance.normalized * avoidanceStrength;

        return Vector2.zero;
    }

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

    // ------------------------
    // STUCK SYSTEM
    // ------------------------

    void CheckIfStuck()
    {
        stuckTimer += Time.deltaTime;

        if (stuckTimer >= stuckCheckInterval)
        {
            float moved = Vector2.Distance(rb.position, lastPosition);

            if (moved < stuckMoveThreshold)
            {
                ResolveStuck();
            }

            lastPosition = rb.position;
            stuckTimer = 0f;
        }
    }

    void ResolveStuck()
    {
        Vector2 randomDir = Random.insideUnitCircle.normalized;

        Vector2 newDir = (moveDirection + randomDir).normalized;

        moveDirection = newDir;

        // Nudge out of overlap
        rb.MovePosition(rb.position + newDir * 0.2f);

        // Adjust target slightly
        targetPosition += randomDir * 0.5f;
    }

    // ------------------------
    // ROTATION
    // ------------------------

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

    // ------------------------
    // STATES
    // ------------------------

    protected virtual void OnReachedTarget()
    {
        currentState = BeeState.Idle;
    }

    protected virtual void IdleBehavior()
    {
        Vector2 randomOffset = Random.insideUnitCircle * roamRadius;
        targetPosition = homePosition + randomOffset;

        currentState = BeeState.Moving;
    }

    protected abstract void WorkBehavior();
    protected abstract void ReturnBehavior();

    // ------------------------
    // HEALTH
    // ------------------------

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
}