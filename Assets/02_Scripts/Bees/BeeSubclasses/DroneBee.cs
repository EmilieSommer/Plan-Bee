using UnityEngine;

public class DroneBee : Bee
{
    private Transform targetEnemy;
    private DroneZone currentZone;

    private float attackTimer;

    [Header("Movement Settings")]
    public float zoneArrivalDistance = 1.5f;

    private Vector2 zoneIdlePosition;

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

        FindNearestEnemy();

        // PRIORITY 1: ENEMY ALWAYS WINS
        if (targetEnemy != null)
        {
            currentZone = null;
            return;
        }

        // PRIORITY 2: FIND ZONE
        if (currentZone == null)
        {
            currentZone = FindDroneZone();
        }
    }

    protected override void StateUpdate()
    {
        if (currentState == BeeState.Dead)
            return;

        // ======================
        // COMBAT MODE
        // ======================
        if (targetEnemy != null)
        {
            float dist = Vector2.Distance(transform.position, targetEnemy.position);

            if (dist <= attackRange)
            {
                currentState = BeeState.Working;
            }
            else
            {
                currentState = BeeState.Moving;
                targetPosition = targetEnemy.position;
            }

            base.StateUpdate();
            return;
        }

        // ======================
        // ZONE MODE (IDLE)
        // ======================
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
                // ✅ IMPORTANT: FREEZE POSITION INSIDE ZONE
                currentState = BeeState.Idle;
                targetPosition = transform.position; // STOP MOVING COMPLETELY

                zoneIdlePosition = transform.position;
            }
        }
        else
        {
            currentState = BeeState.Idle;
            targetPosition = transform.position;
        }

        base.StateUpdate();
    }

    protected override void WorkBehavior()
    {
        if (targetEnemy == null)
        {
            currentState = BeeState.Idle;
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

    void Attack()
    {
        if (targetEnemy == null) return;

        Enemy enemy = targetEnemy.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamage(attackDamage);
        }
    }

    void FindNearestEnemy()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        Enemy closest = null;
        float closestDist = Mathf.Infinity;

        foreach (Enemy enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = enemy;
            }
        }

        targetEnemy = closest != null ? closest.transform : null;
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

    protected override void ReturnBehavior()
    {
        currentZone = null;
        targetEnemy = null;
        targetPosition = transform.position; // IMPORTANT STOP MOVEMENT
    }
}