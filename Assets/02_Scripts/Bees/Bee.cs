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
    public float acceleration = 20f;
    public float turnSpeed = 20f;
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

    protected Animator animator;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (HiveManager.Instance != null)
            HiveManager.Instance.RegisterBee(this);

        attackTimer = 0f;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Disable collisions with each other so they can walk past each other freely
        avoidanceStrength = 0f;
        separationStrength = 0f;

        // Automatically scale the bee to be EXACTLY 25/32 of a tile, ignoring PPU issues!
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();

        if (sr != null && sr.sprite != null)
        {
            float targetSize = 25f / 32f;
            float currentSize = sr.sprite.bounds.size.x; // width in Unity units
            if (currentSize > 0)
            {
                float scale = targetSize / currentSize;
                transform.localScale = new Vector3(scale, scale, 1f);
            }
        }
        else
        {
            transform.localScale = Vector3.one;
        }

        currentHealth = maxHealth;
        currentState = BeeState.Idle;
        homePosition = transform.position;

        PickRandomDirection();
        lastPosition = rb.position;
    }

    protected virtual void Start()
    {
        AssignZone();
        
        // Wait 1 frame so the Queen is guaranteed to have finished her Start() and snapped to the Brood!
        StartCoroutine(SnapToQueenNextFrame());
    }

    private System.Collections.IEnumerator SnapToQueenNextFrame()
    {
        yield return null;
        if (QueenBee.Instance != null && beeType != BeeType.Queen)
        {
            transform.position = QueenBee.Instance.transform.position;
            rb.position = transform.position;
            homePosition = transform.position;
        }
    }

    public void SetName(string newName)
    {
        beeName = string.IsNullOrWhiteSpace(newName) ? "Unnamed Bee" : newName;
        gameObject.name = beeName;
    }

    private float overlapCheckTimer = 0f;

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

        CheckTileOverlap();
        CheckIfStuck();
        StateUpdate();
        UpdateAnimator();
    }

    void CheckTileOverlap()
    {
        if (currentState != BeeState.Idle) return;
        if (isBeingDragged || lockMovement) return;

        overlapCheckTimer += Time.deltaTime;
        if (overlapCheckTimer < 0.5f) return;
        overlapCheckTimer = 0f;

        if (HiveGrid.Instance == null) return;
        Vector3Int myCell = HiveGrid.Instance.WorldToCell(transform.position);
        
        Collider2D[] others = Physics2D.OverlapCircleAll(transform.position, 0.4f);
        foreach (var col in others)
        {
            if (col.gameObject == gameObject) continue;
            if (col.CompareTag("Bee"))
            {
                Bee otherBee = col.GetComponent<Bee>();
                // Only bump if BOTH are idle (don't bump someone who's actively working or moving!)
                if (otherBee != null && otherBee.currentState == BeeState.Idle)
                {
                    Vector3Int otherCell = HiveGrid.Instance.WorldToCell(otherBee.transform.position);
                    if (myCell == otherCell)
                    {
                        // We are sharing a cell and both idle!
                        // The one with the lower Instance ID yields.
                        if (gameObject.GetInstanceID() < otherBee.gameObject.GetInstanceID())
                        {
                            BumpToAdjacentCell();
                            return;
                        }
                    }
                }
            }
        }
    }

    void BumpToAdjacentCell()
    {
        Vector3Int myCell = HiveGrid.Instance.WorldToCell(transform.position);
        Vector3Int[] dirs = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        
        // Shuffle dirs to pick randomly
        for (int i = 0; i < dirs.Length; i++)
        {
            Vector3Int temp = dirs[i];
            int randomIndex = Random.Range(i, dirs.Length);
            dirs[i] = dirs[randomIndex];
            dirs[randomIndex] = temp;
        }

        foreach (var dir in dirs)
        {
            Vector3Int neighbor = myCell + dir;
            if (AStar.IsWalkable(neighbor))
            {
                targetPosition = HiveGrid.Instance.CellToWorld(neighbor);
                currentState = BeeState.Moving;
                isAtHome = false; // Force it to act like it's moving intentionally
                return;
            }
        }
    }

    protected virtual void UpdateAnimator()
    {
        if (animator != null)
        {
            bool isMoving = currentVelocity.sqrMagnitude > 0.01f;
            animator.SetBool("IsMoving", isMoving);
            animator.SetBool("IsWorking", currentState == BeeState.Working);
            animator.SetBool("IsIdle", currentState == BeeState.Idle && !isMoving);
        }
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

        if (distance < 0.05f)
        {
            // SNAP exactly to the tile center so the next movement is a PERFECT cardinal line!
            rb.position = currentWaypoint;
            transform.position = currentWaypoint;

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

        // Snap direction instantly, no curving!
        moveDirection = toTarget.normalized;

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
        Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        Vector2 randomDir = dirs[Random.Range(0, 4)];
        
        moveDirection = randomDir;
        rb.MovePosition(rb.position + moveDirection * 0.2f);
        
        // Don't modify targetPosition because that triggers a completely new AStar path calculation!
    }

    // ======================================================
    // ROTATION
    // ======================================================

    protected void RotateTowardsMovement()
    {
        if (currentVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
            rb.rotation = angle - 90f;
        }
    }

    protected void PickRandomDirection()
    {
        Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        moveDirection = dirs[Random.Range(0, 4)];
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

        // If dropped on dirt, snap them back to their home so they don't get stuck in the walls!
        if (HiveGrid.Instance != null)
        {
            Vector3Int cell = HiveGrid.Instance.WorldToCell(transform.position);
            HiveTileType type = HiveGrid.Instance.GetType(cell);
            if (type == HiveTileType.Hive || type == HiveTileType.Solid)
            {
                transform.position = homePosition;
                rb.position = homePosition;
            }
        }
    }

    public virtual void UpgradeSpeed(float amount, float max)
    {
        moveSpeed = Mathf.Min(moveSpeed + amount, max);
    }
}