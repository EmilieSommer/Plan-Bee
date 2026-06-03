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

    [Header("Parasite Effects")]
    public float workSpeedMultiplier = 1f;

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

    [Header("Job Validation")]
    public float jobCheckInterval = 2f;
    private float jobCheckTimer;

    protected Vector2 moveDirection;
    protected Vector2 targetPosition;
    protected Vector2 currentVelocity;
    protected Vector2 homePosition;

    protected bool lockMovement = false;

    protected BeeState currentState;
    protected Rigidbody2D rb;

    private float stuckTimer;
    private Vector2 lastPosition;

    [Header("Retaliation")]
    public float retaliationRange = 1.5f;
    public float retaliationDamage = 1f;
    public float retaliationCooldown = 2f;
    private float retaliationTimer = 0f;

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

    // tracks whether bee has returned to home zone after finding no work
    protected bool isAtHome = false;

    protected virtual void Awake()
    {
        if (HiveManager.Instance != null)
            HiveManager.Instance.RegisterBee(this);

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
        gameObject.name = beeName;
    }

    protected virtual void Update()
    {
        if (currentState == BeeState.Dead) return;

        if (retaliationTimer > 0f)
            retaliationTimer -= Time.deltaTime;

        jobCheckTimer += Time.deltaTime;
        if (jobCheckTimer >= jobCheckInterval)
        {
            jobCheckTimer = 0f;
            ValidateJob();
        }

        CheckIfStuck();
        StateUpdate();
    }

    // ======================================================
    // JOB VALIDATION SYSTEM
    // ======================================================

    // Subclasses return true when there is work available right now
    protected virtual bool HasWork() => false;

    // Called every jobCheckInterval — override to add extra logic
    protected virtual void ValidateJob()
    {
        if (isBeingDragged || lockMovement) return;
        if (currentState == BeeState.Dead) return;

        if (HasWork())
        {
            // Bee is idle or wandering home but work exists → restart job
            if (currentState == BeeState.Idle || isAtHome)
            {
                isAtHome = false;
                OnJobFound();
            }
        }
        else
        {
            // No work → go home and stand still if not already doing so
            if (currentState == BeeState.Idle && !isAtHome)
            {
                GoHome();
            }
        }
    }

    // Called when validation finds work and bee is idle — override in subclass
    protected virtual void OnJobFound() { }

    // Sends bee back to its zone to stand still
    protected virtual void GoHome()
    {
        if (assignedZone == null) return;

        float distToHome = Vector2.Distance(rb.position, homePosition);

        if (distToHome < 0.3f)
        {
            // Already at home — freeze completely
            isAtHome = true;
            StopMovementInstant();
            currentState = BeeState.Idle;
        }
        else
        {
            // Move back to home zone
            targetPosition = homePosition;
            currentState = BeeState.Moving;
        }
    }

    protected SleepZone FindNearestSleepZone()
    {
        SleepZone[] zones = FindObjectsOfType<SleepZone>();

        SleepZone closest = null;
        float closestDist = Mathf.Infinity;

        foreach (var zone in zones)
        {
            float dist = Vector2.Distance(transform.position, zone.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = zone;
            }
        }

        return closest;
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

        if (currentState == BeeState.Working || currentState == BeeState.Dead)
        {
            currentVelocity = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            return;
        }

        // Hard freeze when idle and at home
        if (currentState == BeeState.Idle && isAtHome)
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
        if (lockMovement) return;

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
            AssignZone();
        return assignedZone != null;
    }

    // ======================================================
    // MOVEMENT
    // ======================================================

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
        moveDirection = Vector2.Lerp(moveDirection, desiredDirection, turnSpeed * Time.deltaTime).normalized;

        float speedFactor = Mathf.Clamp01(distance / slowRadius);
        float adjustedSpeed = moveSpeed * Mathf.Lerp(0.3f, 1f, speedFactor);

        currentVelocity = moveDirection * adjustedSpeed;
    }

    // ======================================================
    // AVOIDANCE
    // ======================================================

    protected Vector2 GetForwardAvoidance()
    {
        Vector2 forward = currentVelocity;
        if (forward.sqrMagnitude < 0.01f) forward = moveDirection;
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

            if (beeType == BeeType.Forager)
            {
                HouseBee house = other as HouseBee;
                if (house != null && house.IsWorking) strengthMultiplier = 4f;
                else if (other.beeType == BeeType.House) continue;
            }

            if (beeType == BeeType.House)
            {
                HouseBee self = this as HouseBee;
                if (self != null && self.IsWorking) continue;
                if (other.beeType == BeeType.Forager) strengthMultiplier = 2.5f;
            }

            Vector2 diff = (Vector2)transform.position - (Vector2)other.transform.position;
            float dist = diff.magnitude;
            if (dist > 0.01f) avoidance += (diff.normalized / dist) * strengthMultiplier;
        }

        return avoidance != Vector2.zero ? avoidance.normalized * avoidanceStrength : Vector2.zero;
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

            if (beeType == BeeType.Forager)
            {
                HouseBee house = other as HouseBee;
                if (house != null && house.IsWorking) strengthMultiplier = 3.5f;
                else if (other.beeType == BeeType.House) continue;
            }

            if (beeType == BeeType.House)
            {
                HouseBee self = this as HouseBee;
                if (self != null && self.IsWorking) continue;
                if (other.beeType == BeeType.Forager) strengthMultiplier = 2.5f;
            }

            Vector2 diff = (Vector2)(transform.position - other.transform.position);
            float dist = diff.magnitude;
            if (dist > 0.01f) force += (diff.normalized / dist) * strengthMultiplier;
        }

        return force;
    }

    // ======================================================
    // STUCK SYSTEM
    // ======================================================

    void CheckIfStuck()
    {
        // Don't run stuck detection when frozen at home
        if (isAtHome || currentState == BeeState.Idle || currentState == BeeState.Working)
            return;

        stuckTimer += Time.deltaTime;
        if (stuckTimer >= stuckCheckInterval)
        {
            float moved = Vector2.Distance(rb.position, lastPosition);
            if (moved < stuckMoveThreshold)
                ResolveStuck();

            lastPosition = rb.position;
            stuckTimer = 0f;
        }
    }

    void ResolveStuck()
    {
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        moveDirection = (moveDirection + randomDir).normalized;
        rb.MovePosition(rb.position + moveDirection * 0.2f);
        targetPosition += randomDir * 0.5f;
    }

    // ======================================================
    // ROTATION
    // ======================================================

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

    // ======================================================
    // STATES
    // ======================================================

    protected virtual void OnReachedTarget()
    {
        // If we just arrived home, freeze
        float distToHome = Vector2.Distance(rb.position, homePosition);
        if (distToHome < 0.3f && !HasWork())
        {
            isAtHome = true;
            StopMovementInstant();
        }

        currentState = BeeState.Idle;
    }

    // Idle no longer roams — job system drives all movement
    protected virtual void IdleBehavior()
    {
        if (lockMovement) return;
        // Intentionally empty: bees stand still when idle.
        // ValidateJob() handles whether to work or go home.
    }

    protected void StopMovementInstant()
    {
        currentVelocity = Vector2.zero;
        moveDirection = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }

    // Call this from subclasses whenever they start a new job cycle
    // so isAtHome gets cleared correctly
    protected void MarkAsWorking()
    {
        isAtHome = false;
    }

    protected abstract void WorkBehavior();
    protected abstract void ReturnBehavior();

    // ======================================================
    // HEALTH
    // ======================================================

    public void TakeDamage(float amount, Enemy attacker = null)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f) { currentHealth = 0f; Die(); return; }
        if (attacker != null) TryRetaliate(attacker);
    }

    void TryRetaliate(Enemy attacker)
    {
        if (retaliationTimer > 0f) return;

        attacker.TakeDamage(retaliationDamage);
        retaliationTimer = retaliationCooldown;

        // Snap fully toward attacker
        Vector2 dirToEnemy = ((Vector2)attacker.transform.position - rb.position).normalized;
        
        // Hard face the enemy
        rb.rotation = Mathf.Atan2(dirToEnemy.y, dirToEnemy.x) * Mathf.Rad2Deg;
        
        // Stronger lunge toward them
        moveDirection = dirToEnemy;
        currentVelocity = dirToEnemy * moveSpeed;
        rb.MovePosition(rb.position + dirToEnemy * 0.6f);
    }

    public void Heal(float amount)
    {
        if (currentState == BeeState.Dead) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
    }

    public void SetHealth(int value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
    }

    protected virtual void Die()
    {
        currentState = BeeState.Dead;

        if (HiveManager.Instance != null) HiveManager.Instance.UnregisterBee(this);
        if (ZoneManager.Instance != null) ZoneManager.Instance.UnregisterBee(this);
        if (assignedZone != null) assignedZone.UnregisterBee(this);

        rb.linearVelocity = Vector2.zero;
        Destroy(gameObject);
    }

    protected virtual void AssignZone()
    {
        if (assignedZone != null) return;
        if (ZoneManager.Instance == null) return;

        assignedZone = ZoneManager.Instance.GetClosestZone(beeType, transform.position);

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
        if (assignedZone != null) assignedZone.UnregisterBee(this);

        assignedZone = newZone;

        if (assignedZone != null)
        {
            assignedZone.RegisterBee(this);
            homePosition = assignedZone.transform.position;
        }

        isAtHome = false;
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
        isAtHome = false; // re-evaluate job on drop
        PickRandomDirection();
        currentState = BeeState.Idle;
    }

    public virtual void UpgradeSpeed(float amount, float max)
    {
        moveSpeed = Mathf.Min(moveSpeed + amount, max);
    }
}