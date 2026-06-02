using UnityEngine;

public class DroneBee : Bee
{
    private Transform targetEnemy;
    private DroneZone currentZone;

    private float attackTimer;

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

    protected override void Update()
    {
        base.Update();

        if (targetEnemy != null)
        {
            targetLockTimer -= Time.deltaTime;

            if (targetLockTimer <= 0f)
                FindBestEnemy();

            currentZone = null;
            return;
        }

        FindBestEnemy();

        if (targetEnemy == null && currentZone == null)
        {
            currentZone = FindDroneZone();
        }
    }

    protected override void StateUpdate()
    {
        if (currentState == BeeState.Dead)
            return;

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

                currentVelocity = Vector2.zero;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            base.StateUpdate();
            return;
        }

        currentState = BeeState.Idle;
        base.StateUpdate();
    }

    protected override void WorkBehavior()
    {
        if (targetEnemy == null)
        {
            StopMovement();
            return;
        }

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
    }

    void StopMovement()
    {
        currentVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    void Attack()
    {
        if (targetEnemy == null) return;

        Enemy enemy = targetEnemy.GetComponent<Enemy>();

        if (enemy != null)
            enemy.TakeDamage(attackDamage);
    }

    // ======================================================
    // 🧠 TARGETING (WITH QUEEN DEFENSE SYSTEM)
    // ======================================================
    void FindBestEnemy()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        Enemy best = null;
        float bestScore = float.NegativeInfinity;

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;

            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            float threat = enemy.GetThreatLevel();

            float crowdPenalty = enemy.currentDroneAttackers * 50f;

            // -------------------------
            // 🐝 QUEEN DEFENSE PRIORITY
            // -------------------------
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

            float score = threat
                        - dist * 5f
                        + queenBonus
                        - crowdPenalty;

            if (!enemy.CanBeTargetedByDrone())
                continue;

            if (score > bestScore)
            {
                bestScore = score;
                best = enemy;
            }
        }

        // fallback if everything is crowded
        if (best == null)
        {
            foreach (Enemy enemy in enemies)
            {
                if (enemy == null) continue;

                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                float threat = enemy.GetThreatLevel();

                float score = threat - dist * 5f;

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
            if (old != null)
                old.UnregisterDrone();
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
            if (enemy != null)
                enemy.UnregisterDrone();
        }
    }

    DroneZone FindDroneZone()
    {
        if (DroneZone.allZones == null || DroneZone.allZones.Count == 0)
            return null;

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

    protected override void ReturnBehavior() { }

    protected override void OnReachedTarget()
    {
        if (targetEnemy == null && currentZone != null)
        {
            currentState = BeeState.Working;
            return;
        }

        base.OnReachedTarget();
    }
}