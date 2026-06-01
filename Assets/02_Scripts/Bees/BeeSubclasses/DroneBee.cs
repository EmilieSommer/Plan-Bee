using UnityEngine;

public class DroneBee : Bee
{
    private Transform targetEnemy;
    private DroneZone currentZone;

    private float attackTimer;

    [Header("Movement Settings")]
    public float zoneArrivalDistance = 1.5f;

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

        // Enemy always takes priority
        if (targetEnemy != null)
        {
            currentZone = null;
            return;
        }

        // No enemy? Find a zone
        if (currentZone == null)
        {
            currentZone = FindDroneZone();
        }
    }

    protected override void StateUpdate()
    {
        if (currentState == BeeState.Dead)
            return;

        // =========================
        // ENEMY BEHAVIOR
        // =========================
        if (targetEnemy != null)
        {
            float dist = Vector2.Distance(
                transform.position,
                targetEnemy.position
            );

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

        // =========================
        // ZONE BEHAVIOR
        // =========================
        if (currentZone != null)
        {
            float dist = Vector2.Distance(
                transform.position,
                currentZone.transform.position
            );

            if (dist > zoneArrivalDistance)
            {
                currentState = BeeState.Moving;
                targetPosition = currentZone.transform.position;
            }
            else
            {
                // EXACTLY like ForagerBee sheltering:
                // stop completely and wait
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
        // Waiting in zone
        if (targetEnemy == null)
        {
            currentVelocity = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            return;
        }

        // Combat mode
        float dist = Vector2.Distance(
            transform.position,
            targetEnemy.position
        );

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
        if (targetEnemy == null)
            return;

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
            if (enemy == null)
                continue;

            float dist = Vector2.Distance(
                transform.position,
                enemy.transform.position
            );

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
        if (DroneZone.allZones == null ||
            DroneZone.allZones.Count == 0)
        {
            return null;
        }

        DroneZone best = null;
        float bestDist = Mathf.Infinity;

        foreach (var zone in DroneZone.allZones)
        {
            if (zone == null)
                continue;

            float dist = Vector2.Distance(
                transform.position,
                zone.transform.position
            );

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
    }

    protected override void OnReachedTarget()
    {
        // If we reached our zone and there are no enemies,
        // freeze completely.
        if (targetEnemy == null && currentZone != null)
        {
            currentState = BeeState.Working;
            return;
        }

        base.OnReachedTarget();
    }
}