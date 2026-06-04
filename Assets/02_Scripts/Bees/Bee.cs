using UnityEngine;
using System.Collections.Generic;

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

    [Header("Sleep")]
    public float sleepDelayRange = 1.5f;
    private float sleepDelayTimer = 0f;
    private bool waitingForSleep = false;

    protected Vector2 moveDirection;
    
    private Vector2 _targetPosition;
    protected List<Vector2> currentPath;
    protected int currentPathIndex;

    protected Vector2 targetPosition
    {
        get => _targetPosition;
        set
        {
            _targetPosition = value;
            currentPath = AStar.FindPathWorld(rb.position, value);
            currentPathIndex = 0;
        }
    }

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

    private Enemy lastAttacker;


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

    protected bool isAtHome = false;

    private SleepZone currentSleepZone;
    private SleepZone pendingSleepZone;

    protected virtual void Awake()
    {
        if (HiveManager.Instance != null)
            HiveManager.Instance.RegisterBee(this);

        attackTimer = 0f;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Resize bee to perfectly fit inside a 32x32 tile at 25x25 size
        transform.localScale = new Vector3(25f/32f, 25f/32f, 1f);

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

        // Staggered sleep delay
        if (waitingForSleep)
        {
            sleepDelayTimer -= Time.deltaTime;
            if (sleepDelayTimer <= 0f)
            {
                waitingForSleep = false;
                GoHome();
            }
            return;
        }

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

    protected virtual bool HasWork() => false;

    protected virtual void ValidateJob()
    {
        if (isBeingDragged || lockMovement) return;
        if (currentState == BeeState.Dead) return;

        if (HasWork())
        {
            waitingForSleep = false;
            sleepDelayTimer = 0f;

            if (currentState == BeeState.Idle || isAtHome)
            {
                isAtHome = false;
                OnJobFound();
            }
        }
        else
        {
            if (currentState == BeeState.Idle && !isAtHome && !waitingForSleep)
            {
                sleepDelayTimer = Random.Range(0f, sleepDelayRange);
                waitingForSleep = true;
            }
        }
    }

    protected virtual void OnJobFound() { }

    protected virtual void GoHome()
    {
        if (assignedZone == null) return;

        float distToHome = Vector2.Distance(rb.position, homePosition);

        if (distToHome < 0.3f)
        {
            isAtHome = true;
            StopMovementInstant();
            currentState = BeeState.Idle;
        }
        else
        {
            targetPosition = homePosition;
            currentState = BeeState.Moving;
        }
    }

    // ======================================================
    // SLEEP ZONE
    // ======================================================

    protected SleepZone FindNearestSleepZone()
    {
        SleepZone[] zones = FindObjectsOfType<SleepZone>();

        SleepZone closest = null;
        float closestDist = Mathf.Infinity;

        foreach (var zone in zones)
        {
            if (!zone.HasSpace) continue;

            float dist = Vector2.Distance(transform.position, zone.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = zone;
            }
        }

        return closest;
    }

    protected void ReserveSleep(SleepZone zone)
    {
        if (pendingSleepZone != null && pendingSleepZone != zone)
        {
            pendingSleepZone.Unregister(this);
            pendingSleepZone = null;
        }

        if (zone != null && pendingSleepZone != zone)
        {
            if (zone.TryRegister(this))
                pendingSleepZone = zone;
        }
    }

    protected void RegisterSleep(SleepZone zone)
    {
        pendingSleepZone = null;
        currentSleepZone = zone;
    }

    protected void UnregisterSleep()
    {
        if (pendingSleepZone != null)
        {
            pendingSleepZone.Unregister(this);
            pendingSleepZone = null;
        }

        if (currentSleepZone != null)
        {
            currentSleepZone.Unregister(this);
            currentSleepZone = null;
        }
    }

    protected SleepZone GetSleepZoneAtPosition(Vector2 pos)
    {
        SleepZone[] zones = FindObjectsOfType<SleepZone>();
        foreach (var zone in zones)
        {
            if (Vector2.Distance(pos, zone.transform.position) < 0.5f)
                return zone;
        }
        return null;
    }

    // ======================================================
    // FIXED UPDATE
    // ======================================================

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
        if (currentPath == null || currentPath.Count == 0 || currentPathIndex >= currentPath.Count)
        {
            currentVelocity = Vector2.zero;
            OnReachedTarget();
            return;
        }

        Vector2 currentWaypoint = currentPath[currentPathIndex];
        Vector2 toTarget = currentWaypoint - rb.position;
        float distance = toTarget.magnitude;

        if (distance < 0.15f)
        {
            currentPathIndex++;
            if (currentPathIndex >= currentPath.Count)
            {
                currentVelocity = Vector2.zero;
                OnReachedTarget();
                return;
            }
            currentWaypoint = currentPath[currentPathIndex];
            toTarget = currentWaypoint - rb.position;
            distance = toTarget.magnitude;
        }

        Vector2 desiredDirection = toTarget.normalized;
        moveDirection = Vector2.Lerp(moveDirection, desiredDirection, turnSpeed * Time.deltaTime).normalized;

        float speedFactor = 1f;
        if (currentPathIndex == currentPath.Count - 1)
        {
            float distToFinalTarget = Vector2.Distance(rb.position, targetPosition);
            speedFactor = Mathf.Clamp01(distToFinalTarget / slowRadius);
        }
        
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
        float distToHome = Vector2.Distance(rb.position, homePosition);
        if (distToHome < 0.3f && !HasWork())
        {
            isAtHome = true;
            StopMovementInstant();
        }

        currentState = BeeState.Idle;
    }

    protected virtual void IdleBehavior()
    {
        if (lockMovement) return;
    }

    protected void StopMovementInstant()
    {
        currentVelocity = Vector2.zero;
        moveDirection = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }

    protected void MarkAsWorking()
    {
        isAtHome = false;
        waitingForSleep = false;
        sleepDelayTimer = 0f;
        UnregisterSleep();
    }

    protected abstract void WorkBehavior();
    protected abstract void ReturnBehavior();

    // ======================================================
    // HEALTH
    // ======================================================

    public void TakeDamage(float amount, Enemy attacker = null)
    {
        currentHealth -= amount;
        if (attacker != null) lastAttacker = attacker;
        if (currentHealth <= 0f) { currentHealth = 0f; Die(); return; }
        if (attacker != null) TryRetaliate(attacker);
    }

    void TryRetaliate(Enemy attacker)
    {
        if (retaliationTimer > 0f) return;

        attacker.TakeDamage(retaliationDamage);
        retaliationTimer = retaliationCooldown;

        Vector2 dirToEnemy = ((Vector2)attacker.transform.position - rb.position).normalized;
        rb.rotation = Mathf.Atan2(dirToEnemy.y, dirToEnemy.x) * Mathf.Rad2Deg;
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
        UnregisterSleep();

      if (lastAttacker != null)
        BeeDeathPopup.Instance.ShowDeath(beeType.ToString(), beeName, lastAttacker.enemyType.ToString());

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
        isAtHome = false;
        waitingForSleep = false;
        sleepDelayTimer = 0f;
        UnregisterSleep();
        PickRandomDirection();
        currentState = BeeState.Idle;
    }

    public virtual void UpgradeSpeed(float amount, float max)
    {
        moveSpeed = Mathf.Min(moveSpeed + amount, max);
    }
}