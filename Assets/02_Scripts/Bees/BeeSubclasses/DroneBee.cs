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

    // ======================================================
    // JOB VALIDATION
    // ======================================================

    protected override bool HasWork()
    {
        // Drones always have a "job" — either hunt enemies or patrol their zone
        return true;
    }

    protected override void OnJobFound()
    {
        // Kicked back to work by heartbeat — re-evaluate targeting
        MarkAsWorking();
        FindBestEnemy();

        if (targetEnemy == null)
        {
            currentZone = FindDroneZone();
            if (currentZone != null)
            {
                targetPosition = currentZone.transform.position;
                currentState = BeeState.Moving;
            }
        }
    }

    // ======================================================
    // UPDATE
    // ======================================================

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
            currentZone = FindDroneZone();
    }

    // ======================================================
    // STATE UPDATE
    // ======================================================

    protected override void StateUpdate()
    {
        if (currentState == BeeState.Dead) return;

        // --- COMBAT MODE ---
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

        // --- PATROL MODE ---
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
                // At zone, stand patrol
                currentState = BeeState.Working;
                StopMovementInstant();
            }

            base.StateUpdate();
            return;
        }

        // No target, no zone → idle (base job system will call OnJobFound shortly)
        currentState = BeeState.Idle;
        base.StateUpdate();
    }

    // ======================================================
    // IDLE
    // ======================================================

    protected override void IdleBehavior()
    {
        // Don't roam — immediately try to find work
        FindBestEnemy();

        if (targetEnemy != null) return;

        currentZone = FindDroneZone();
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

    protected override void WorkBehavior()
    {
        // --- COMBAT ---
        if (targetEnemy != null)
        {
            float dist = Vector2.Distance(transform.position, targetEnemy.position);

            if (dist > attackRange)
            {
                currentState = BeeState.Moving;
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

        // --- PATROL ---
        if (currentZone != null)
        {
            float dist = Vector2.Distance(transform.position, currentZone.transform.position);

            if (dist > zoneArrivalDistance)
            {
                targetPosition = currentZone.transform.position;
                currentState = BeeState.Moving;
            }

            return;
        }

        // Nothing to do
        currentState = BeeState.Idle;
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

        // Fallback: ignore crowd penalty if nothing found
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

    // ======================================================
    // ZONE SEARCH
    // ======================================================

    DroneZone FindDroneZone()
    {
        if (DroneZone.allZones == null || DroneZone.allZones.Count == 0) return null;

        DroneZone best = null;
        float bestDist = Mathf.Infinity;

        foreach (var zone in DroneZone.allZones)
        {
            if (zone == null) continue;

            float dist = Vector2.Distance(transform.position, zone.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = zone;
            }
        }

        return best;
    }

    // ======================================================
    // CLEANUP
    // ======================================================

    protected void OnDestroy()
    {
        if (targetEnemy != null)
        {
            Enemy enemy = targetEnemy.GetComponent<Enemy>();
            if (enemy != null) enemy.UnregisterDrone();
        }
    }

    protected override void ReturnBehavior() { }
}