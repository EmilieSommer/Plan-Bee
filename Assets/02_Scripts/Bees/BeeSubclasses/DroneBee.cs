using UnityEngine;

public class DroneBee : Bee
{
    private Transform targetEnemy;
    private DroneZone currentZone;

    [Header("Movement Settings")]
    public float zoneArrivalDistance = 1.5f;

    [Header("Target Priority")]
    public float targetLockTime = 2f;
    private float targetLockTimer;

    protected override void Awake()
    {
        base.Awake();
        beeType = BeeType.Drone;
    }

    protected override void Start()
    {
        base.Start();
        attackTimer = attackCooldown;
    }

    protected override bool HasWork() => true;

    protected override void OnJobFound()
    {
        MarkAsWorking();
        FindBestEnemy();

        if (targetEnemy == null)
        {
            if (assignedZone == null) AssignZone();
            currentZone = assignedZone as DroneZone;
            
            if (currentZone != null)
            {
                targetPosition = currentZone.transform.position;
                currentState = BeeState.Moving;
            }
        }
    }

    protected override void Update()
    {
        base.Update();

        if (targetEnemy != null)
        {
            targetLockTimer -= Time.deltaTime;
            if (targetLockTimer <= 0f)
                FindBestEnemy();

            return;
        }

        FindBestEnemy();

        if (targetEnemy == null && currentZone == null)
        {
            if (assignedZone == null) AssignZone();
            currentZone = assignedZone as DroneZone;
        }
    }

    // ======================================================
    // STATE UPDATE
    // ======================================================

    protected override void StateUpdate()
    {
        if (currentState == BeeState.Dead) return;

        if (targetEnemy != null)
        {
            float dist = Vector2.Distance(transform.position, targetEnemy.position);

            if (dist <= attackRange)
                currentState = BeeState.Working;
            else
            {
                currentState = BeeState.Moving;
                targetPosition = targetEnemy.position;
            }

            base.StateUpdate();
            return;
        }

        if (currentZone != null)
        {
            float dist = Vector2.Distance(transform.position, currentZone.transform.position);

            if (dist > zoneArrivalDistance)
            {
                currentState = BeeState.Moving;
                targetPosition = currentZone.transform.position;
            }
            else
            {
                currentState = BeeState.Working;
                StopMovementInstant();
            }

            base.StateUpdate();
            return;
        }

        currentState = BeeState.Idle;
        base.StateUpdate();
    }

    // ======================================================
    // IDLE
    // ======================================================

    protected override void IdleBehavior()
    {
        FindBestEnemy();
        if (targetEnemy != null) return;

        if (assignedZone == null) AssignZone();
        currentZone = assignedZone as DroneZone;
        
        if (currentZone != null)
        {
            MarkAsWorking();
            targetPosition = currentZone.transform.position;
            currentState = BeeState.Moving;
        }
    }

    // ======================================================
    // WORK
    // ======================================================

    private float patrolTimer;

    protected override void WorkBehavior()
    {
        if (targetEnemy == null)
            FindBestEnemy();

        if (targetEnemy != null)
        {
            if (currentZone != null && Vector2.Distance(targetEnemy.position, currentZone.transform.position) > maxLeashDistance)
            {
                SetTarget(null);
                ReturnToZone();
                return;
            }

            float dist = Vector2.Distance(transform.position, targetEnemy.position);

            if (dist > attackRange)
            {
                currentState = BeeState.Moving;
                targetPosition = targetEnemy.position;
                return;
            }

            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                Attack();
                attackTimer = attackCooldown;
            }

            return;
        }

        if (currentZone != null)
        {
            float dist = Vector2.Distance(transform.position, currentZone.transform.position);
            if (dist > zoneArrivalDistance)
            {
                targetPosition = currentZone.transform.position;
                currentState = BeeState.Moving;
            }
            else
            {
                // Patrol slightly inside the zone so they don't form a singularity blob
                patrolTimer -= Time.deltaTime;
                if (patrolTimer <= 0f)
                {
                    patrolTimer = Random.Range(2f, 5f);
                    Vector2 offset = Random.insideUnitCircle * 0.3f;
                    targetPosition = (Vector2)currentZone.transform.position + offset;
                    currentState = BeeState.Moving;
                }
            }
            return;
        }

