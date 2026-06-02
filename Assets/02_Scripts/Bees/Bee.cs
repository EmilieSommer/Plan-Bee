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
    public float CurrentHealth => currentHealth;

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

    protected bool lockMovement = false;

    protected BeeState currentState;
    protected Rigidbody2D rb;

    // 🔥 Stuck detection
    private float stuckTimer;
    private Vector2 lastPosition;

    private bool isLockedOnTarget = false;

    public enum BeeType
    {
        Forager,
        House,
        Nurse,
        Drone,
        Builder,
        Queen,
    }
    
    [Header("Combat")]
    public float attackDamage = 2f;
    public float attackRange = 1.2f;
    public float attackCooldown = 1f;

    protected float attackTimer;

    [Header("Bee Type")]
    public BeeType beeType;

    protected Zone assignedZone;

    protected bool isBeingDragged = false;

    [Header("Identity")]
    public string beeName = "Unnamed Bee";

    protected virtual void Awake()
    {

        if (HiveManager.Instance != null)
        {
            HiveManager.Instance.RegisterBee(this);
        }

        attackTimer = 0f;
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

    protected virtual void Start()
    {
        AssignZone();
    }

    public void SetName(string newName)
    {
        beeName = string.IsNullOrWhiteSpace(newName) ? "Unnamed Bee" : newName;
        gameObject.name = beeName; // optional but useful in hierarchy
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
        if (isBeingDragged || lockMovement)
        {
            currentVelocity = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            return;
        }

        // 🔥 HARD STOP MOVEMENT (zones, idle freeze, etc.)
        if (currentState == BeeState.Working || currentState == BeeState.Dead || lockMovement)
        {
            currentVelocity = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            return;
        }

        Vector2 desiredVelocity = Vector2.zero;

        if (currentState == BeeState.Moving || currentState == BeeState.Returning)
        {
            Vector2 steering = moveDirection * moveSpeed;

            steering += GetForwardAvoidance();
            steering += GetSeparationForce() * separationStrength;

            desiredVelocity = steering;
        }

        // 🔥 IMPORTANT: smooth ONLY when not locked
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
        // 🔥 HARD FREEZE OVERRIDE
        if (lockMovement)
            return;

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
    protected bool HasValidZone()
    {
        if (assignedZone == null)
        {
            AssignZone();
        }

        return assignedZone != null;
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

            Bee other = hit.GetComponent<Bee>();
            if (other == null) continue;

            float strengthMultiplier = 1f;

            // 🔥 DYNAMIC PRIORITY RULES

            // ------------------------
            // FORAGER BEHAVIOR
            // ------------------------
            if (beeType == BeeType.Forager)
            {
                // Check if it's a working house bee
                HouseBee house = other as HouseBee;

                if (house != null && house.IsWorking)
                {
                    // ✅ STRONGLY avoid working bees
                    strengthMultiplier = 4f;
                }
                else
                {
                    // ✅ ignore non-working house bees
                    if (other.beeType == BeeType.House)
                        continue;
                }
            }

            // ------------------------
            // HOUSE BEE BEHAVIOR
            // ------------------------
            if (beeType == BeeType.House)
            {
                // If THIS bee is working → ignore everything
                HouseBee self = this as HouseBee;
                if (self != null && self.IsWorking)
                    continue;

                // Otherwise yield to foragers
                if (other.beeType == BeeType.Forager)
                    strengthMultiplier = 2.5f;
            }

            Vector2 diff = (Vector2)transform.position - (Vector2)other.transform.position;
            float dist = diff.magnitude;

            if (dist > 0.01f)
                avoidance += (diff.normalized / dist) * strengthMultiplier;
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

            Bee other = n.GetComponent<Bee>();
            if (other == null) continue;

            float strengthMultiplier = 1f;

            // ------------------------
            // FORAGER BEHAVIOR
            // ------------------------
            if (beeType == BeeType.Forager)
            {
                HouseBee house = other as HouseBee;

                if (house != null && house.IsWorking)
                {
                    // ✅ Strongly avoid working house bees
                    strengthMultiplier = 3.5f;
                }
                else
                {
                    // ✅ Ignore non-working house bees
                    if (other.beeType == BeeType.House)
                        continue;
                }
            }

            // ------------------------
            // HOUSE BEE BEHAVIOR
            // ------------------------
            if (beeType == BeeType.House)
            {
                HouseBee self = this as HouseBee;

                // ✅ If THIS bee is working → ignore everything
                if (self != null && self.IsWorking)
                    continue;

                // ✅ Yield to foragers
                if (other.beeType == BeeType.Forager)
                    strengthMultiplier = 2.5f;
            }

            Vector2 diff = (Vector2)(transform.position - other.transform.position);
            float dist = diff.magnitude;

            if (dist > 0.01f)
            {
                force += (diff.normalized / dist) * strengthMultiplier;
            }
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
    if (lockMovement)
        return;

    Vector2 randomOffset = Random.insideUnitCircle * roamRadius;
    targetPosition = homePosition + randomOffset;

    currentState = BeeState.Moving;
}

    protected void StopMovementInstant()
    {
        currentVelocity = Vector2.zero;
        moveDirection = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }

    protected abstract void WorkBehavior();
    protected abstract void ReturnBehavior();

    // ------------------------
    // HEALTH
    // ------------------------

    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (currentState == BeeState.Dead)
            return;

        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
    }

    public void SetHealth(int value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
    }

    protected virtual void Die()
    {
        currentState = BeeState.Dead;

        // 🧠 Unregister from Hive
        if (HiveManager.Instance != null)
        {
            HiveManager.Instance.UnregisterBee(this);
        }

        // 🧠 Unregister from Zone system
        if (ZoneManager.Instance != null)
        {
            ZoneManager.Instance.UnregisterBee(this);
        }

        // 🔥 ALSO remove from its zone
        if (assignedZone != null)
        {
            assignedZone.UnregisterBee(this);
        }

        rb.linearVelocity = Vector2.zero;

        Destroy(gameObject);
    }

    protected virtual void AssignZone()
    {
        if (assignedZone != null)
            return;

        if (ZoneManager.Instance == null)
            return;

        assignedZone = ZoneManager.Instance.GetClosestZone(
            beeType,
            transform.position
        );

        if (assignedZone != null)
        {
            homePosition = assignedZone.transform.position;
            assignedZone.RegisterBee(this);
            ZoneManager.Instance.RegisterBee(this);
        }
    }

    public void AssignZoneDirect(Zone newZone)
    {
        if (assignedZone == newZone) return;

        if (assignedZone != null)
            assignedZone.UnregisterBee(this);

        assignedZone = newZone;

        if (assignedZone != null)
        {
            assignedZone.RegisterBee(this);
            homePosition = assignedZone.transform.position;
        }

        currentState = BeeState.Idle;
    }

    public void StartDragging()
    {
        isBeingDragged = true;
        currentVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }

    public virtual void StopDragging()
    {
        isBeingDragged = false;
        PickRandomDirection();
        currentState = BeeState.Idle;
    }
}