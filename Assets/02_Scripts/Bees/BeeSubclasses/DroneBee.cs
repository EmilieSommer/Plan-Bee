using UnityEngine;

public class DroneBee : Bee
{
    private Transform targetEnemy;
    private float attackTimer;

    protected override void Awake()
    {
        base.Awake();
        beeType = BeeType.Drone;
    }

    protected override void Update()
    {
        base.Update();
        FindNearestEnemy();
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

    protected override void StateUpdate()
    {
        if (currentState == BeeState.Dead)
            return;

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

    protected override void ReturnBehavior() { }

    protected override void Die()
    {
        // custom behavior (e.g. particles)

        base.Die(); // VERY IMPORTANT
    }
}