        currentState = BeeState.Idle;
    }

    // ======================================================
    // RETURN TO ZONE
    // ======================================================

    void ReturnToZone()
    {
        if (assignedZone == null) AssignZone();
        currentZone = assignedZone as DroneZone;

        if (currentZone != null)
        {
            targetPosition = currentZone.transform.position;
            currentState = BeeState.Moving;
        }
    }

    // ======================================================
    // TARGET REACHED
    // ======================================================

    protected override void OnReachedTarget()
    {
        if (targetEnemy != null)
        {
            currentState = BeeState.Working;
            return;
        }

        if (currentZone != null)
        {
            currentState = BeeState.Working;
            StopMovementInstant();
            return;
        }

        base.OnReachedTarget();
    }

    // ======================================================
    // ATTACK
    // ======================================================

    void Attack()
    {
        if (targetEnemy == null) return;

        Enemy enemy = targetEnemy.GetComponent<Enemy>();
        if (enemy != null)
            enemy.TakeDamage(attackDamage);
    }

    [Header("Drone Leash")]
    public float maxLeashDistance = 10f;

    // ======================================================
    // TARGETING
    // ======================================================

    void FindBestEnemy()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        Enemy best = null;
        float bestScore = float.NegativeInfinity;

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;
            if (!enemy.CanBeTargetedByDrone()) continue;

            if (currentZone != null)
            {
                if (Vector2.Distance(enemy.transform.position, currentZone.transform.position) > maxLeashDistance)
                    continue;
            }

            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            float threat = enemy.GetThreatLevel();
            float crowdPenalty = enemy.currentDroneAttackers * 50f;

            float queenBonus = 0f;
            if (QueenBee.Instance != null)
            {
                float distToQueen = Vector2.Distance(enemy.transform.position, QueenBee.Instance.transform.position);
                if (distToQueen < QueenBee.Instance.protectionRadius)
                {
                    queenBonus = 120f - distToQueen * 10f;
                    queenBonus *= QueenBee.Instance.dangerMultiplier;
                }
            }

            float score = threat - dist * 5f + queenBonus - crowdPenalty;

            if (score > bestScore)
            {
                bestScore = score;
                best = enemy;
            }
        }

        // Fallback: ignore crowd penalty but still respect threat/distance
        if (best == null)
        {
            bestScore = float.NegativeInfinity;

            foreach (Enemy enemy in enemies)
            {
                if (enemy == null) continue;

                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                float score = enemy.GetThreatLevel() - dist * 5f;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = enemy;
                }
            }
        }

        SetTarget(best);
    }

    void SetTarget(Enemy enemy)
    {
        if (targetEnemy != null)
        {
            Enemy old = targetEnemy.GetComponent<Enemy>();
            if (old != null) old.UnregisterDrone();
        }

        if (enemy != null)
        {
            targetEnemy = enemy.transform;
            enemy.RegisterDrone();
            targetLockTimer = targetLockTime;
        }
        else
        {
            targetEnemy = null;
        }
    }



    protected void OnDestroy()
    {
        if (targetEnemy != null)
        {
            Enemy enemy = targetEnemy.GetComponent<Enemy>();
            if (enemy != null) enemy.UnregisterDrone();
        }
    }

    protected override void GoHome()
    {
        // Drones sleep at their assigned DroneZone, not a general SleepZone.
        if (assignedZone == null) AssignZone();
        currentZone = assignedZone as DroneZone;
        
        if (currentZone != null)
        {
            targetPosition = currentZone.transform.position;
            currentState = BeeState.Moving;
        }
        else
        {
            // Fallback if no drone zone exists
            base.GoHome();
        }
    }

    protected override void ReturnBehavior() { }
}